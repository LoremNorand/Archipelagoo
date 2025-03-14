using UnityEngine;

public class CityCameraController : MonoBehaviour
{
	[Header("��������� �������� ������")]
	[Tooltip("�������� ����������� ������ (WASD)")]
	public float moveSpeed = 10f;

	[Header("��������� �������� ������")]
	[Tooltip("�������� ��������������� �������� (yaw)")]
	public float horizontalRotationSpeed = 100f;
	[Tooltip("�������� ������������� �������� (pitch)")]
	public float verticalRotationSpeed = 80f;
	[Tooltip("���������� �� ��������� ��� ����� �����, ���� Raycast �� ��� ����������")]
	public float defaultPivotDistance = 10f;
	[Tooltip("����������� ���������� ���� ������� (����), �������� -5�")]
	public float minPitch = -5f;
	[Tooltip("������������ ���������� ���� ������� (����), �������� 80�")]
	public float maxPitch = 80f;

	[Header("��������� ����")]
	[Tooltip("�������� ���� (������ ����)")]
	public float zoomSpeed = 5f;
	[Tooltip("����������� ���������� �� ����� �����")]
	public float minZoomDistance = 2f;
	[Tooltip("������������ ���������� �� ����� �����")]
	public float maxZoomDistance = 50f;

	// ���������� ��� ���������� ��������� ������
	private bool isRotating = false;
	private Vector3 pivotPoint;
	private float currentYaw;
	private float currentPitch;
	private float currentDistance;

	void Start()
	{
		// ���� ��������� ������� ��� ������, ������� ��������� ��������� ���� ������������ ����� �����.
		// � ������ ������ � �������� ����� ������� ����� �� ���������� defaultPivotDistance ����� �������.
		pivotPoint = transform.position + transform.forward * defaultPivotDistance;
		Vector3 offset = transform.position - pivotPoint;
		currentDistance = offset.magnitude;
		currentYaw = Mathf.Atan2(offset.x, offset.z) * Mathf.Rad2Deg;
		currentPitch = Mathf.Asin(offset.y / currentDistance) * Mathf.Rad2Deg;
	}

	void Update()
	{
		HandleMovement();
		HandleRotation();
		HandleZoom();
	}

	/// <summary>
	/// ��������� ����������� ������ �� WASD
	/// </summary>
	private void HandleMovement()
	{
		float horizontal = Input.GetAxis("Horizontal");
		float vertical = Input.GetAxis("Vertical");

		// �������� �� ��������� XZ ������������ ����������� ������
		Vector3 forward = transform.forward;
		forward.y = 0;
		forward.Normalize();
		Vector3 right = transform.right;
		right.y = 0;
		right.Normalize();

		Vector3 move = (forward * vertical + right * horizontal) * moveSpeed * Time.deltaTime;
		transform.position += move;
		// ��� ����������� ����� �������� pivotPoint, ���� ���������, ��� �������� ��� �������������
	}

	/// <summary>
	/// ��������� �������� ������ �� ������� ������� ������ ����
	/// </summary>
	private void HandleRotation()
	{
		// ��� ������� ��� ���������� ����� ����� (pivot)
		if(Input.GetMouseButtonDown(2))
		{
			Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
			RaycastHit hit;
			if(Physics.Raycast(ray, out hit))
				pivotPoint = hit.point;
			else
				pivotPoint = ray.origin + ray.direction * defaultPivotDistance;

			// ��������� ��������� ���� � ��������� ������������ pivotPoint
			Vector3 offset = transform.position - pivotPoint;
			currentDistance = offset.magnitude;
			currentYaw = Mathf.Atan2(offset.x, offset.z) * Mathf.Rad2Deg;
			currentPitch = Mathf.Asin(offset.y / currentDistance) * Mathf.Rad2Deg;
			isRotating = true;
		}

		if(Input.GetMouseButtonUp(2))
		{
			isRotating = false;
		}

		if(isRotating)
		{
			float mouseX = Input.GetAxis("Mouse X");
			float mouseY = Input.GetAxis("Mouse Y");

			// ��������� ����
			currentYaw += mouseX * horizontalRotationSpeed * Time.deltaTime;
			currentPitch -= mouseY * verticalRotationSpeed * Time.deltaTime;
			// ������������ ������������ ���� (pitch)
			currentPitch = Mathf.Clamp(currentPitch, minPitch, maxPitch);

			// ������������� ��������� ������ �� ������ ����������� ���������
			Vector3 offset = SphericalToCartesian(currentDistance, currentYaw, currentPitch);
			transform.position = pivotPoint + offset;
			transform.LookAt(pivotPoint);
		}
	}

	/// <summary>
	/// ��������� ���� � ������� ������ ����
	/// </summary>
	private void HandleZoom()
	{
		float scroll = Input.GetAxis("Mouse ScrollWheel");
		if(Mathf.Abs(scroll) > 0.01f)
		{
			// ���� �� �������, ���������� pivotPoint ����������� �������
			if(!isRotating)
			{
				Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
				RaycastHit hit;
				if(Physics.Raycast(ray, out hit))
					pivotPoint = hit.point;
				else
					pivotPoint = ray.origin + ray.direction * defaultPivotDistance;

				// ��������� ��������� ���� � ���������
				Vector3 offset = transform.position - pivotPoint;
				currentDistance = offset.magnitude;
				currentYaw = Mathf.Atan2(offset.x, offset.z) * Mathf.Rad2Deg;
				currentPitch = Mathf.Asin(offset.y / currentDistance) * Mathf.Rad2Deg;
			}

			// ��������� ��������� � ������ ����
			currentDistance -= scroll * zoomSpeed;
			currentDistance = Mathf.Clamp(currentDistance, minZoomDistance, maxZoomDistance);

			// ������������� ��������� ������
			Vector3 newOffset = SphericalToCartesian(currentDistance, currentYaw, currentPitch);
			transform.position = pivotPoint + newOffset;
			transform.LookAt(pivotPoint);
		}
	}

	/// <summary>
	/// ����������� ����������� ���������� � ���������
	/// currentDistance � ������ (����������)
	/// currentYaw � ���� �������� ������ ������������ ��� (� ��������)
	/// currentPitch � ���� ������� (� ��������)
	/// </summary>
	private Vector3 SphericalToCartesian(float radius, float yaw, float pitch)
	{
		float yawRad = yaw * Mathf.Deg2Rad;
		float pitchRad = pitch * Mathf.Deg2Rad;
		float x = radius * Mathf.Sin(yawRad) * Mathf.Cos(pitchRad);
		float y = radius * Mathf.Sin(pitchRad);
		float z = radius * Mathf.Cos(yawRad) * Mathf.Cos(pitchRad);
		return new Vector3(x, y, z);
	}
}

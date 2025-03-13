using UnityEngine;

public class CityCameraController : MonoBehaviour
{
	[Header("��������� �������� ������")]
	[Tooltip("�������� ����������� ������")]
	public float moveSpeed = 10f;
	[Tooltip("�������� �������� ������")]
	public float rotationSpeed = 100f;
	[Tooltip("���������� �� ��������� ��� ����� ��������, ���� Raycast �� ��� ����������")]
	public float defaultPivotDistance = 10f;

	private Vector3 pivotPoint; // �������� ����� �����
	private bool isRotating = false;

	void Update()
	{
		HandleMovement();
		HandleRotation();
	}

	/// <summary>
	/// ��������� ����������� ������ � ������� ������ WASD
	/// </summary>
	private void HandleMovement()
	{
		// �������� ���� �� ����������� � ���������
		float horizontal = Input.GetAxis("Horizontal");
		float vertical = Input.GetAxis("Vertical");

		// ����������� ������������ ����������� ������ (��� ����� ������������� ����������)
		Vector3 forward = transform.forward;
		forward.y = 0;
		forward.Normalize();

		Vector3 right = transform.right;
		right.y = 0;
		right.Normalize();

		// ��������� �������� � ���������� ������
		Vector3 moveDirection = (forward * vertical + right * horizontal) * moveSpeed * Time.deltaTime;
		transform.position += moveDirection;
	}

	/// <summary>
	/// ��������� �������� ������ ��� ��������� ������� ������ ����
	/// </summary>
	private void HandleRotation()
	{
		// ���� ������ ������� ������ ����
		if(Input.GetMouseButtonDown(2))
		{
			// ���������� ����� ����� � ������� Raycast �� ������ ������
			Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
			RaycastHit hit;
			if(Physics.Raycast(ray, out hit))
			{
				pivotPoint = hit.point;
			}
			else
			{
				// ���� ������ �� �������, ������ ����� ����� �� ���������� �� ���������
				pivotPoint = ray.origin + ray.direction * defaultPivotDistance;
			}
			isRotating = true;
		}

		// ���� �������� ������� ������ ����, ���������� ��������
		if(Input.GetMouseButtonUp(2))
		{
			isRotating = false;
		}

		// ���� ������ � ������ ��������
		if(isRotating)
		{
			// �������� ���� ���� ��� ��������
			float mouseX = Input.GetAxis("Mouse X");
			float mouseY = Input.GetAxis("Mouse Y");

			// �������� ������ ������������ ��� (Y) ������������ ����� �����
			transform.RotateAround(pivotPoint, Vector3.up, mouseX * rotationSpeed * Time.deltaTime);

			// ��� ������� ������: ������� ������ ������ ���
			// ����� ����� ���������� ������, ���� ��� ����������
			transform.RotateAround(pivotPoint, transform.right, -mouseY * rotationSpeed * Time.deltaTime);
		}
	}
}

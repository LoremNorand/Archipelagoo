using UnityEngine;

public class CityCameraController : MonoBehaviour
{
	[Header("Настройки движения камеры")]
	[Tooltip("Скорость перемещения камеры (WASD)")]
	public float moveSpeed = 10f;

	[Header("Настройки вращения камеры")]
	[Tooltip("Скорость горизонтального вращения (yaw)")]
	public float horizontalRotationSpeed = 100f;
	[Tooltip("Скорость вертикального вращения (pitch)")]
	public float verticalRotationSpeed = 80f;
	[Tooltip("Расстояние по умолчанию для точки опоры, если Raycast не дал результата")]
	public float defaultPivotDistance = 10f;
	[Tooltip("Минимальный допустимый угол наклона (питч), например -5°")]
	public float minPitch = -5f;
	[Tooltip("Максимальный допустимый угол наклона (питч), например 80°")]
	public float maxPitch = 80f;

	[Header("Настройки зума")]
	[Tooltip("Скорость зума (колесо мыши)")]
	public float zoomSpeed = 5f;
	[Tooltip("Минимальное расстояние до точки опоры")]
	public float minZoomDistance = 2f;
	[Tooltip("Максимальное расстояние до точки опоры")]
	public float maxZoomDistance = 50f;

	// Переменные для управления вращением камеры
	private bool isRotating = false;
	private Vector3 pivotPoint;
	private float currentYaw;
	private float currentPitch;
	private float currentDistance;

	void Start()
	{
		// Если стартовая позиция уже задана, пробуем вычислить начальные углы относительно точки опоры.
		// В данном случае в качестве опоры возьмем точку на расстоянии defaultPivotDistance перед камерой.
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
	/// Обработка перемещения камеры по WASD
	/// </summary>
	private void HandleMovement()
	{
		float horizontal = Input.GetAxis("Horizontal");
		float vertical = Input.GetAxis("Vertical");

		// Движение по плоскости XZ относительно направления камеры
		Vector3 forward = transform.forward;
		forward.y = 0;
		forward.Normalize();
		Vector3 right = transform.right;
		right.y = 0;
		right.Normalize();

		Vector3 move = (forward * vertical + right * horizontal) * moveSpeed * Time.deltaTime;
		transform.position += move;
		// При перемещении можно обновить pivotPoint, если требуется, или оставить его фиксированным
	}

	/// <summary>
	/// Обработка вращения камеры по нажатию средней кнопки мыши
	/// </summary>
	private void HandleRotation()
	{
		// При нажатии СКМ определяем точку опоры (pivot)
		if(Input.GetMouseButtonDown(2))
		{
			Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
			RaycastHit hit;
			if(Physics.Raycast(ray, out hit))
				pivotPoint = hit.point;
			else
				pivotPoint = ray.origin + ray.direction * defaultPivotDistance;

			// Вычисляем начальные углы и дистанцию относительно pivotPoint
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

			// Обновляем углы
			currentYaw += mouseX * horizontalRotationSpeed * Time.deltaTime;
			currentPitch -= mouseY * verticalRotationSpeed * Time.deltaTime;
			// Ограничиваем вертикальный угол (pitch)
			currentPitch = Mathf.Clamp(currentPitch, minPitch, maxPitch);

			// Пересчитываем положение камеры на основе сферических координат
			Vector3 offset = SphericalToCartesian(currentDistance, currentYaw, currentPitch);
			transform.position = pivotPoint + offset;
			transform.LookAt(pivotPoint);
		}
	}

	/// <summary>
	/// Обработка зума с помощью колеса мыши
	/// </summary>
	private void HandleZoom()
	{
		float scroll = Input.GetAxis("Mouse ScrollWheel");
		if(Mathf.Abs(scroll) > 0.01f)
		{
			// Если не вращаем, определяем pivotPoint аналогичным образом
			if(!isRotating)
			{
				Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
				RaycastHit hit;
				if(Physics.Raycast(ray, out hit))
					pivotPoint = hit.point;
				else
					pivotPoint = ray.origin + ray.direction * defaultPivotDistance;

				// Обновляем начальные углы и дистанцию
				Vector3 offset = transform.position - pivotPoint;
				currentDistance = offset.magnitude;
				currentYaw = Mathf.Atan2(offset.x, offset.z) * Mathf.Rad2Deg;
				currentPitch = Mathf.Asin(offset.y / currentDistance) * Mathf.Rad2Deg;
			}

			// Обновляем дистанцию с учётом зума
			currentDistance -= scroll * zoomSpeed;
			currentDistance = Mathf.Clamp(currentDistance, minZoomDistance, maxZoomDistance);

			// Пересчитываем положение камеры
			Vector3 newOffset = SphericalToCartesian(currentDistance, currentYaw, currentPitch);
			transform.position = pivotPoint + newOffset;
			transform.LookAt(pivotPoint);
		}
	}

	/// <summary>
	/// Преобразует сферические координаты в декартовы
	/// currentDistance — радиус (расстояние)
	/// currentYaw — угол поворота вокруг вертикальной оси (в градусах)
	/// currentPitch — угол наклона (в градусах)
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

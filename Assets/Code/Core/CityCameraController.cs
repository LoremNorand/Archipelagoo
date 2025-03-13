using UnityEngine;

public class CityCameraController : MonoBehaviour
{
	[Header("Настройки движения камеры")]
	[Tooltip("Скорость перемещения камеры")]
	public float moveSpeed = 10f;
	[Tooltip("Скорость вращения камеры")]
	public float rotationSpeed = 100f;
	[Tooltip("Расстояние по умолчанию для точки вращения, если Raycast не дал результата")]
	public float defaultPivotDistance = 10f;

	private Vector3 pivotPoint; // Условная точка опоры
	private bool isRotating = false;

	void Update()
	{
		HandleMovement();
		HandleRotation();
	}

	/// <summary>
	/// Обработка перемещения камеры с помощью клавиш WASD
	/// </summary>
	private void HandleMovement()
	{
		// Получаем ввод по горизонтали и вертикали
		float horizontal = Input.GetAxis("Horizontal");
		float vertical = Input.GetAxis("Vertical");

		// Перемещение относительно направления камеры (без учета вертикального компонента)
		Vector3 forward = transform.forward;
		forward.y = 0;
		forward.Normalize();

		Vector3 right = transform.right;
		right.y = 0;
		right.Normalize();

		// Вычисляем смещение и перемещаем камеру
		Vector3 moveDirection = (forward * vertical + right * horizontal) * moveSpeed * Time.deltaTime;
		transform.position += moveDirection;
	}

	/// <summary>
	/// Обработка вращения камеры при удержании средней кнопки мыши
	/// </summary>
	private void HandleRotation()
	{
		// Если нажата средняя кнопка мыши
		if(Input.GetMouseButtonDown(2))
		{
			// Определяем точку опоры с помощью Raycast из центра экрана
			Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
			RaycastHit hit;
			if(Physics.Raycast(ray, out hit))
			{
				pivotPoint = hit.point;
			}
			else
			{
				// Если ничего не найдено, задаем точку опоры на расстоянии по умолчанию
				pivotPoint = ray.origin + ray.direction * defaultPivotDistance;
			}
			isRotating = true;
		}

		// Если отпущена средняя кнопка мыши, прекращаем вращение
		if(Input.GetMouseButtonUp(2))
		{
			isRotating = false;
		}

		// Если камера в режиме вращения
		if(isRotating)
		{
			// Получаем ввод мыши для вращения
			float mouseX = Input.GetAxis("Mouse X");
			float mouseY = Input.GetAxis("Mouse Y");

			// Вращение вокруг вертикальной оси (Y) относительно точки опоры
			transform.RotateAround(pivotPoint, Vector3.up, mouseX * rotationSpeed * Time.deltaTime);

			// Для наклона камеры: вращаем вокруг правой оси
			// Здесь можно ограничить наклон, если это необходимо
			transform.RotateAround(pivotPoint, transform.right, -mouseY * rotationSpeed * Time.deltaTime);
		}
	}
}

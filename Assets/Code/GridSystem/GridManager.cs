using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
	public static GridManager Instance { get; private set; }

	[Header("Настройки сетки размещения")]
	[Tooltip("Префаб ячейки сетки")]
	public GameObject gridCellPrefab;
	[Tooltip("Радиус, в пределах которого ячейки видны (в world-единицах)")]
	public float visibleRadius = 10f;
	[Tooltip("Размер ячейки сетки (8 вокселей * размер вокселя). Например, если voxelSize = 0.1, то cellSize = 0.8")]
	public float cellSize = 0.8f;

	[Header("Параметры генератора (должны совпадать с WorldGenerator)")]
	public int worldWidth;      // количество воксельных ячеек по X (из генератора)
	public int worldHeight;     // количество воксельных ячеек по Y (из генератора)
	public float voxelSize = 0.1f;
	public float noiseScale = 10f;
	[Range(0f, 1f)]
	public float threshold = 0.5f;
	public int seed = 0;

	public GridCell this[Vector3 v3]
	{
		get => gridCells[v3];
	}

	// Список созданных ячеек
	private Dictionary<Vector3, GridCell> gridCells = new Dictionary<Vector3, GridCell>();
	// Точка курсора в мировых координатах
	private Vector3 cursorWorldPos;

	private void Awake()
	{
		if(Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}
		Instance = this;
	}

	// Можно запускать генерацию сетки после генерации террейна, например, через событие или Coroutine
	private IEnumerator Start()
	{
		// Ждем, пока террейн сгенерируется (здесь для примера ждём один кадр)
		yield return null;

		// Если сид не задан, зададим случайный (чтобы совпадало с WorldGenerator)
		if(seed == 0)
			seed = Random.Range(1, 100000);

		// Расчет количества ячеек по сетке.
		// Поскольку размер одной ячейки = cellSize,
		// а общий размер мира = (worldWidth * voxelSize) по X и (worldHeight * voxelSize) по Z,
		// вычисляем число ячеек.
		float worldSizeX = worldWidth * voxelSize;
		float worldSizeZ = worldHeight * voxelSize;
		int numCellsX = Mathf.FloorToInt(worldSizeX / cellSize);
		int numCellsZ = Mathf.FloorToInt(worldSizeZ / cellSize);

		// Генерируем сетку: пробегаем по всем потенциальным ячейкам
		for(int gx = 0; gx < numCellsX; gx++)
		{
			for(int gz = 0; gz < numCellsZ; gz++)
			{
				// Рассчитываем центр ячейки в мировых координатах.
				Vector3 cellCenter = new Vector3(gx * cellSize + cellSize / 2, 0.05f, gz * cellSize + cellSize / 2);
				// Проверяем, что под ячейкой полностью суша
				if(IsAreaLand(cellCenter, cellSize))
				{
					// Создаем ячейку сетки
					GameObject cellGO = Instantiate(gridCellPrefab, cellCenter, Quaternion.identity, transform);
					GridCell cell = cellGO.GetComponent<GridCell>();
					if(cell != null)
					{
						Debug.Log($"Created Coords: {cellCenter.x} {cellCenter.y} {cellCenter.z}");
						cell.Initialize(cellCenter, cellSize);
						gridCells[cellCenter] = cell;
					}
					else
					{
						Debug.Log($"!!! Created Coords: {cellCenter.x} {cellCenter.y} {cellCenter.z}");
					}
				}
			}
		}
	}

	/// <summary>
	/// Проверяет, что в заданной области (ячейки) все углы – суша.
	/// </summary>
	private bool IsAreaLand(Vector3 cellCenter, float size)
	{
		float half = size / 2f;
		// Берем 4 угла ячейки
		Vector3[] corners = new Vector3[4];
		corners[0] = cellCenter + new Vector3(-half, 0, -half);
		corners[1] = cellCenter + new Vector3(half, 0, -half);
		corners[2] = cellCenter + new Vector3(half, 0, half);
		corners[3] = cellCenter + new Vector3(-half, 0, half);

		foreach(var pos in corners)
		{
			// Преобразуем позицию в координаты для функции шума.
			// Поскольку WorldGenerator использует: noiseValue = Mathf.PerlinNoise((worldX + seed)/noiseScale, (worldY + seed)/noiseScale)
			// а pos.x, pos.z – world координаты, преобразуем их в «ячейки»:
			int worldX = Mathf.FloorToInt(pos.x / voxelSize);
			int worldY = Mathf.FloorToInt(pos.z / voxelSize);
			float noiseValue = Mathf.PerlinNoise((worldX + seed) / noiseScale, (worldY + seed) / noiseScale);
			if(noiseValue <= threshold)
				return false;
		}
		return true;
	}

	private void Update()
	{
		UpdateCursorWorldPosition();
		//UpdateGridCellsVisibility();
	}

	/// <summary>
	/// Определяет мировую позицию курсора через Raycast к земле.
	/// </summary>
	private void UpdateCursorWorldPosition()
	{
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		if(Physics.Raycast(ray, out hit))
			cursorWorldPos = hit.point;
	}

	/// <summary>
	/// Обновляет прозрачность ячеек в зависимости от расстояния до курсора.
	/// </summary>
	//private void UpdateGridCellsVisibility()
	//{
	//	foreach(var cell in gridCells)
	//	{
	//		float dist = Vector3.Distance(cursorWorldPos, cell.Center);
	//		float alpha = dist <= visibleRadius ? Mathf.Clamp01(1f - (dist / visibleRadius)) : 0f;
	//		cell.SetAlpha(alpha);
	//	}
	//}

	/// <summary>
	/// Возвращает позицию, выровненную по центру ячеек сетки, с учётом размера здания.
	/// </summary>
	/// <param name="hitPoint">Исходная мировая позиция (например, позиция попадания Raycast)</param>
	/// <param name="buildingGridSize">Размер здания в ячейках сетки (например, 3x2)</param>
	/// <returns>Снэпнутая позиция, соответствующая центру области для размещения здания</returns>
	public Vector3 GetSnappedPosition(Vector3 hitPoint, Vector2Int buildingGridSize)
	{
		// Вычисляем нижнюю левую границу ячейки, в которой находится hitPoint.
		float x = Mathf.Floor(hitPoint.x / cellSize) * cellSize;
		float z = Mathf.Floor(hitPoint.z / cellSize) * cellSize;
		// Смещаем позицию так, чтобы здание с заданным количеством ячеек было выровнено по центру.
		x += (buildingGridSize.x * cellSize) / 2f;
		z += (buildingGridSize.y * cellSize) / 2f;
		// Возвращаем позицию, учитывая, что ячейки располагаются на высоте 0.05f.
		return new Vector3(x, 0.05f, z);
	}

	/// <summary>
	/// Возвращает ячейку сетки, в которой находится позиция pos.
	/// Если такая ячейка не найдена, возвращает null.
	/// </summary>
	public GridCell GetCellAtPosition(Vector3 pos)
	{
		float keyX = Mathf.Floor(pos.x / cellSize) * cellSize + cellSize / 2f;
		float keyZ = Mathf.Floor(pos.z / cellSize) * cellSize + cellSize / 2f;
		Vector3 key = new Vector3(keyX, 0.05f, keyZ);
		GridCell cell = null;
		gridCells.TryGetValue(key, out cell);
		return cell;
	}




}

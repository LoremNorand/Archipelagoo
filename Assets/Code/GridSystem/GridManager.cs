using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
	public static GridManager Instance { get; private set; }

	[Header("��������� ����� ����������")]
	[Tooltip("������ ������ �����")]
	public GameObject gridCellPrefab;
	[Tooltip("������, � �������� �������� ������ ����� (� world-��������)")]
	public float visibleRadius = 10f;
	[Tooltip("������ ������ ����� (8 �������� * ������ �������). ��������, ���� voxelSize = 0.1, �� cellSize = 0.8")]
	public float cellSize = 0.8f;

	[Header("��������� ���������� (������ ��������� � WorldGenerator)")]
	public int worldWidth;      // ���������� ���������� ����� �� X (�� ����������)
	public int worldHeight;     // ���������� ���������� ����� �� Y (�� ����������)
	public float voxelSize = 0.1f;
	public float noiseScale = 10f;
	[Range(0f, 1f)]
	public float threshold = 0.5f;
	public int seed = 0;

	public GridCell this[Vector3 v3]
	{
		get => gridCells[v3];
	}

	// ������ ��������� �����
	private Dictionary<Vector3, GridCell> gridCells = new Dictionary<Vector3, GridCell>();
	// ����� ������� � ������� �����������
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

	// ����� ��������� ��������� ����� ����� ��������� ��������, ��������, ����� ������� ��� Coroutine
	private IEnumerator Start()
	{
		// ����, ���� ������� ������������� (����� ��� ������� ��� ���� ����)
		yield return null;

		// ���� ��� �� �����, ������� ��������� (����� ��������� � WorldGenerator)
		if(seed == 0)
			seed = Random.Range(1, 100000);

		// ������ ���������� ����� �� �����.
		// ��������� ������ ����� ������ = cellSize,
		// � ����� ������ ���� = (worldWidth * voxelSize) �� X � (worldHeight * voxelSize) �� Z,
		// ��������� ����� �����.
		float worldSizeX = worldWidth * voxelSize;
		float worldSizeZ = worldHeight * voxelSize;
		int numCellsX = Mathf.FloorToInt(worldSizeX / cellSize);
		int numCellsZ = Mathf.FloorToInt(worldSizeZ / cellSize);

		// ���������� �����: ��������� �� ���� ������������� �������
		for(int gx = 0; gx < numCellsX; gx++)
		{
			for(int gz = 0; gz < numCellsZ; gz++)
			{
				// ������������ ����� ������ � ������� �����������.
				Vector3 cellCenter = new Vector3(gx * cellSize + cellSize / 2, 0.05f, gz * cellSize + cellSize / 2);
				// ���������, ��� ��� ������� ��������� ����
				if(IsAreaLand(cellCenter, cellSize))
				{
					// ������� ������ �����
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
	/// ���������, ��� � �������� ������� (������) ��� ���� � ����.
	/// </summary>
	private bool IsAreaLand(Vector3 cellCenter, float size)
	{
		float half = size / 2f;
		// ����� 4 ���� ������
		Vector3[] corners = new Vector3[4];
		corners[0] = cellCenter + new Vector3(-half, 0, -half);
		corners[1] = cellCenter + new Vector3(half, 0, -half);
		corners[2] = cellCenter + new Vector3(half, 0, half);
		corners[3] = cellCenter + new Vector3(-half, 0, half);

		foreach(var pos in corners)
		{
			// ����������� ������� � ���������� ��� ������� ����.
			// ��������� WorldGenerator ����������: noiseValue = Mathf.PerlinNoise((worldX + seed)/noiseScale, (worldY + seed)/noiseScale)
			// � pos.x, pos.z � world ����������, ����������� �� � �������:
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
	/// ���������� ������� ������� ������� ����� Raycast � �����.
	/// </summary>
	private void UpdateCursorWorldPosition()
	{
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		if(Physics.Raycast(ray, out hit))
			cursorWorldPos = hit.point;
	}

	/// <summary>
	/// ��������� ������������ ����� � ����������� �� ���������� �� �������.
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
	/// ���������� �������, ����������� �� ������ ����� �����, � ������ ������� ������.
	/// </summary>
	/// <param name="hitPoint">�������� ������� ������� (��������, ������� ��������� Raycast)</param>
	/// <param name="buildingGridSize">������ ������ � ������� ����� (��������, 3x2)</param>
	/// <returns>��������� �������, ��������������� ������ ������� ��� ���������� ������</returns>
	public Vector3 GetSnappedPosition(Vector3 hitPoint, Vector2Int buildingGridSize)
	{
		// ��������� ������ ����� ������� ������, � ������� ��������� hitPoint.
		float x = Mathf.Floor(hitPoint.x / cellSize) * cellSize;
		float z = Mathf.Floor(hitPoint.z / cellSize) * cellSize;
		// ������� ������� ���, ����� ������ � �������� ����������� ����� ���� ��������� �� ������.
		x += (buildingGridSize.x * cellSize) / 2f;
		z += (buildingGridSize.y * cellSize) / 2f;
		// ���������� �������, ��������, ��� ������ ������������� �� ������ 0.05f.
		return new Vector3(x, 0.05f, z);
	}

	/// <summary>
	/// ���������� ������ �����, � ������� ��������� ������� pos.
	/// ���� ����� ������ �� �������, ���������� null.
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

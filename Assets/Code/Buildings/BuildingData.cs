using UnityEngine;

public class BuildingData : MonoBehaviour
{
	[Header("��������� ������")]
	public GameObject buildingPrefab;
	public Vector2Int gridSize = new Vector2Int(2, 2);  // ������ � ������� �����
	public int cost = 100;
	public int resourceCost = 50;
	[Tooltip("������������ �������� ������ �� ������ �������� (��������, 0.05)")]
	public float heightOffset = 0.05f;
}

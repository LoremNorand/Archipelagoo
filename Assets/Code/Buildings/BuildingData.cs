using UnityEngine;

public class BuildingData : MonoBehaviour
{
	[Header("Параметры здания")]
	public GameObject buildingPrefab;
	public Vector2Int gridSize = new Vector2Int(2, 2);  // размер в клетках сетки
	public int cost = 100;
	public int resourceCost = 50;
	[Tooltip("Вертикальное смещение здания от уровня террейна (например, 0.05)")]
	public float heightOffset = 0.05f;
}

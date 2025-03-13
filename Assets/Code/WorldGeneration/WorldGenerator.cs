using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
	[Header("Настройки мира")]
	[Tooltip("Ширина мира (в ячейках)")]
	public int worldWidth = 100;
	[Tooltip("Высота мира (в ячейках)")]
	public int worldHeight = 100;
	[Tooltip("Размер одного вокселя в Unity Unit (0.1 Unit)")]
	public float voxelSize = 0.1f;
	[Tooltip("Масштаб шума Перлина")]
	public float noiseScale = 10f;
	[Tooltip("Порог для разделения суши и воды")]
	[Range(0f, 1f)]
	public float threshold = 0.5f;
	[Tooltip("Размер чанка (кол-во ячеек по стороне)")]
	public int chunkSize = 16;
	[Tooltip("Сид генерации. Если 0, то используется случайное значение")]
	public int seed = 0;

	[Header("Материалы (опционально)")]
	public Material landMaterial;
	public Material waterMaterial;

	private void Start()
	{
		// Если сид не задан (0), генерируем случайный
		if(seed == 0)
			seed = Random.Range(1, 100000);

		GenerateWorld();
	}

	/// <summary>
	/// Генерирует мир, деля его на чанки.
	/// </summary>
	private void GenerateWorld()
	{
		int numChunksX = Mathf.CeilToInt((float)worldWidth / chunkSize);
		int numChunksY = Mathf.CeilToInt((float)worldHeight / chunkSize);

		for(int cx = 0; cx < numChunksX; cx++)
		{
			for(int cy = 0; cy < numChunksY; cy++)
			{
				CreateChunk(cx, cy);
			}
		}
	}

	/// <summary>
	/// Создаёт чанк и генерирует его Mesh.
	/// </summary>
	/// <param name="chunkX">Индекс чанка по X</param>
	/// <param name="chunkY">Индекс чанка по Y</param>
	private void CreateChunk(int chunkX, int chunkY)
	{
		// Создаем объект чанка и делаем его дочерним текущему объекту
		GameObject chunkGO = new GameObject("Chunk_" + chunkX + "_" + chunkY);
		chunkGO.transform.parent = transform;
		// Позиция чанка рассчитывается исходя из его размеров и размера вокселя
		chunkGO.transform.position = new Vector3(chunkX * chunkSize * voxelSize, 0, chunkY * chunkSize * voxelSize);

		MeshFilter mf = chunkGO.AddComponent<MeshFilter>();
		MeshRenderer mr = chunkGO.AddComponent<MeshRenderer>();

		Mesh mesh = new Mesh();
		List<Vector3> vertices = new List<Vector3>();
		List<int> triangles = new List<int>();
		List<Vector2> uvs = new List<Vector2>();
		List<Color> colors = new List<Color>(); // Используем для покраски: суша – зелёная, вода – синяя

		int vertIndex = 0;
		// Проходим по каждой ячейке в чанке
		for(int x = 0; x < chunkSize; x++)
		{
			for(int y = 0; y < chunkSize; y++)
			{
				// Определяем позицию ячейки в мировых координатах
				int worldX = chunkX * chunkSize + x;
				int worldY = chunkY * chunkSize + y;
				// Вычисляем значение шума
				float noiseValue = Mathf.PerlinNoise((worldX + seed) / noiseScale, (worldY + seed) / noiseScale);
				bool isLand = noiseValue > threshold;
				Color cellColor = isLand ? Color.green : Color.blue;
				// Задаем высоту: для суши можно задать небольшой подъем, для воды – нулевая высота или чуть ниже
				float cellHeight = isLand ? voxelSize : 0f;

				// Создаем простой квад с вершинами, соответствующий ячейке
				vertices.Add(new Vector3(x * voxelSize, cellHeight, y * voxelSize));
				vertices.Add(new Vector3((x + 1) * voxelSize, cellHeight, y * voxelSize));
				vertices.Add(new Vector3((x + 1) * voxelSize, cellHeight, (y + 1) * voxelSize));
				vertices.Add(new Vector3(x * voxelSize, cellHeight, (y + 1) * voxelSize));

				// Простые UV для текстурирования
				uvs.Add(new Vector2(0, 0));
				uvs.Add(new Vector2(1, 0));
				uvs.Add(new Vector2(1, 1));
				uvs.Add(new Vector2(0, 1));

				// Задаем цвет каждой вершины для визуализации
				colors.Add(cellColor);
				colors.Add(cellColor);
				colors.Add(cellColor);
				colors.Add(cellColor);

				// Добавляем два треугольника для квадрата
				triangles.Add(vertIndex);
				triangles.Add(vertIndex + 2);
				triangles.Add(vertIndex + 1);
				triangles.Add(vertIndex);
				triangles.Add(vertIndex + 3);
				triangles.Add(vertIndex + 2);
				vertIndex += 4;
			}
		}

		mesh.vertices = vertices.ToArray();
		mesh.triangles = triangles.ToArray();
		mesh.uv = uvs.ToArray();
		mesh.colors = colors.ToArray();
		mesh.RecalculateNormals();

		mf.mesh = mesh;
		// Используем стандартный шейдер, который может работать с цветовыми атрибутами вершин
		mr.material = new Material(Shader.Find("Standard"));
		// При необходимости можно сменить материал на landMaterial/waterMaterial
	}
}

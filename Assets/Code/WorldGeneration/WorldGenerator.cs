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

	private bool IsCellLand(int chunkX, int chunkY, int localX, int localY)
	{
		// Если выходим за границы чанка, считаем, что там "не суша" (чтобы создавать боковую грань)
		if(localX < 0 || localX >= chunkSize || localY < 0 || localY >= chunkSize)
			return false;

		int worldX = chunkX * chunkSize + localX;
		int worldY = chunkY * chunkSize + localY;

		float noiseValue = Mathf.PerlinNoise((worldX + seed) / noiseScale, (worldY + seed) / noiseScale);
		return (noiseValue > threshold);
	}

	/// <summary>
	/// Добавляет в общие списки вершины и треугольники для квадрата (двух треугольников).
	/// corners: массив из 4-х углов квадрата в порядке обхода (0→1→2→3).
	/// targetTriangles: список треугольников, в который добавляем (landTriangles или waterTriangles).
	/// vertList, uvList, colorList — общие списки, куда добавляются данные.
	/// </summary>
	private void AddQuad(
		Vector3[] corners,
		List<int> targetTriangles,
		List<Vector3> vertList,
		List<Vector2> uvList,
		List<Color> colorList,
		Color quadColor
	)
	{
		int startIndex = vertList.Count;

		// Добавляем вершины
		for(int i = 0; i < 4; i++)
		{
			vertList.Add(corners[i]);
			uvList.Add(new Vector2(corners[i].x, corners[i].z)); // Условный UV
			colorList.Add(quadColor);
		}

		// Два треугольника
		targetTriangles.Add(startIndex);
		targetTriangles.Add(startIndex + 2);
		targetTriangles.Add(startIndex + 1);

		targetTriangles.Add(startIndex);
		targetTriangles.Add(startIndex + 3);
		targetTriangles.Add(startIndex + 2);
	}

	/// <summary>
	/// Создаёт чанк и генерирует его Mesh с двумя субмешами: для суши и для воды.
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

		// Общие списки вершин, UV и цветов
		List<Vector3> vertices = new List<Vector3>();
		List<Vector2> uvs = new List<Vector2>();
		List<Color> colors = new List<Color>();

		// Списки треугольников для суши и воды
		List<int> landTriangles = new List<int>();
		List<int> waterTriangles = new List<int>();

		// Перебираем ячейки чанка
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
				// Задаем высоту: для суши небольшой подъем, для воды – нулевая высота
				float cellHeight = isLand ? voxelSize : 0f;

				// Запоминаем начальный индекс вершин для данной ячейки
				int vertIndex = vertices.Count;

				// Создаем вершины квадрата (ячейки)
				vertices.Add(new Vector3(x * voxelSize, cellHeight, y * voxelSize));
				vertices.Add(new Vector3((x + 1) * voxelSize, cellHeight, y * voxelSize));
				vertices.Add(new Vector3((x + 1) * voxelSize, cellHeight, (y + 1) * voxelSize));
				vertices.Add(new Vector3(x * voxelSize, cellHeight, (y + 1) * voxelSize));

				// Простой UV для текстурирования
				uvs.Add(new Vector2(0, 0));
				uvs.Add(new Vector2(1, 0));
				uvs.Add(new Vector2(1, 1));
				uvs.Add(new Vector2(0, 1));

				// Можно задать цвет для отладки (будет использоваться, если шейдер учитывает vertex color)
				Color cellColor = isLand ? Color.green : Color.blue;
				colors.Add(cellColor);
				colors.Add(cellColor);
				colors.Add(cellColor);
				colors.Add(cellColor);

				// Два треугольника для квадрата
				int[] cellTriangles = new int[]
				{
					vertIndex, vertIndex + 2, vertIndex + 1,
					vertIndex, vertIndex + 3, vertIndex + 2
				};

				// Добавляем треугольники в соответствующий список
				if(isLand)
				{
					landTriangles.AddRange(cellTriangles);
				}
				else
				{
					waterTriangles.AddRange(cellTriangles);
				}

				if(isLand && cellHeight > 0f)
				{
					// Четыре направления: left, right, back, forward
					// 1) LEFT (x-1)
					if(!IsCellLand(chunkX, chunkY, x - 1, y))
					{
						// Квадрат от высоты 0 до cellHeight
						Vector3[] corners = new Vector3[4];
						corners[0] = new Vector3(x * voxelSize, 0, y * voxelSize);
						corners[1] = new Vector3(x * voxelSize, cellHeight, y * voxelSize);
						corners[2] = new Vector3(x * voxelSize, cellHeight, (y + 1) * voxelSize);
						corners[3] = new Vector3(x * voxelSize, 0, (y + 1) * voxelSize);

						AddQuad(corners, landTriangles, vertices, uvs, colors, Color.green);
					}

					// 2) RIGHT (x+1)
					if(!IsCellLand(chunkX, chunkY, x + 1, y))
					{
						Vector3[] corners = new Vector3[4];
						corners[0] = new Vector3((x + 1) * voxelSize, 0, (y + 1) * voxelSize);
						corners[1] = new Vector3((x + 1) * voxelSize, cellHeight, (y + 1) * voxelSize);
						corners[2] = new Vector3((x + 1) * voxelSize, cellHeight, y * voxelSize);
						corners[3] = new Vector3((x + 1) * voxelSize, 0, y * voxelSize);

						AddQuad(corners, landTriangles, vertices, uvs, colors, Color.green);
					}

					// 3) BACK (y-1)
					if(!IsCellLand(chunkX, chunkY, x, y - 1))
					{
						Vector3[] corners = new Vector3[4];
						corners[0] = new Vector3((x + 1) * voxelSize, 0, y * voxelSize);
						corners[1] = new Vector3((x + 1) * voxelSize, cellHeight, y * voxelSize);
						corners[2] = new Vector3(x * voxelSize, cellHeight, y * voxelSize);
						corners[3] = new Vector3(x * voxelSize, 0, y * voxelSize);

						AddQuad(corners, landTriangles, vertices, uvs, colors, Color.green);
					}

					// 4) FORWARD (y+1)
					if(!IsCellLand(chunkX, chunkY, x, y + 1))
					{
						Vector3[] corners = new Vector3[4];
						corners[0] = new Vector3(x * voxelSize, 0, (y + 1) * voxelSize);
						corners[1] = new Vector3(x * voxelSize, cellHeight, (y + 1) * voxelSize);
						corners[2] = new Vector3((x + 1) * voxelSize, cellHeight, (y + 1) * voxelSize);
						corners[3] = new Vector3((x + 1) * voxelSize, 0, (y + 1) * voxelSize);

						AddQuad(corners, landTriangles, vertices, uvs, colors, Color.green);
					}
				}
			}
		}



		// Назначаем общие вершины, UV и цвета в Mesh
		mesh.vertices = vertices.ToArray();
		mesh.uv = uvs.ToArray();
		mesh.colors = colors.ToArray();

		// Указываем, что будет два субмеша: [0] - суша, [1] - вода
		mesh.subMeshCount = 2;
		mesh.SetTriangles(landTriangles, 0);
		mesh.SetTriangles(waterTriangles, 1);
		mesh.RecalculateNormals();

		mf.mesh = mesh;

		// Если материалы не назначены, используем стандартный шейдер
		if(landMaterial == null)
			landMaterial = new Material(Shader.Find("Standard"));
		if(waterMaterial == null)
			waterMaterial = new Material(Shader.Find("Standard"));

		// Назначаем два материала: первый для суши, второй для воды
		mr.materials = new Material[] { landMaterial, waterMaterial };
	}
}

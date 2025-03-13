using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
	[Header("��������� ����")]
	[Tooltip("������ ���� (� �������)")]
	public int worldWidth = 100;
	[Tooltip("������ ���� (� �������)")]
	public int worldHeight = 100;
	[Tooltip("������ ������ ������� � Unity Unit (0.1 Unit)")]
	public float voxelSize = 0.1f;
	[Tooltip("������� ���� �������")]
	public float noiseScale = 10f;
	[Tooltip("����� ��� ���������� ���� � ����")]
	[Range(0f, 1f)]
	public float threshold = 0.5f;
	[Tooltip("������ ����� (���-�� ����� �� �������)")]
	public int chunkSize = 16;
	[Tooltip("��� ���������. ���� 0, �� ������������ ��������� ��������")]
	public int seed = 0;

	[Header("��������� (�����������)")]
	public Material landMaterial;
	public Material waterMaterial;

	private void Start()
	{
		// ���� ��� �� ����� (0), ���������� ���������
		if(seed == 0)
			seed = Random.Range(1, 100000);

		GenerateWorld();
	}

	/// <summary>
	/// ���������� ���, ���� ��� �� �����.
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
	/// ������ ���� � ���������� ��� Mesh.
	/// </summary>
	/// <param name="chunkX">������ ����� �� X</param>
	/// <param name="chunkY">������ ����� �� Y</param>
	private void CreateChunk(int chunkX, int chunkY)
	{
		// ������� ������ ����� � ������ ��� �������� �������� �������
		GameObject chunkGO = new GameObject("Chunk_" + chunkX + "_" + chunkY);
		chunkGO.transform.parent = transform;
		// ������� ����� �������������� ������ �� ��� �������� � ������� �������
		chunkGO.transform.position = new Vector3(chunkX * chunkSize * voxelSize, 0, chunkY * chunkSize * voxelSize);

		MeshFilter mf = chunkGO.AddComponent<MeshFilter>();
		MeshRenderer mr = chunkGO.AddComponent<MeshRenderer>();

		Mesh mesh = new Mesh();
		List<Vector3> vertices = new List<Vector3>();
		List<int> triangles = new List<int>();
		List<Vector2> uvs = new List<Vector2>();
		List<Color> colors = new List<Color>(); // ���������� ��� ��������: ���� � ������, ���� � �����

		int vertIndex = 0;
		// �������� �� ������ ������ � �����
		for(int x = 0; x < chunkSize; x++)
		{
			for(int y = 0; y < chunkSize; y++)
			{
				// ���������� ������� ������ � ������� �����������
				int worldX = chunkX * chunkSize + x;
				int worldY = chunkY * chunkSize + y;
				// ��������� �������� ����
				float noiseValue = Mathf.PerlinNoise((worldX + seed) / noiseScale, (worldY + seed) / noiseScale);
				bool isLand = noiseValue > threshold;
				Color cellColor = isLand ? Color.green : Color.blue;
				// ������ ������: ��� ���� ����� ������ ��������� ������, ��� ���� � ������� ������ ��� ���� ����
				float cellHeight = isLand ? voxelSize : 0f;

				// ������� ������� ���� � ���������, ��������������� ������
				vertices.Add(new Vector3(x * voxelSize, cellHeight, y * voxelSize));
				vertices.Add(new Vector3((x + 1) * voxelSize, cellHeight, y * voxelSize));
				vertices.Add(new Vector3((x + 1) * voxelSize, cellHeight, (y + 1) * voxelSize));
				vertices.Add(new Vector3(x * voxelSize, cellHeight, (y + 1) * voxelSize));

				// ������� UV ��� ���������������
				uvs.Add(new Vector2(0, 0));
				uvs.Add(new Vector2(1, 0));
				uvs.Add(new Vector2(1, 1));
				uvs.Add(new Vector2(0, 1));

				// ������ ���� ������ ������� ��� ������������
				colors.Add(cellColor);
				colors.Add(cellColor);
				colors.Add(cellColor);
				colors.Add(cellColor);

				// ��������� ��� ������������ ��� ��������
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
		// ���������� ����������� ������, ������� ����� �������� � ��������� ���������� ������
		mr.material = new Material(Shader.Find("Standard"));
		// ��� ������������� ����� ������� �������� �� landMaterial/waterMaterial
	}
}

using UnityEngine;

public class GridCell : MonoBehaviour
{
	// ����� ������ (��� ������� ���������)
	public Vector3 Center { get; private set; }
	public float CellSize { get; private set; }

	// ���������, ������� �������� �� ��������� ������ (��������, SpriteRenderer ��� MeshRenderer)
	private Renderer cellRenderer;
	// �������� ���� ������ (� ����� = 1)
	private Color baseColor = Color.cyan;

	public void Initialize(Vector3 center, float cellSize)
	{
		Center = center;
		CellSize = cellSize;
		transform.position = new Vector3(center.x, center.y + 0.05f, center.z);
		transform.localScale = new Vector3(cellSize, 1, cellSize);

		cellRenderer = GetComponent<Renderer>();
		//if(cellRenderer != null)
		//{
		//	// ������� ��������� ���������, ����� ������ ����� ����������
		//	cellRenderer.material = new Material(cellRenderer.material);
		//	cellRenderer.material.color = baseColor;
		//}

		if(cellRenderer != null)
		{
			// ������� ��������� ��������� ��� ������������ ����������
			Material mat = new Material(cellRenderer.material);
			SetMaterialTransparent(mat);
			mat.color = baseColor;
			cellRenderer.material = mat;
		}
	}

	private void SetMaterialTransparent(Material mat)
	{
		// ��� ������������ ������� ������������� ����� ������������
		mat.SetFloat("_Mode", 3f);
		mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
		mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
		mat.SetInt("_ZWrite", 0);
		mat.DisableKeyword("_ALPHATEST_ON");
		mat.EnableKeyword("_ALPHABLEND_ON");
		mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
		mat.renderQueue = 3000;
	}


	/// <summary>
	/// ������������� ������������ ������.
	/// </summary>
	public void SetAlpha(float alpha)
	{
		if(cellRenderer != null)
		{
			Color c = baseColor;
			c.a = alpha;
			cellRenderer.material.color = c;
		}
	}
}

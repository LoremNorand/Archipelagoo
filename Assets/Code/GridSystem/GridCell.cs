using UnityEngine;

public class GridCell : MonoBehaviour
{
	// ÷ентр €чейки (дл€ расчЄта видимости)
	public Vector3 Center { get; private set; }
	public float CellSize { get; private set; }

	//  омпонент, который отвечает за отрисовку €чейки (например, SpriteRenderer или MeshRenderer)
	private Renderer cellRenderer;
	// »сходный цвет €чейки (с альфа = 1)
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
		//	// —оздаем экземпл€р материала, чтобы мен€ть альфа независимо
		//	cellRenderer.material = new Material(cellRenderer.material);
		//	cellRenderer.material.color = baseColor;
		//}

		if(cellRenderer != null)
		{
			// —оздаем экземпл€р материала дл€ независимого управлени€
			Material mat = new Material(cellRenderer.material);
			SetMaterialTransparent(mat);
			mat.color = baseColor;
			cellRenderer.material = mat;
		}
	}

	private void SetMaterialTransparent(Material mat)
	{
		// ƒл€ стандартного шейдера устанавливаем режим прозрачности
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
	/// ”станавливает прозрачность €чейки.
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

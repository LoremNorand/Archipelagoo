using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BuildingPlacementManager : MonoBehaviour
{
	public static BuildingPlacementManager Instance { get; private set; }

	[Header("��������� UI")]
	[Tooltip("������ ����������� � ���������� �������/��������")]
	public GameObject insufficientFundsPopup; // ������ ��������� ��������� Text

	// ���� ��� �������� ������, ������� ��������
	private GameObject ghostBuilding;
	private BuildingData currentBuildingData;
	private GameObject currentBuildingPrefab;
	private bool isPlacing = false;

	// ����� ghost-������: ���������� � ��� ������ ����������
	private Color validColor = new Color(0, 1, 0, 0.5f); // �������������� �������
	private Color invalidColor = new Color(1, 0, 0, 0.5f); // �������������� �������

	private void Awake()
	{
		if(Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}
		Instance = this;
	}

	private void Update()
	{
		if(isPlacing && ghostBuilding != null)
		{
			UpdateGhostPosition();
			HandleInput();
		}
	}

	/// <summary>
	/// ��������� ����� ���������� ������ ��� ���������� ����.
	/// ���������� �� UI ����� ������ ��� ����������.
	/// </summary>
	public void StartPlacingBuilding(BuildingData buildingData)
	{
		// ��������� ������ �������� ������
		currentBuildingData = buildingData;
		currentBuildingPrefab = buildingData.buildingPrefab;

		// �������� �������
		float budget = CityStatsManager.Instance.GetStat("Budget");
		float resources = CityStatsManager.Instance.GetStat("Resources");

		if(budget < currentBuildingData.cost || resources < currentBuildingData.resourceCost)
		{
			Debug.Log("������������ ������� ��� ��������� ������.");
			StartCoroutine(ShowPopup("������������ ����� ��� ����������!", 4f));
			return;
		}

		// ������� ghost-������
		ghostBuilding = Instantiate(currentBuildingPrefab);
		ApplyGhostEffect(ghostBuilding, validColor);
		isPlacing = true;
		Debug.Log("������ ���������� ������. Ghost-������ �������.");
	}

	/// <summary>
	/// ��������� ������� ghost-������: ��� ������ ��������� �� ��������,
	/// ��������� � ����� � ������������� �� ������ � ������ heightOffset.
	/// </summary>
	private void UpdateGhostPosition()
	{
		Plane placementPlane = new Plane(Vector3.up, Vector3.zero);
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		float enter;
		if(placementPlane.Raycast(ray, out enter))
		{
			Vector3 hitPoint = ray.GetPoint(enter);
			Vector3 snappedPos = GridManager.Instance.GetSnappedPosition(hitPoint, currentBuildingData.gridSize);
			snappedPos.y += currentBuildingData.heightOffset;
			ghostBuilding.transform.position = snappedPos;

			if(CanPlaceBuilding())
			{
				UpdateGhostColor(validColor);
			}
			else
			{
				UpdateGhostColor(invalidColor);
			}
		}
	}

	/// <summary>
	/// ��������� ����� ��� ghost-������:
	/// ��� � ������������� ����������,
	/// ��� ��� Escape � ������,
	/// E � Q � �������.
	/// </summary>
	private void HandleInput()
	{
		if(Input.GetKeyDown(KeyCode.E))
		{
			ghostBuilding.transform.Rotate(0, 90, 0);
			Debug.Log("������� ������ �� ������� �������.");
		}
		if(Input.GetKeyDown(KeyCode.Q))
		{
			ghostBuilding.transform.Rotate(0, -90, 0);
			Debug.Log("������� ������ ������ ������� �������.");
		}

		// ������������� ���������� � ���
		if(Input.GetMouseButtonDown(0))
		{
			if(CanPlaceBuilding())
			{
				PlaceBuilding();
			}
			else
			{
				Debug.Log("���������� ���������� ������ � ������ �������.");
				StartCoroutine(ShowPopup("������ ��������� �����!", 4f));
			}
		}

		// ������ ���������� � ��� ��� Escape
		if(Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
		{
			Debug.Log("���������� ������ ��������.");
			CancelPlacement();
		}
	}

	/// <summary>
	/// ���������, ����� �� ���������� ������ � ������� �������.
	/// ��������� ������� ������� �����, ����� ���������� ������ ����� GridManager.
	/// </summary>
	private bool CanPlaceBuilding()
	{
		float cellSize = GridManager.Instance.cellSize;
		// ghostBuilding.transform.position ��������� ������� ����� �����
		Vector3 center = ghostBuilding.transform.position;
		Vector3 blockOrigin = center - new Vector3(((currentBuildingData.gridSize.x - 1) * cellSize) / 2f, 0, ((currentBuildingData.gridSize.y - 1) * cellSize) / 2f);

		for(int x = 0; x < currentBuildingData.gridSize.x; x++)
		{
			for(int z = 0; z < currentBuildingData.gridSize.y; z++)
			{
				Vector3 cellCenter = blockOrigin + new Vector3(x * cellSize, 0, z * cellSize);
				cellCenter.y = 0.05f;
				GridCell cell = GridManager.Instance.GetCellAtPosition(cellCenter);
				if(cell == null)
				{
					Debug.Log($"ERR Coords: {cellCenter.x} {cellCenter.y} {cellCenter.z}");
					return false;
				}
				// ����� �������� �������� ��������� ������, ���� �����������.
			}
		}
		return true;
	}

	/// <summary>
	/// ��������� ������, ��������� �������� � ������ ��������� ������.
	/// </summary>
	private void PlaceBuilding()
	{
		CityStatsManager.Instance.SafeModifyStat("Budget", -currentBuildingData.cost);
		CityStatsManager.Instance.SafeModifyStat("Resources", -currentBuildingData.resourceCost);

		GameObject finalBuilding = Instantiate(currentBuildingPrefab, ghostBuilding.transform.position, ghostBuilding.transform.rotation);
		Debug.Log("������ ������� ��������� �� �������: " + ghostBuilding.transform.position);
		CancelPlacement();
	}

	/// <summary>
	/// ������ ����������: ���������� ghost-������ � ��������� ����� �������������.
	/// </summary>
	public void CancelPlacement()
	{
		isPlacing = false;
		if(ghostBuilding != null)
			Destroy(ghostBuilding);
		Debug.Log("����� ���������� ��������.");
	}

	/// <summary>
	/// ��������� ghost-������ � �������: ������������� �������� � �������� ������ � �������������.
	/// </summary>
	private void ApplyGhostEffect(GameObject obj, Color ghostColor)
	{
		Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
		foreach(Renderer r in renderers)
		{
			Material mat = new Material(r.material);
			mat.color = ghostColor;
			SetMaterialTransparent(mat);
			r.material = mat;
		}
	}

	/// <summary>
	/// ��������� ���� ghost-������.
	/// </summary>
	private void UpdateGhostColor(Color newColor)
	{
		Renderer[] renderers = ghostBuilding.GetComponentsInChildren<Renderer>();
		foreach(Renderer r in renderers)
		{
			if(r.material.color != newColor)
			{
				r.material.color = newColor;
			}
		}
	}

	/// <summary>
	/// ������������� �������� � ����� ������������.
	/// </summary>
	private void SetMaterialTransparent(Material mat)
	{
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
	/// ������� popup-����������� � ���������� �� �������� �����.
	/// </summary>
	private IEnumerator ShowPopup(string message, float duration)
	{
		if(insufficientFundsPopup != null)
		{
			Text popupText = insufficientFundsPopup.GetComponentInChildren<Text>();
			if(popupText != null)
				popupText.text = message;
			insufficientFundsPopup.SetActive(true);
			yield return new WaitForSeconds(duration);
			insufficientFundsPopup.SetActive(false);
		}
		yield break;
	}
}

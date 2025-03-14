using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BuildingPlacementManager : MonoBehaviour
{
	public static BuildingPlacementManager Instance { get; private set; }

	[Header("Настройки UI")]
	[Tooltip("Панель уведомления о недостатке средств/ресурсов")]
	public GameObject insufficientFundsPopup; // должен содержать компонент Text

	// Поля для текущего здания, которое строится
	private GameObject ghostBuilding;
	private BuildingData currentBuildingData;
	private GameObject currentBuildingPrefab;
	private bool isPlacing = false;

	// Цвета ghost-модели: нормальный и при ошибке размещения
	private Color validColor = new Color(0, 1, 0, 0.5f); // полупрозрачный зеленый
	private Color invalidColor = new Color(1, 0, 0, 0.5f); // полупрозрачный красный

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
	/// Запускает режим размещения здания для выбранного типа.
	/// Вызывается из UI через обёртку без параметров.
	/// </summary>
	public void StartPlacingBuilding(BuildingData buildingData)
	{
		// Сохраняем данные текущего здания
		currentBuildingData = buildingData;
		currentBuildingPrefab = buildingData.buildingPrefab;

		// Проверка средств
		float budget = CityStatsManager.Instance.GetStat("Budget");
		float resources = CityStatsManager.Instance.GetStat("Resources");

		if(budget < currentBuildingData.cost || resources < currentBuildingData.resourceCost)
		{
			Debug.Log("Недостаточно средств для постройки здания.");
			StartCoroutine(ShowPopup("Недостаточно денег или материалов!", 4f));
			return;
		}

		// Создаем ghost-модель
		ghostBuilding = Instantiate(currentBuildingPrefab);
		ApplyGhostEffect(ghostBuilding, validColor);
		isPlacing = true;
		Debug.Log("Начало размещения здания. Ghost-модель создана.");
	}

	/// <summary>
	/// Обновляет позицию ghost-модели: она должна следовать за курсором,
	/// снэпиться к сетке и выравниваться по высоте с учетом heightOffset.
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
	/// Обработка ввода для ghost-модели:
	/// ЛКМ – подтверждение размещения,
	/// ПКМ или Escape – отмена,
	/// E и Q – поворот.
	/// </summary>
	private void HandleInput()
	{
		if(Input.GetKeyDown(KeyCode.E))
		{
			ghostBuilding.transform.Rotate(0, 90, 0);
			Debug.Log("Поворот здания по часовой стрелке.");
		}
		if(Input.GetKeyDown(KeyCode.Q))
		{
			ghostBuilding.transform.Rotate(0, -90, 0);
			Debug.Log("Поворот здания против часовой стрелки.");
		}

		// Подтверждение размещения – ЛКМ
		if(Input.GetMouseButtonDown(0))
		{
			if(CanPlaceBuilding())
			{
				PlaceBuilding();
			}
			else
			{
				Debug.Log("Невозможно разместить здание в данной позиции.");
				StartCoroutine(ShowPopup("Нельзя построить здесь!", 4f));
			}
		}

		// Отмена размещения – ПКМ или Escape
		if(Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
		{
			Debug.Log("Размещение здания отменено.");
			CancelPlacement();
		}
	}

	/// <summary>
	/// Проверяет, можно ли разместить здание в текущей позиции.
	/// Вычисляет базовую позицию блока, затем перебирает ячейки через GridManager.
	/// </summary>
	private bool CanPlaceBuilding()
	{
		float cellSize = GridManager.Instance.cellSize;
		// ghostBuilding.transform.position считается центром всего блока
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
				// Можно добавить проверку занятости ячейки, если реализовано.
			}
		}
		return true;
	}

	/// <summary>
	/// Размещает здание, списывает средства и создаёт финальную модель.
	/// </summary>
	private void PlaceBuilding()
	{
		CityStatsManager.Instance.SafeModifyStat("Budget", -currentBuildingData.cost);
		CityStatsManager.Instance.SafeModifyStat("Resources", -currentBuildingData.resourceCost);

		GameObject finalBuilding = Instantiate(currentBuildingPrefab, ghostBuilding.transform.position, ghostBuilding.transform.rotation);
		Debug.Log("Здание успешно размещено по позиции: " + ghostBuilding.transform.position);
		CancelPlacement();
	}

	/// <summary>
	/// Отмена размещения: уничтожает ghost-модель и завершает режим строительства.
	/// </summary>
	public void CancelPlacement()
	{
		isPlacing = false;
		if(ghostBuilding != null)
			Destroy(ghostBuilding);
		Debug.Log("Режим размещения завершен.");
	}

	/// <summary>
	/// Применяет ghost-эффект к объекту: устанавливает материал с заданным цветом и прозрачностью.
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
	/// Обновляет цвет ghost-модели.
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
	/// Устанавливает материал в режим прозрачности.
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
	/// Выводит popup-уведомление с сообщением на заданное время.
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

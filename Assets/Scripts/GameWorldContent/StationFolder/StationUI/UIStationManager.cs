using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIStationManager : MonoBehaviour
{
	[Header("Card")]
	[SerializeField] private GameObject stationInfoRoot;
	[SerializeField] private Image stationInfoBackground;
	[SerializeField] private TMP_Text stationInfoText;

	[Header("Follow Mouse")]
	[SerializeField] private bool followMouse = true;
	[SerializeField] private Vector2 screenOffset = new(20f, -20f);

	[Header("Background Colors")]
	[SerializeField] private Color defaultBackgroundColor = new(0f, 0f, 0f, 0.75f);
	[SerializeField] private Color cityBackgroundColor = new(0.1f, 0.2f, 0.45f, 0.85f);
	[SerializeField] private Color factoryBackgroundColor = new(0.35f, 0.2f, 0.1f, 0.85f);
	[SerializeField] private Color portBackgroundColor = new(0.05f, 0.35f, 0.4f, 0.85f);

	private Canvas parentCanvas;
	private RectTransform rootRectTransform;
	private Camera uiCamera;

	public static UIStationManager Instance { get; private set; }

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}

		Instance = this;
		CacheReferences();
		HideStationInfo();
	}

	private void Update()
	{
		if (!followMouse || stationInfoRoot == null || !stationInfoRoot.activeSelf)
			return;

		UpdateCardPosition();
	}

	private void OnDestroy()
	{
		if (Instance == this)
			Instance = null;
	}

	public void ShowStationInfo(Station station)
	{
		if (station == null)
		{
			HideStationInfo();
			return;
		}

		CacheReferences();

		if (stationInfoRoot == null || stationInfoText == null)
			return;

		stationInfoRoot.SetActive(true);
		stationInfoText.text = BuildStationDescription(station);

		if (stationInfoBackground != null)
			stationInfoBackground.color = ResolveBackgroundColor(station.Attributes);

		UpdateCardPosition();
	}

	public void HideStationInfo()
	{
		if (stationInfoRoot != null)
			stationInfoRoot.SetActive(false);
	}

	public void SetOnHoverText(string text)
	{
		CacheReferences();

		if (stationInfoRoot == null || stationInfoText == null)
			return;

		stationInfoRoot.SetActive(true);
		stationInfoText.text = text;

		if (stationInfoBackground != null)
			stationInfoBackground.color = defaultBackgroundColor;

		UpdateCardPosition();
	}

	public void OffOnHoverText()
	{
		HideStationInfo();
	}

	private void CacheReferences()
	{
		if (stationInfoRoot == null)
		{
			GameObject rootObject = GameObject.Find("StationInfoPanel");
			if (rootObject != null)
				stationInfoRoot = rootObject;
		}

		if (stationInfoText == null)
		{
			GameObject textObject = GameObject.Find("OnHoverText");
			if (textObject != null)
				stationInfoText = textObject.GetComponent<TMP_Text>();
		}

		if (stationInfoBackground == null && stationInfoRoot != null)
			stationInfoBackground = stationInfoRoot.GetComponent<Image>();

		if (stationInfoRoot != null)
		{
			rootRectTransform = stationInfoRoot.GetComponent<RectTransform>();
			parentCanvas = stationInfoRoot.GetComponentInParent<Canvas>();
			uiCamera = parentCanvas != null && parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay
				? parentCanvas.worldCamera
				: null;
		}
	}

	private void UpdateCardPosition()
	{
		if (rootRectTransform == null)
			return;

		Vector2 screenPosition = (Vector2)Input.mousePosition + screenOffset;
		screenPosition.x = Mathf.Clamp(screenPosition.x, 0f, Screen.width);
		screenPosition.y = Mathf.Clamp(screenPosition.y, 0f, Screen.height);

		if (parentCanvas == null)
		{
			rootRectTransform.position = screenPosition;
			return;
		}

		RectTransform canvasRect = parentCanvas.transform as RectTransform;
		if (canvasRect == null)
		{
			rootRectTransform.position = screenPosition;
			return;
		}

		if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPosition, uiCamera, out Vector2 localPoint))
		{
			Vector2 clampedLocalPoint = localPoint;

			// Keep the panel inside canvas bounds so UI cameras never receive invalid screen-space points.
			if (rootRectTransform.rect.width > 0f && rootRectTransform.rect.height > 0f)
			{
				float halfWidth = rootRectTransform.rect.width * 0.5f;
				float halfHeight = rootRectTransform.rect.height * 0.5f;
				Rect canvasLocalRect = canvasRect.rect;

				clampedLocalPoint.x = Mathf.Clamp(clampedLocalPoint.x, canvasLocalRect.xMin + halfWidth, canvasLocalRect.xMax - halfWidth);
				clampedLocalPoint.y = Mathf.Clamp(clampedLocalPoint.y, canvasLocalRect.yMin + halfHeight, canvasLocalRect.yMax - halfHeight);
			}

			rootRectTransform.anchoredPosition = clampedLocalPoint;
		}
	}

	private static string BuildStationDescription(Station station)
	{
		StringBuilder builder = new StringBuilder();
		builder.AppendLine(station.gameObject.name);
		builder.Append("Population: ").AppendLine(station.Population.ToString());
		builder.Append("Attributes: ");
		AppendAttributes(builder, station.Attributes);
		builder.AppendLine();
		AppendResourceSection(builder, "Produced", station.ProducedResources, station.Supply);
		AppendResourceSection(builder, "Requested", station.ConsumedResources, station.Demand);
		return builder.ToString();
	}

	private static void AppendAttributes(StringBuilder builder, IReadOnlyList<StationAttribute> attributes)
	{
		if (attributes == null || attributes.Count == 0)
		{
			builder.Append("None");
			return;
		}

		bool hasAttributes = false;

		foreach (StationAttribute attribute in attributes)
		{
			if (attribute == null)
				continue;

			if (hasAttributes)
				builder.Append(", ");

			builder.Append(NicifyName(attribute.AttributeType.ToString()));
			hasAttributes = true;
		}

		if (!hasAttributes)
			builder.Append("None");
	}

	private static void AppendResourceSection(
		StringBuilder builder,
		string sectionTitle,
		IReadOnlyList<ResourceAmount> resourceTypes,
		ResourceAmount[] values)
	{
		builder.Append(sectionTitle).Append(": ");

		if (resourceTypes == null || resourceTypes.Count == 0)
		{
			builder.AppendLine("None");
			return;
		}

		for (int i = 0; i < resourceTypes.Count; i++)
		{
			ResourceAmount resourceInfo = resourceTypes[i];
			ResourceType resourceType = resourceInfo.Type;

			if (i > 0)
				builder.Append(", ");

			int amount = values != null && (int)resourceType < values.Length
				? values[(int)resourceType].Amount
				: 0;

			builder
				.Append(NicifyName(resourceType.ToString()))
				.Append(" x")
				.Append(resourceInfo.Amount)
				.Append(" (")
				.Append(amount)
				.Append(')');
		}

		builder.AppendLine();
	}

	private Color ResolveBackgroundColor(IReadOnlyList<StationAttribute> attributes)
	{
		if (attributes != null)
		{
			foreach (StationAttribute attribute in attributes)
			{
				if (attribute == null)
					continue;

				switch (attribute.AttributeType)
				{
					case StationAttributeType.City:
					case StationAttributeType.Village:
						return cityBackgroundColor;
					case StationAttributeType.Factory:
					case StationAttributeType.FurnitureFactory:
					case StationAttributeType.FoodIndustry:
					case StationAttributeType.TextileIndustry:
					case StationAttributeType.MechanicalEngineering:
					case StationAttributeType.Shipbuilding:
					case StationAttributeType.IronOreIndustry:
					case StationAttributeType.NonFerrousMetallurgy:
					case StationAttributeType.CoalIndustry:
					case StationAttributeType.OilIndustry:
						return factoryBackgroundColor;
					case StationAttributeType.Port:
					case StationAttributeType.Seaport:
					case StationAttributeType.RiverFishing:
					case StationAttributeType.SeaFishing:
						return portBackgroundColor;
				}
			}
		}

		return defaultBackgroundColor;
	}

	private static string NicifyName(string value)
	{
		if (string.IsNullOrEmpty(value))
			return string.Empty;

		StringBuilder builder = new StringBuilder(value.Length + 8);
		builder.Append(value[0]);

		for (int i = 1; i < value.Length; i++)
		{
			char current = value[i];
			char previous = value[i - 1];

			if (char.IsUpper(current) && !char.IsUpper(previous))
				builder.Append(' ');

			builder.Append(current);
		}

		return builder.ToString();
	}
}

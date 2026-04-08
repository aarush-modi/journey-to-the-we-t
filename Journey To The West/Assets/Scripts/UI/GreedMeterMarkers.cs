using UnityEngine;
using UnityEngine.UI;

public class GreedMeterMarkers : MonoBehaviour
{
    [SerializeField] private RectTransform sliderRect;
    [SerializeField] private Color markerColor = Color.white;
    [SerializeField] private float markerWidth = 2f;

    private void Start()
    {
        CreateMarkers();
    }

    private void CreateMarkers()
    {
        if (sliderRect == null) return;

        int[] thresholds = GreedMeterLogic.GetTierThresholds();
        int max = GreedMeterLogic.GetMaxThreshold();

        foreach (int threshold in thresholds)
        {
            if (threshold >= max) continue; // skip marker at far right edge

            float normalized = (float)threshold / max;

            GameObject marker = new GameObject($"TierMarker_{threshold}", typeof(RectTransform), typeof(Image));
            marker.transform.SetParent(sliderRect, false);

            RectTransform rt = marker.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(normalized, 0f);
            rt.anchorMax = new Vector2(normalized, 1f);
            rt.sizeDelta = new Vector2(markerWidth, 0f);
            rt.anchoredPosition = Vector2.zero;

            Image img = marker.GetComponent<Image>();
            img.color = markerColor;
            img.raycastTarget = false;
        }
    }
}

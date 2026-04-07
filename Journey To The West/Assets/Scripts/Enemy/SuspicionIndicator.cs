using UnityEngine;

/// <summary>
/// Renders floating exclamation marks above a guard to visualize suspicion level.
///   Low  → !   (yellow)
///   Med  → !!  (orange)
///   High → !!! (red, pulsing)
/// Automatically copies the sorting layer from the guard's own SpriteRenderer
/// so the icons render on top of the character, not behind the tilemap.
/// </summary>
[RequireComponent(typeof(StealthDetector))]
public class SuspicionIndicator : MonoBehaviour
{
    [Header("Exclamation Sprite")]
    [SerializeField] private Sprite exclamationSprite;

    [Header("Layout")]
    [SerializeField] private float offsetY = 2f;
    [SerializeField] private float spacing = 0.5f;
    [SerializeField] private float iconScale = 4f;

    [Header("Bob Animation")]
    [SerializeField] private float bobAmplitude = 0.15f;
    [SerializeField] private float bobSpeed = 3f;

    [Header("Pulse (High only)")]
    [SerializeField] private float pulseMinScale = 0.7f;
    [SerializeField] private float pulseMaxScale = 1.0f;
    [SerializeField] private float pulseSpeed = 5f;

    private static readonly Color ColorLow = Color.yellow;
    private static readonly Color ColorMedium = new Color(1f, 0.55f, 0f);
    private static readonly Color ColorHigh = Color.red;

    private StealthDetector detector;
    private SpriteRenderer guardRenderer;
    private Transform container;
    private SpriteRenderer[] marks;
    private int visibleCount;
    private SuspicionLevel currentLevel = SuspicionLevel.None;

    private void Awake()
    {
        detector = GetComponent<StealthDetector>();
        guardRenderer = GetComponent<SpriteRenderer>();

        container = new GameObject("_SuspicionMarks").transform;
        container.SetParent(transform, false);
        container.localPosition = new Vector3(0f, offsetY, 0f);
        container.localScale = Vector3.one;

        int baseSortOrder = guardRenderer != null ? guardRenderer.sortingOrder + 10 : 100;
        string sortLayer = guardRenderer != null ? guardRenderer.sortingLayerName : "Default";

        marks = new SpriteRenderer[3];
        for (int i = 0; i < 3; i++)
        {
            GameObject go = new GameObject($"Mark{i}");
            go.transform.SetParent(container, false);
            go.transform.localScale = Vector3.one * iconScale;

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = exclamationSprite;
            sr.sortingLayerName = sortLayer;
            sr.sortingOrder = baseSortOrder + i;
            go.SetActive(false);

            marks[i] = sr;
        }
    }

    private void OnEnable()
    {
        if (detector != null)
            detector.OnSuspicionLevelChanged += HandleLevelChanged;
    }

    private void OnDisable()
    {
        if (detector != null)
            detector.OnSuspicionLevelChanged -= HandleLevelChanged;
    }

    private void HandleLevelChanged(SuspicionLevel level)
    {
        ApplyLevel(level);
    }

    private void ApplyLevel(SuspicionLevel level)
    {
        if (level == currentLevel) return;
        currentLevel = level;

        switch (level)
        {
            case SuspicionLevel.None:
                SetMarks(0, Color.white);
                break;
            case SuspicionLevel.Low:
                SetMarks(1, ColorLow);
                break;
            case SuspicionLevel.Medium:
                SetMarks(2, ColorMedium);
                break;
            case SuspicionLevel.High:
                SetMarks(3, ColorHigh);
                break;
        }
    }

    private void SetMarks(int count, Color color)
    {
        visibleCount = count;
        float totalWidth = (count - 1) * spacing;
        float startX = -totalWidth * 0.5f;

        for (int i = 0; i < marks.Length; i++)
        {
            bool active = i < count;
            marks[i].gameObject.SetActive(active);
            if (active)
            {
                marks[i].color = color;
                marks[i].transform.localPosition = new Vector3(startX + i * spacing, 0f, 0f);
                marks[i].transform.localScale = Vector3.one * iconScale;
            }
        }
    }

    private void Update()
    {
        if (detector != null)
            ApplyLevel(detector.CurrentSuspicionLevel);

        if (visibleCount == 0) return;

        float bob = Mathf.Sin(Time.time * bobSpeed * Mathf.PI) * bobAmplitude;
        container.localPosition = new Vector3(0f, offsetY + bob, 0f);

        if (currentLevel == SuspicionLevel.High)
        {
            float pulse = Mathf.Lerp(pulseMinScale, pulseMaxScale,
                (Mathf.Sin(Time.time * pulseSpeed * Mathf.PI) + 1f) * 0.5f);
            for (int i = 0; i < visibleCount; i++)
                marks[i].transform.localScale = Vector3.one * (iconScale * pulse);
        }
    }
}

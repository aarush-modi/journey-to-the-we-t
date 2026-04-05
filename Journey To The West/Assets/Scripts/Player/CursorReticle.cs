using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CursorReticle : MonoBehaviour
{
    [SerializeField] private float detectionRadius = 1.5f;
    [SerializeField] private SpriteRenderer circleRenderer;
    [SerializeField] private Color noTargetColor = new Color(1f, 1f, 1f, 0.25f);
    [SerializeField] private Color hasTargetColor = new Color(1f, 0f, 0f, 0.4f);

    [Header("Range Constraint")]
    [SerializeField] private Color lineColor = new Color(1f, 1f, 1f, 0.5f);
    [SerializeField] private float lineWidth = 0.08f;
    [SerializeField] private float lineStartPadding = 0.5f;
    [SerializeField] private float lineEndPadding = 0.3f;
    [SerializeField] private string lineSortingLayer = "Decor";
    [SerializeField] private int lineSortingOrder = 100;

    private Transform constraintOrigin;
    private float maxRange;
    private LineRenderer lineLeft;
    private LineRenderer lineRight;
    private LineRenderer lineCenter;

    private List<IDamageable> currentTargets = new List<IDamageable>();
    private Camera mainCamera;

    public bool HasTarget => currentTargets.Count > 0;

    private void Awake()
    {
        mainCamera = Camera.main;
        if (circleRenderer != null)
        {
            float diameter = detectionRadius * 2f;
            circleRenderer.transform.localScale = new Vector3(diameter, diameter, 1f);
        }

        lineLeft = CreateLine("IndicatorLineLeft");
        lineRight = CreateLine("IndicatorLineRight");
        lineCenter = CreateLine("IndicatorLineCenter");
    }

    private void Update()
    {
        if (PauseController.IsGamePaused)
        {
            if (!Cursor.visible) Cursor.visible = true;
            return;
        }
        else if (Cursor.visible)
        {
            Cursor.visible = false;
        }

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null) return;
        }

        // Track mouse position in world space
        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        float camDist = Mathf.Abs(mainCamera.transform.position.z);
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, camDist));
        worldPos.z = 0f;

        if (constraintOrigin != null && maxRange > 0f)
        {
            Vector2 origin = constraintOrigin.position;
            Vector2 offset = (Vector2)worldPos - origin;
            if (offset.magnitude > maxRange)
            {
                worldPos = (Vector3)(origin + offset.normalized * maxRange);
                worldPos.z = 0f;
            }
        }

        transform.position = worldPos;

        // Detect enemies in radius
        currentTargets.Clear();
        Collider2D[] hits = Physics2D.OverlapCircleAll(worldPos, detectionRadius);
        foreach (Collider2D hit in hits)
        {
            // Skip the player
            if (hit.CompareTag("Player")) continue;

            IDamageable damageable = hit.GetComponent<IDamageable>();
            if (damageable != null)
            {
                currentTargets.Add(damageable);
            }
        }

        // Update circle color
        if (circleRenderer != null)
        {
            circleRenderer.color = HasTarget ? hasTargetColor : noTargetColor;
        }

        UpdateIndicatorLines();
    }

    public List<IDamageable> GetTargets()
    {
        return new List<IDamageable>(currentTargets);
    }

    public Vector2 GetWorldPosition()
    {
        return transform.position;
    }

    public void SetDetectionRadius(float radius)
    {
        detectionRadius = radius;
        if (circleRenderer != null)
        {
            float diameter = detectionRadius * 2f;
            circleRenderer.transform.localScale = new Vector3(diameter, diameter, 1f);
        }
    }

    public void SetConstraint(float range, Transform origin)
    {
        maxRange = range;
        constraintOrigin = origin;
    }

    private LineRenderer CreateLine(string name)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(transform);
        LineRenderer lr = obj.AddComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.positionCount = 2;
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;

        // Use a reliable unlit material for the lines
        Shader shader = Shader.Find("Sprites/Default");
        if (shader == null) shader = Shader.Find("UI/Default");
        if (shader == null) shader = Shader.Find("Unlit/Color");
        lr.material = new Material(shader);

        lr.startColor = lineColor;
        lr.endColor = lineColor;
        lr.sortingLayerName = lineSortingLayer;
        lr.sortingOrder = lineSortingOrder;
        return lr;
    }

    private void UpdateIndicatorLines()
    {
        if (lineLeft == null || lineRight == null || lineCenter == null || constraintOrigin == null)
            return;

        Vector2 playerPos = constraintOrigin.position;
        Vector2 reticlePos = transform.position;
        float dist = Vector2.Distance(playerPos, reticlePos);

        if (dist < 0.1f)
        {
            lineLeft.enabled = false;
            lineRight.enabled = false;
            lineCenter.enabled = false;
            return;
        }

        lineLeft.enabled = true;
        lineRight.enabled = true;

        Vector2 dir = (reticlePos - playerPos).normalized;
        Vector2 perp = new Vector2(-dir.y, dir.x);

        Vector2 leftEdge = reticlePos + perp * detectionRadius;
        Vector2 rightEdge = reticlePos - perp * detectionRadius;

        lineLeft.SetPosition(0, (Vector3)playerPos);
        lineLeft.SetPosition(1, (Vector3)leftEdge);
        lineRight.SetPosition(0, (Vector3)playerPos);
        lineRight.SetPosition(1, (Vector3)rightEdge);

        lineCenter.enabled = true;
        Vector2 lineStart = playerPos + dir * lineStartPadding;
        Vector2 lineEnd = reticlePos - dir * lineEndPadding;
        lineCenter.SetPosition(0, (Vector3)lineStart);
        lineCenter.SetPosition(1, (Vector3)lineEnd);
    }

    private void OnDisable()
    {
        Cursor.visible = true;
        if (lineLeft != null) lineLeft.enabled = false;
        if (lineRight != null) lineRight.enabled = false;
        if (lineCenter != null) lineCenter.enabled = false;
    }

    private void OnEnable()
    {
        Cursor.visible = false;
        // Lines will be re-enabled in next Update via UpdateIndicatorLines
    }
}

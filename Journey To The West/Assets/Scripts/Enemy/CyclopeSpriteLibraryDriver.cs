using UnityEngine;
using UnityEngine.U2D.Animation;

[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteResolver))]
public class CyclopeSpriteLibraryDriver : MonoBehaviour
{
    [Header("Refs (optional)")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private SpriteResolver resolver;

    [Header("Sprite Library Keys")]
    [SerializeField] private string idleCategory = "Idle";
    [SerializeField] private string walkCategory = "Walk";

    [Header("Labels (must exactly match Cyclope.spriteLib)")]
    [SerializeField] private string[] idleLabels = { "New Label", "New Label_0", "New Label_1", "New Label_2" };
    [SerializeField] private string[] walkLabels =
    {
        "New Label", "New Label_0", "New Label_1", "New Label_2",
        "New Label_3", "New Label_4", "New Label_5", "New Label_6",
        "New Label_7", "New Label_8", "New Label_9", "New Label_10",
    };

    [Header("Timing")]
    [SerializeField] private float framesPerSecond = 8f;
    [SerializeField] private float movementThreshold = 0.05f;

    private string lastCategory;
    private int frameIndex;
    private float frameTimer;

    private void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (resolver == null) resolver = GetComponent<SpriteResolver>();
    }

    private void OnEnable()
    {
        lastCategory = null;
        frameIndex = 0;
        frameTimer = 0f;
    }

    private void LateUpdate()
    {
        // Sprite library frames can look "rotated"; keep the actor upright in world space.
        transform.rotation = Quaternion.identity;
        if (rb != null) rb.angularVelocity = 0f;
    }

    private void Update()
    {
        if (resolver == null) return;

        bool isMoving = rb != null && rb.linearVelocity.sqrMagnitude > movementThreshold * movementThreshold;
        string category = isMoving ? walkCategory : idleCategory;
        string[] labels = isMoving ? walkLabels : idleLabels;
        if (labels == null || labels.Length == 0) return;

        if (category != lastCategory)
        {
            lastCategory = category;
            frameIndex = 0;
            frameTimer = 0f;
            Set(category, labels[frameIndex]);
            return;
        }

        float fps = Mathf.Max(1f, framesPerSecond);
        frameTimer += Time.deltaTime;
        float frameDuration = 1f / fps;
        if (frameTimer < frameDuration) return;

        frameTimer -= frameDuration;
        frameIndex = (frameIndex + 1) % labels.Length;
        Set(category, labels[frameIndex]);
    }

    private void Set(string category, string label)
    {
        if (string.IsNullOrWhiteSpace(category) || string.IsNullOrWhiteSpace(label)) return;
        resolver.SetCategoryAndLabel(category, label);
    }
}


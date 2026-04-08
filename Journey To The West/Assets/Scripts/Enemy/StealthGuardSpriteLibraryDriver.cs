using UnityEngine;
using UnityEngine.U2D.Animation;

[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteResolver))]
public class StealthGuardSpriteLibraryDriver : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator animatorToDisable;
    [SerializeField] private SpriteResolver resolver;

    [Header("Sprite Library Keys")]
    [SerializeField] private string idleCategory = "Idle";
    [SerializeField] private string walkCategory = "Walk";

    [Tooltip("Cyclope idle labels (exact match).")]
    [SerializeField] private string[] idleLabels = { "New Label", "New Label_0", "New Label_1", "New Label_2" };

    [Tooltip("Cyclope walk labels (exact match).")]
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
        if (animatorToDisable == null) animatorToDisable = GetComponent<Animator>();
        if (resolver == null) resolver = GetComponent<SpriteResolver>();
    }

    private void OnEnable()
    {
        if (animatorToDisable != null)
            animatorToDisable.enabled = false;
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


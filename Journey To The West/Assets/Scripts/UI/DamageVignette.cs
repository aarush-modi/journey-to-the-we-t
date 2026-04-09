using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DamageVignette : MonoBehaviour
{
    public static DamageVignette Instance;

    [Header("Flash Settings")]
    [SerializeField] private Image vignetteImage;
    [SerializeField] private float flashPeakAlpha = 0.5f;
    [SerializeField] private float lowHPFlashPeakAlpha = 0.8f;
    [SerializeField] private float flashFadeDuration = 0.3f;

    [Header("Low HP Pulse")]
    [SerializeField] private float hpThreshold = 20f;
    [SerializeField] private float pulseSpeed = 3f;
    [SerializeField] private float pulseAmplitude = 0.08f;
    [SerializeField] private float pulseBaseline = 0.2f;

    private bool isLowHP;
    private Coroutine flashRoutine;
    private PlayerCombat playerCombat;
    private Color baseColor;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        baseColor = vignetteImage.color;
        vignetteImage.raycastTarget = false;
        SetAlpha(0f);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void OnEnable()
    {
        BindPlayer();
        if (playerCombat != null)
            playerCombat.OnHPChanged.AddListener(OnHPChanged);
    }

    private void OnDisable()
    {
        if (playerCombat != null)
            playerCombat.OnHPChanged.RemoveListener(OnHPChanged);
    }

    private void BindPlayer()
    {
        if (playerCombat != null) return;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerCombat = player.GetComponent<PlayerCombat>();
    }

    private void OnHPChanged(float current, float max)
    {
        bool wasLowHP = isLowHP;
        isLowHP = current > 0 && current < hpThreshold;

        if (wasLowHP && !isLowHP)
            Clear();

        if (current >= max)
            Clear();
    }

    private void Clear()
    {
        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
            flashRoutine = null;
        }
        SetAlpha(0f);
    }

    public void Flash()
    {
        if (flashRoutine != null)
            StopCoroutine(flashRoutine);

        flashRoutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        float peakAlpha = isLowHP ? lowHPFlashPeakAlpha : flashPeakAlpha;
        float targetAlpha = isLowHP ? pulseBaseline : 0f;

        SetAlpha(peakAlpha);

        float elapsed = 0f;
        while (elapsed < flashFadeDuration)
        {
            elapsed += Time.deltaTime;
            SetAlpha(Mathf.Lerp(peakAlpha, targetAlpha, elapsed / flashFadeDuration));
            yield return null;
        }

        SetAlpha(targetAlpha);
        flashRoutine = null;
    }

    private void Update()
    {
        if (isLowHP && flashRoutine == null)
        {
            SetAlpha(pulseBaseline + Mathf.Sin(Time.time * pulseSpeed) * pulseAmplitude);
        }
    }

    private void SetAlpha(float alpha)
    {
        Color c = baseColor;
        c.a = alpha;
        vignetteImage.color = c;
    }
}

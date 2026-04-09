using UnityEngine;
using UnityEngine.Events;

public class PlayerShield : MonoBehaviour
{
    public UnityEvent<int, int> OnShieldChanged; // current, max

    [SerializeField] private GameObject shieldIconContainer;

    [Header("Layout")]
    [SerializeField] private float iconSpacing = 0.3f;

    private GreedMeter greedMeter;
    private int currentShields;
    private GameObject[] shieldIcons;
    private float shieldCooldownUntil;

    private void Awake()
    {
        greedMeter = GetComponent<GreedMeter>();
        // Hide shield icons until a greed tier grants them
        if (shieldIconContainer != null)
            shieldIconContainer.SetActive(false);
    }

    private void OnEnable()
    {
        if (greedMeter != null)
            greedMeter.OnTierChanged.AddListener(OnTierChanged);
    }

    private void OnDisable()
    {
        if (greedMeter != null)
            greedMeter.OnTierChanged.RemoveListener(OnTierChanged);
    }

    private void Start()
    {
        // Sync to whatever the current tier already is (e.g. after scene load
        // with serialized gold or DontDestroyOnLoad carry-over).
        int max = GetMaxShields();
        if (max > 0)
        {
            currentShields = max;
            SpawnIcons(max);
        }
    }

    public bool HasShields() => currentShields > 0;

    /// <summary>
    /// Tries to absorb a hit. Returns true if a shield absorbed it.
    /// </summary>
    public bool TryAbsorbHit()
    {
        if (currentShields <= 0) return false;
        if (Time.time < shieldCooldownUntil) return true;

        currentShields--;
        shieldCooldownUntil = Time.time + 0.3f;
        UpdateShieldDisplay();
        OnShieldChanged?.Invoke(currentShields, GetMaxShields());
        return true;
    }

    public void ReplenishShields()
    {
        int max = GetMaxShields();
        currentShields = max;
        SpawnIcons(max);
        OnShieldChanged?.Invoke(currentShields, max);
    }

    public int GetCurrentShields() => currentShields;

    public int GetMaxShields() => greedMeter != null ? greedMeter.GetShieldCount() : 0;

    private void OnTierChanged(GreedTier tier)
    {
        int max = GetMaxShields();
        currentShields = max;
        SpawnIcons(max);
        OnShieldChanged?.Invoke(currentShields, max);
    }

    /// <summary>
    /// Uses first child of container as template, duplicates as needed.
    /// </summary>
    private void SpawnIcons(int count)
    {
        if (shieldIconContainer == null) return;

        // Destroy all children except the first (template)
        for (int i = shieldIconContainer.transform.childCount - 1; i > 0; i--)
            Destroy(shieldIconContainer.transform.GetChild(i).gameObject);

        if (shieldIconContainer.transform.childCount == 0 || count <= 0)
        {
            shieldIconContainer.SetActive(false);
            shieldIcons = null;
            return;
        }

        shieldIconContainer.SetActive(true);

        GameObject template = shieldIconContainer.transform.GetChild(0).gameObject;
        shieldIcons = new GameObject[count];
        shieldIcons[0] = template;

        for (int i = 1; i < count; i++)
        {
            GameObject icon = Instantiate(template, shieldIconContainer.transform);
            icon.name = "ShieldIcon" + (i + 1);
            shieldIcons[i] = icon;
        }

        UpdateShieldDisplay();
    }

    private void UpdateShieldDisplay()
    {
        if (shieldIcons == null) return;

        for (int i = 0; i < shieldIcons.Length; i++)
        {
            if (shieldIcons[i] != null)
                shieldIcons[i].SetActive(i < currentShields);
        }

        // Center visible icons horizontally
        float totalWidth = (currentShields - 1) * iconSpacing;
        float startX = -totalWidth / 2f;
        for (int i = 0; i < currentShields; i++)
        {
            if (shieldIcons[i] != null)
            {
                Vector3 pos = shieldIcons[i].transform.localPosition;
                pos.x = startX + i * iconSpacing;
                shieldIcons[i].transform.localPosition = pos;
            }
        }

        if (shieldIconContainer != null)
            shieldIconContainer.SetActive(currentShields > 0);
    }
}

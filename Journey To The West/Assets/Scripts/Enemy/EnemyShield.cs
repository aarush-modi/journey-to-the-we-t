using UnityEngine;

public class EnemyShield : MonoBehaviour
{
    [SerializeField] private int shieldCount = 1;
    [SerializeField] private GameObject shieldIconContainer;

    [Header("Layout")]
    [SerializeField] private float iconSpacing = 0.5f;

    private GameObject[] shieldIcons;
    private int currentShields;

    private void Awake()
    {
        currentShields = shieldCount;
        SpawnIcons();
    }

    private void SpawnIcons()
    {
        if (shieldIconContainer == null || shieldIconContainer.transform.childCount == 0) return;

        // Use the first child as a template, then duplicate it
        GameObject template = shieldIconContainer.transform.GetChild(0).gameObject;
        shieldIcons = new GameObject[shieldCount];
        shieldIcons[0] = template;

        for (int i = 1; i < shieldCount; i++)
        {
            GameObject icon = Instantiate(template, shieldIconContainer.transform);
            icon.name = "ShieldIcon" + (i + 1);
            shieldIcons[i] = icon;
        }

        UpdateShieldDisplay();
    }

    public bool HasShields() => currentShields > 0;

    /// <summary>
    /// Tries to absorb a hit. Returns true if the shield absorbed it (no HP damage should be applied).
    /// </summary>
    public bool TryAbsorbHit()
    {
        if (currentShields <= 0) return false;

        currentShields--;
        UpdateShieldDisplay();
        return true;
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

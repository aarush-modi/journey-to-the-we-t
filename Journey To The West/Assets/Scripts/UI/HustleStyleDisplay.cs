using TMPro;
using UnityEngine;

public class HustleStyleDisplay : MonoBehaviour
{
    private TMP_Text text;

    private void Awake()
    {
        text = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        UpdateDisplay();

        if (HustleStyleManager.Instance != null)
        {
            HustleStyleManager.Instance.OnStyleSelected.AddListener(OnStyleChanged);
        }
    }

    private void OnDisable()
    {
        if (HustleStyleManager.Instance != null)
        {
            HustleStyleManager.Instance.OnStyleSelected.RemoveListener(OnStyleChanged);
        }
    }

    private void OnStyleChanged(HustleStyleData style)
    {
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (text == null) return;

        HustleStyleManager manager = HustleStyleManager.Instance;
        if (manager != null && manager.HasChosenStyle())
        {
            HustleStyleData style = manager.GetCurrentStyle();
            text.text = style.styleName.ToUpper() + " " + FormatStats(style);
        }
        else
        {
            text.text = "NONE";
        }
    }

    private static string FormatStats(HustleStyleData style)
    {
        var parts = new System.Collections.Generic.List<string>();

        if (style.combatGoldModifier != 1f)
            parts.Add($"COMBAT x{style.combatGoldModifier:0.#}");
        if (style.npcGoldModifier != 1f)
            parts.Add($"NPC x{style.npcGoldModifier:0.#}");
        if (style.shopPriceModifier != 1f)
            parts.Add($"SHOP x{style.shopPriceModifier:0.#}");
        if (style.bonusGold != 0)
            parts.Add($"+{style.bonusGold}G");
        if (style.maxHPModifier != 1f)
            parts.Add($"HP x{style.maxHPModifier:0.#}");

        return parts.Count > 0 ? "(" + string.Join(", ", parts) + ")" : "";
    }
}

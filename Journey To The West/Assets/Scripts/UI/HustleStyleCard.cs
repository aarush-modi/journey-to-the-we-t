using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HustleStyleCard : MonoBehaviour
{
    [SerializeField] private Image styleImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text statsText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private GameObject selectedIndicator;

    public HustleStyleData Style { get; private set; }

    private Action<HustleStyleData> onClick;

    public void Setup(HustleStyleData style, Action<HustleStyleData> clickCallback)
    {
        Style = style;
        onClick = clickCallback;

        if (styleImage != null) styleImage.sprite = style.sprite;
        if (nameText != null) nameText.text = style.styleName;
        if (descriptionText != null) descriptionText.text = style.description;
        if (statsText != null) statsText.text = FormatStats(style);
    }

    public void SetSelected(bool selected)
    {
        if (selectedIndicator != null)
            selectedIndicator.SetActive(selected);
    }

    public void OnClick()
    {
        onClick?.Invoke(Style);
    }

    private static string FormatStats(HustleStyleData style)
    {
        var sb = new System.Text.StringBuilder();

        if (style.combatGoldModifier != 1f)
            sb.AppendLine($"Combat Gold: x{style.combatGoldModifier:0.#}");
        if (style.npcGoldModifier != 1f)
            sb.AppendLine($"NPC Gold: x{style.npcGoldModifier:0.#}");
        if (style.shopPriceModifier != 1f)
            sb.AppendLine($"Shop Prices: x{style.shopPriceModifier:0.#}");
        if (style.bonusGold != 0)
            sb.AppendLine($"Bonus Gold: +{style.bonusGold}");
        if (style.maxHPModifier != 1f)
            sb.AppendLine($"Max HP: x{style.maxHPModifier:0.#}");

        return sb.ToString().TrimEnd();
    }
}

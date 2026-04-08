using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    public GameObject currentItem; //the item that is currently in the slot

    private Image cooldownOverlay;
    private Image activeHighlight;
    private Coroutine cooldownCoroutine;

    private void Awake()
    {
        // --- Cooldown overlay (filled radial, on top of skill icon) ---
        GameObject overlayGO = new GameObject("CooldownOverlay");
        overlayGO.transform.SetParent(transform, false);

        cooldownOverlay = overlayGO.AddComponent<Image>();
        cooldownOverlay.color = new Color(0f, 0f, 0f, 0.5f);
        cooldownOverlay.type = Image.Type.Filled;
        cooldownOverlay.fillMethod = Image.FillMethod.Radial360;
        cooldownOverlay.fillAmount = 0f;
        cooldownOverlay.raycastTarget = false;

        RectTransform overlayRT = overlayGO.GetComponent<RectTransform>();
        overlayRT.anchorMin = Vector2.zero;
        overlayRT.anchorMax = Vector2.one;
        overlayRT.sizeDelta = Vector2.zero;
        overlayRT.anchoredPosition = Vector2.zero;

        // Ensure the overlay renders on top of any skill item added later
        overlayGO.transform.SetAsLastSibling();
        overlayGO.SetActive(false); // Hidden until cooldown starts

        // --- Active highlight (yellow tint, hidden by default) ---
        GameObject highlightGO = new GameObject("ActiveHighlight");
        highlightGO.transform.SetParent(transform, false);

        activeHighlight = highlightGO.AddComponent<Image>();
        activeHighlight.color = new Color(1f, 1f, 0f, 0.3f);
        activeHighlight.raycastTarget = false;

        RectTransform highlightRT = highlightGO.GetComponent<RectTransform>();
        highlightRT.anchorMin = Vector2.zero;
        highlightRT.anchorMax = Vector2.one;
        highlightRT.sizeDelta = Vector2.zero;
        highlightRT.anchoredPosition = Vector2.zero;

        highlightGO.SetActive(false);
    }

    /// <summary>
    /// Returns the SkillData on the current item, or null if empty.
    /// </summary>
    public SkillData GetSkillData()
    {
        return currentItem?.GetComponent<Skill>()?.data;
    }

    /// <summary>
    /// Toggles the active highlight on this slot.
    /// </summary>
    public void SetActive(bool active)
    {
        activeHighlight.gameObject.SetActive(active);
    }

    /// <summary>
    /// Starts the cooldown overlay animation and swaps the icon to disabledIcon.
    /// </summary>
    public void StartCooldown(float duration)
    {
        if (cooldownCoroutine != null)
        {
            StopCoroutine(cooldownCoroutine);
        }

        // Swap to disabled icon
        SkillData data = GetSkillData();
        if (data != null && data.disabledIcon != null && currentItem != null)
        {
            Image itemImage = currentItem.GetComponent<Image>();
            if (itemImage != null)
            {
                itemImage.sprite = data.disabledIcon;
            }
        }

        // Show overlay and ensure it renders on top
        cooldownOverlay.gameObject.SetActive(true);
        cooldownOverlay.transform.SetAsLastSibling();

        cooldownCoroutine = StartCoroutine(AnimateCooldown(duration));
    }

    /// <summary>
    /// Stops any running cooldown, resets the overlay, and restores the normal icon.
    /// </summary>
    public void ResetCooldown()
    {
        if (cooldownCoroutine != null)
        {
            StopCoroutine(cooldownCoroutine);
            cooldownCoroutine = null;
        }

        cooldownOverlay.fillAmount = 0f;
        cooldownOverlay.gameObject.SetActive(false);

        // Restore normal icon
        SkillData data = GetSkillData();
        if (data != null && currentItem != null)
        {
            Image itemImage = currentItem.GetComponent<Image>();
            if (itemImage != null)
            {
                itemImage.sprite = data.icon;
            }
        }
    }

    private IEnumerator AnimateCooldown(float duration)
    {
        float elapsed = 0f;
        cooldownOverlay.fillAmount = 1f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            cooldownOverlay.fillAmount = 1f - (elapsed / duration);
            yield return null;
        }

        cooldownOverlay.fillAmount = 0f;
        cooldownOverlay.gameObject.SetActive(false);

        // Restore normal icon when cooldown finishes naturally
        SkillData data = GetSkillData();
        if (data != null && currentItem != null)
        {
            Image itemImage = currentItem.GetComponent<Image>();
            if (itemImage != null)
            {
                itemImage.sprite = data.icon;
            }
        }

        cooldownCoroutine = null;
    }
}

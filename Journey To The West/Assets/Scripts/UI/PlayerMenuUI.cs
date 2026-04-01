using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerMenuUI : MonoBehaviour
{
    [Header("Hustle Style Display")]
    [SerializeField] private TMP_Text hustleStyleText;
    [SerializeField] private Image hustleStyleImage;

    [Header("Defaults")]
    [SerializeField] private string defaultStyleLabel = "NONE";
    [SerializeField] private Sprite defaultPortrait;

    private void OnEnable()
    {
        RefreshDisplay();

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
        ApplyStyle(style);
    }

    private void RefreshDisplay()
    {
        if (HustleStyleManager.Instance != null && HustleStyleManager.Instance.HasChosenStyle())
        {
            ApplyStyle(HustleStyleManager.Instance.GetCurrentStyle());
        }
        else
        {
            if (hustleStyleText != null)
                hustleStyleText.text = defaultStyleLabel;

            if (hustleStyleImage != null)
            {
                hustleStyleImage.sprite = defaultPortrait;
                hustleStyleImage.gameObject.SetActive(defaultPortrait != null);
            }
        }
    }

    private void ApplyStyle(HustleStyleData style)
    {
        if (style == null) return;

        if (hustleStyleText != null)
            hustleStyleText.text = style.styleName.ToUpper();

        if (hustleStyleImage != null)
        {
            hustleStyleImage.sprite = style.sprite;
            hustleStyleImage.gameObject.SetActive(style.sprite != null);
        }
    }
}

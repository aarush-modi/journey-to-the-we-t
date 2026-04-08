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

    [Header("Red Packets")]
    [SerializeField] private TMP_Text redPacketLabelText;
    [SerializeField] private TMP_Text redPacketCountText;

    private void OnEnable()
    {
        RefreshDisplay();

        if (HustleStyleManager.Instance != null)
        {
            HustleStyleManager.Instance.OnStyleSelected.AddListener(OnStyleChanged);
        }

        if (RedPacketTracker.Instance != null)
        {
            RedPacketTracker.Instance.onRedPacketCollected.AddListener(OnRedPacketCollected);
        }
    }

    private void OnDisable()
    {
        if (HustleStyleManager.Instance != null)
        {
            HustleStyleManager.Instance.OnStyleSelected.RemoveListener(OnStyleChanged);
        }

        if (RedPacketTracker.Instance != null)
        {
            RedPacketTracker.Instance.onRedPacketCollected.RemoveListener(OnRedPacketCollected);
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

        UpdateRedPacketCount();
    }

    private void OnRedPacketCollected()
    {
        UpdateRedPacketCount();
    }

    private void UpdateRedPacketCount()
    {
        if (redPacketCountText != null && RedPacketTracker.Instance != null)
            redPacketCountText.text = $"{RedPacketTracker.Instance.GetCount()}/6";
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

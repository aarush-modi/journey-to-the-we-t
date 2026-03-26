using System;
using UnityEngine;

public class HustleStyleSelectionUI : MonoBehaviour
{
    [SerializeField] private HustleStyleData[] styles;
    [SerializeField] private HustleStyleCard[] cards;

    private HustleStyleData selectedStyle;
    private Action<HustleStyleData> onConfirmed;

    public void Open(Action<HustleStyleData> callback)
    {
        onConfirmed = callback;
        selectedStyle = null;

        for (int i = 0; i < cards.Length; i++)
        {
            if (i < styles.Length)
            {
                cards[i].Setup(styles[i], OnCardClicked);
                cards[i].gameObject.SetActive(true);
                cards[i].SetSelected(false);
            }
            else
            {
                cards[i].gameObject.SetActive(false);
            }
        }

        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
        PauseController.SetPause(false);
    }

    private void OnCardClicked(HustleStyleData style)
    {
        selectedStyle = style;

        for (int i = 0; i < cards.Length; i++)
        {
            if (cards[i].gameObject.activeSelf)
            {
                cards[i].SetSelected(cards[i].Style == style);
            }
        }
    }

    public void OnConfirmClicked()
    {
        if (selectedStyle == null) return;

        onConfirmed?.Invoke(selectedStyle);
        Close();
    }
}

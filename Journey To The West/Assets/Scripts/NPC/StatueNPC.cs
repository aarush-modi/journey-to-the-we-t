using UnityEngine;

public class StatueNPC : NPCBase
{
    [Header("Dialogue")]
    [SerializeField] private NPCDialogue blessingDialogue;
    [SerializeField] private NPCDialogue reminderDialogue;

    [Header("Card Selection")]
    [SerializeField] private HustleStyleSelectionUI selectionUI;

    private bool hasBlessed;
    private bool awaitingCardUI;

    protected override bool ShouldUnpauseOnDialogueEnd()
    {
        return !awaitingCardUI;
    }

    public override void Interact(GameObject player)
    {
        if (!hasBlessed)
        {
            awaitingCardUI = true;
            OnDialogueComplete.AddListener(OnBlessingDialogueComplete);
            PlayDialogue(blessingDialogue);
        }
        else
        {
            PlayDialogue(reminderDialogue);
        }
    }

    private void OnBlessingDialogueComplete()
    {
        OnDialogueComplete.RemoveListener(OnBlessingDialogueComplete);
        awaitingCardUI = false;

        if (selectionUI != null)
        {
            selectionUI.Open(OnStyleSelected);
        }
        else
        {
            PauseController.SetPause(false);
        }
    }

    private void OnStyleSelected(HustleStyleData style)
    {
        hasBlessed = true;

        if (HustleStyleManager.Instance != null)
        {
            HustleStyleManager.Instance.ApplyStyle(style);
        }
    }
}

using UnityEngine;

public class NickelNoumanNPC : NPCBase
{
    private const string TeleporterUnlockKey = "MerchantTown.Teleporter1PlusUnlocked";
    private const string CorrectAnswerOutcome = "answer_moneygrubber";

    [Header("Dialogue")]
    [SerializeField] private NPCDialogue dialogue;

    public bool IsTeleporterUnlocked => PlayerPrefs.GetInt(TeleporterUnlockKey, 0) == 1;

    public override void Interact(GameObject player)
    {
        OnDialogueComplete.RemoveListener(HandleDialogueComplete);
        OnDialogueComplete.AddListener(HandleDialogueComplete);
        PlayDialogue(dialogue);
    }

    private void HandleDialogueComplete()
    {
        OnDialogueComplete.RemoveListener(HandleDialogueComplete);

        if (lastDialogueOutcome != CorrectAnswerOutcome)
        {
            return;
        }

        PlayerPrefs.SetInt(TeleporterUnlockKey, 1);
        PlayerPrefs.Save();
    }
}

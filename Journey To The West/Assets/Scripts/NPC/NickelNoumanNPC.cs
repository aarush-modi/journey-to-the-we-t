using UnityEngine;
using UnityEngine.InputSystem;

public class NickelNoumanNPC : NPCBase
{
    private const string CorrectAnswerOutcome = "answer_moneygrubber";
    private static bool isTeleporterUnlockedThisSession;

    [Header("Dialogue")]
    [SerializeField] private NPCDialogue dialogue;

    public bool IsTeleporterUnlocked => isTeleporterUnlockedThisSession;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetTeleporterUnlockState()
    {
        isTeleporterUnlockedThisSession = false;
    }

    private void Update()
    {
        if (Keyboard.current == null
            || (!Keyboard.current.digit9Key.wasPressedThisFrame && !Keyboard.current.numpad9Key.wasPressedThisFrame))
        {
            return;
        }

        SetTeleporterUnlocked(!IsTeleporterUnlocked);
        Debug.Log($"[NickelNouman] Debug toggle with 9 pressed. Teleporter 1+ unlocked = {IsTeleporterUnlocked}", this);
    }

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
            Debug.Log($"[NickelNouman] Dialogue ended with '{lastDialogueOutcome}'. Teleporter remains locked = {!IsTeleporterUnlocked}", this);
            return;
        }

        SetTeleporterUnlocked(true);
        Debug.Log("[NickelNouman] Correct answer received. Teleporter 1+ unlocked permanently.", this);
    }

    private void SetTeleporterUnlocked(bool unlocked)
    {
        isTeleporterUnlockedThisSession = unlocked;
    }
}

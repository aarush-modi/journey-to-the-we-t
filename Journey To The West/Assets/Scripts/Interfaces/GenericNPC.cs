using UnityEngine;

public class GenericNPC : NPCBase
{
    [Header("Dialogue")]
    [SerializeField] private NPCDialogue introDialogue;
    [SerializeField] private NPCDialogue reminderDialogue;

    private bool hasSpokenBefore;

    public override void Interact(GameObject player)
    {
        if (!hasSpokenBefore)
        {
            OnDialogueComplete.AddListener(OnIntroComplete);
            PlayDialogue(introDialogue);
        }
        else
        {
            PlayDialogue(reminderDialogue);
        }
    }

    private void OnIntroComplete()
    {
        OnDialogueComplete.RemoveListener(OnIntroComplete);
        hasSpokenBefore = true;
    }
}
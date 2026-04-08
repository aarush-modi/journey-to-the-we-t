using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CastleWarnerNPC : NPCBase
{
    [Header("Dialogue")]
    [SerializeField] private NPCDialogue introDialogue;
    [SerializeField] private NPCDialogue reminderDialogue;

    private bool hasSpoken;

    protected override void Start()
    {
        base.Start();
    }

    public override void Interact(GameObject player)
    {
        if (isDialogueActive)
        {
            PlayDialogue(hasSpoken ? reminderDialogue : introDialogue);
            return;
        }

        if (!hasSpoken)
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
        hasSpoken = true;
    }
}

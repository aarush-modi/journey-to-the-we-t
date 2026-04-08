using UnityEngine;

public class GhostGirlNPC : NPCBase
{
    [Header("Dialogue")]
    [SerializeField] private NPCDialogue introDialogue;
    [SerializeField] private NPCDialogue reminderDialogue;

    [Header("Quest")]
    [SerializeField] private PackageData packageToGive;
    [SerializeField] private QuestData questToComplete;
    [SerializeField] private QuestData questToStart;

    private PlayerInventory playerInventory;
    private bool hasInteracted;

    public override void Interact(GameObject player)
    {
        if (playerInventory == null)
            playerInventory = player.GetComponent<PlayerInventory>();

        if (!hasInteracted)
        {
            OnDialogueComplete.AddListener(OnIntroComplete);
            PlayDialogue(introDialogue);
        }
        else
        {
            PlayDialogue(reminderDialogue);
        }
    }

    private async void OnIntroComplete()
    {
        OnDialogueComplete.RemoveListener(OnIntroComplete);

        if (hasInteracted) return;
        hasInteracted = true;

        if (playerInventory != null && packageToGive != null)
            playerInventory.AddPackage(packageToGive);

        if (questToComplete != null)
            QuestManager.Instance.CompleteQuest(questToComplete);

        if (questToStart != null)
            QuestManager.Instance.StartQuest(questToStart);

        if (ScreenFader.Instance != null)
        {
            await ScreenFader.Instance.FadeOut();
            gameObject.SetActive(false);
            await ScreenFader.Instance.FadeIn();
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
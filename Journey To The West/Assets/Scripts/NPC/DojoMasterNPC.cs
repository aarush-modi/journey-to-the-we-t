using UnityEngine;

public class DojoMasterNPC : NPCBase
{
    [Header("Dialogue")]
    [SerializeField] private NPCDialogue introDialogue;
    [SerializeField] private NPCDialogue reminderDialogue;
    [SerializeField] private NPCDialogue completeDialogue;

    [Header("Quest")]
    [SerializeField] private QuestData questToStart;
    [SerializeField] private PackageData packageToGive;
    [SerializeField] private ObjectiveTracker objectiveTracker;

    private PlayerInventory playerInventory;
    private bool questStarted;
    private bool hasGivenPackage;

    public override void Interact(GameObject player)
    {
        if (playerInventory == null)
            playerInventory = player.GetComponent<PlayerInventory>();

        if (!questStarted)
        {
            OnDialogueComplete.AddListener(OnIntroComplete);
            PlayDialogue(introDialogue);
        }
        else if (!hasGivenPackage && objectiveTracker != null && objectiveTracker.IsComplete)
        {
            OnDialogueComplete.AddListener(OnQuestComplete);
            PlayDialogue(completeDialogue);
        }
        else
        {
            PlayDialogue(reminderDialogue);
        }
    }

    private void OnIntroComplete()
    {
        OnDialogueComplete.RemoveListener(OnIntroComplete);

        if (questStarted) return;
        if (lastDialogueOutcome != "accepted") return;

        questStarted = true;

        if (questToStart != null)
            QuestManager.Instance.StartQuest(questToStart);
    }

    private void OnQuestComplete()
    {
        OnDialogueComplete.RemoveListener(OnQuestComplete);

        if (hasGivenPackage) return;

        hasGivenPackage = true;

        if (playerInventory != null && packageToGive != null)
            playerInventory.AddPackage(packageToGive);

        if (questToStart != null)
            QuestManager.Instance.CompleteQuest(questToStart);

        ShowInteractionIcon(false);
    }

    public override bool CanInteract()
    {
        return !hasGivenPackage && base.CanInteract();
    }
}

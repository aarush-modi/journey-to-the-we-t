using UnityEngine;

public class VillageElderNPC : NPCBase
{
    [Header("Dialogue")]
    [SerializeField] private NPCDialogue introDialogue;
    [SerializeField] private NPCDialogue reminderDialogue;

    [Header("Quest")]
    [SerializeField] private PackageData packageToGive;
    [SerializeField] private QuestData questToStart;

    private PlayerInventory playerInventory;
    private bool hasGivenPackage;

    public override void Interact(GameObject player)
    {
        if (playerInventory == null)
            playerInventory = player.GetComponent<PlayerInventory>();

        if (!hasGivenPackage)
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

        if (hasGivenPackage) return;

        if (lastDialogueOutcome != "accepted") return;

        hasGivenPackage = true;

        if (playerInventory != null && packageToGive != null)
            playerInventory.AddPackage(packageToGive);

        if (questToStart != null)
            QuestManager.Instance.StartQuest(questToStart);
    }
}

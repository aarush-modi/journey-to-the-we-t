using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CastleWarnerNPC : NPCBase
{
    [Header("Dialogue")]
    [SerializeField] private NPCDialogue introDialogue;
    [SerializeField] private NPCDialogue reminderDialogue;

    [Header("Quest & Package")]
    [SerializeField] private PackageData packageToGive;
    [SerializeField] private QuestData questToStart;

    private PlayerInventory playerInventory;
    private bool hasGivenPackage;

    protected override void Start()
    {
        if (dialoguePanel == null)
            BuildDialogueUI();

        base.Start();
    }
    

    public override void Interact(GameObject player)
    {
        if (playerInventory == null)
            playerInventory = player.GetComponent<PlayerInventory>()
                ?? player.GetComponentInChildren<PlayerInventory>()
                ?? player.GetComponentInParent<PlayerInventory>();

        if (isDialogueActive)
        {
            PlayDialogue(hasGivenPackage ? reminderDialogue : introDialogue);
            return;
        }

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

        hasGivenPackage = true;

        if (playerInventory != null && packageToGive != null)
            playerInventory.AddPackage(packageToGive);

        if (questToStart != null)
            QuestManager.Instance.StartQuest(questToStart);
    }

    private GameObject MakeRect(string name, Transform parent)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        return obj;
    }
}

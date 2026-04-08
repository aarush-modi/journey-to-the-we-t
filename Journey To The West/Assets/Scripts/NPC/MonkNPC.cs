using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MonkNPC : NPCBase
{
    [Header("Dialogue")]
    [SerializeField] private NPCDialogue introDialogue;
    [SerializeField] private NPCDialogue reminderDialogue;
    [SerializeField] private NPCDialogue rewardDialogue;

    [Header("Quest")]
    [SerializeField] private QuestData questToStart;
    [SerializeField] private GameObject shadowEnemy;

    [Header("Reward")]
    [SerializeField] private ItemData charmItem;
    [SerializeField] private float damageBoostMultiplier = 2.5f;

    [Header("Blocker")]
    [SerializeField] private GameObject southVillageBlocker;

    private PlayerInventory playerInventory;
    private PlayerCombat playerCombat;
    private bool hasGivenQuest;
    private bool hasGivenReward;

    protected override void Start()
    {


        base.Start();
    }

    private bool IsShadowDead()
    {
        return shadowEnemy == null || !shadowEnemy.activeInHierarchy;
    }

    public override void Interact(GameObject player)
    {
        if (playerInventory == null)
            playerInventory = player.GetComponent<PlayerInventory>()
                ?? player.GetComponentInChildren<PlayerInventory>()
                ?? player.GetComponentInParent<PlayerInventory>();

        if (playerCombat == null)
            playerCombat = player.GetComponent<PlayerCombat>()
                ?? player.GetComponentInChildren<PlayerCombat>()
                ?? player.GetComponentInParent<PlayerCombat>();

        if (isDialogueActive)
        {
            if (hasGivenReward)
                PlayDialogue(rewardDialogue);
            else if (hasGivenQuest && IsShadowDead())
                PlayDialogue(rewardDialogue);
            else if (hasGivenQuest)
                PlayDialogue(reminderDialogue);
            else
                PlayDialogue(introDialogue);
            return;
        }

        if (hasGivenReward)
        {
            PlayDialogue(rewardDialogue);
            return;
        }

        if (hasGivenQuest && IsShadowDead())
        {
            OnDialogueComplete.AddListener(OnRewardComplete);
            PlayDialogue(rewardDialogue);
            return;
        }

        if (hasGivenQuest)
        {
            PlayDialogue(reminderDialogue);
            return;
        }

        OnDialogueComplete.AddListener(OnIntroComplete);
        PlayDialogue(introDialogue);
    }

    private void OnIntroComplete()
    {
        OnDialogueComplete.RemoveListener(OnIntroComplete);
        Debug.Log($"[Monk] OnIntroComplete fired. hasGivenQuest={hasGivenQuest}, blocker={southVillageBlocker != null}");

        if (hasGivenQuest) return;

        hasGivenQuest = true;

        try
        {
            if (questToStart != null && QuestManager.Instance != null)
                QuestManager.Instance.StartQuest(questToStart);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[Monk] Quest start failed: {e.Message}");
        }

        if (playerCombat != null)
            playerCombat.Heal(playerCombat.GetMaxHP());

        if (southVillageBlocker != null)
        {
            southVillageBlocker.SetActive(false);
            Destroy(southVillageBlocker);
        }
    }

    private void OnRewardComplete()
    {
        OnDialogueComplete.RemoveListener(OnRewardComplete);

        if (hasGivenReward) return;

        hasGivenReward = true;

        if (playerInventory != null && charmItem != null)
            playerInventory.AddItem(charmItem);

        if (playerCombat != null)
            playerCombat.ApplyDamageBoost(damageBoostMultiplier);

        if (questToStart != null)
            QuestManager.Instance.CompleteQuest(questToStart);
    }

    private GameObject MakeRect(string name, Transform parent)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        return obj;
    }
}

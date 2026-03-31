using UnityEngine;

public class MerchantNPC : NPCBase
{
    [Header("Merchant")]
    [SerializeField] private NPCDialogue merchantDialogue;
    [SerializeField] private int shopOpenDialogueIndex = 3;

    private MerchantShopController shopController;
    private bool isOpeningShop = false;

    protected override void Start()
    {
        base.Start();
        shopController = GetComponent<MerchantShopController>();
    }

    public override void Interact(GameObject player)
    {
        PlayDialogue(merchantDialogue);
    }

    protected override bool ShouldUnpauseOnDialogueEnd()
    {
        return !isOpeningShop;
    }

    protected override void ChooseOption(int nextIndex)
    {
        if (nextIndex == shopOpenDialogueIndex && shopController != null)
        {
            ClearChoices();
            EndDialogue();
            shopController.OpenShop();
            return;
        }

        base.ChooseOption(nextIndex);
    }
}
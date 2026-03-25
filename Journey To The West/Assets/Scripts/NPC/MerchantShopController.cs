using UnityEngine;

public class MerchantShopController : MonoBehaviour
{
    public static MerchantShopController Instance;

    [Header("References")]
    [SerializeField] private GameObject shopMenuUI;
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private NPCBase npcBase;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (npcBase == null)
            npcBase = GetComponent<NPCBase>();

        if (shopMenuUI != null)
            shopMenuUI.SetActive(false);
    }

    public void OpenShop()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        PauseController.SetPause(false);

        if (shopMenuUI != null)
            shopMenuUI.SetActive(true);
    }

    public void CloseShop()
    {
        if (shopMenuUI != null)
            shopMenuUI.SetActive(false);

        FindObjectOfType<MerchantNPC>()?.ResumeDialogue();
    }
}
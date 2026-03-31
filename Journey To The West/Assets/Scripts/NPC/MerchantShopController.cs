using UnityEngine;
using System.Collections;

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
        Debug.Log("OpenShop called. shopMenuUI is: " + (shopMenuUI == null ? "NULL" : shopMenuUI.name));
        Debug.Log("shopMenuUI active before: " + (shopMenuUI != null ? shopMenuUI.activeSelf.ToString() : "N/A"));
    
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        PauseController.SetPause(false);

        if (shopMenuUI != null)
        {
            shopMenuUI.SetActive(true);
        }

        Debug.Log("shopMenuUI active after - activeSelf: " + shopMenuUI.activeSelf + " | activeInHierarchy: " + shopMenuUI.activeInHierarchy);
        StartCoroutine(CheckShopNextFrame());
    }

    public void CloseShop()
    {
        Debug.Log("CloseShop called from: " + System.Environment.StackTrace);
        
        if (shopMenuUI != null)
            shopMenuUI.SetActive(false);

        PauseController.SetPause(false);
    }

    private IEnumerator CheckShopNextFrame()
    {
        yield return null; // wait one frame
        Debug.Log("One frame later - activeSelf: " + shopMenuUI.activeSelf + " | activeInHierarchy: " + shopMenuUI.activeInHierarchy);
    }
}
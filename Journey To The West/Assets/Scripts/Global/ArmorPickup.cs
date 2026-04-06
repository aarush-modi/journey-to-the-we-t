using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// World-space armor pickup. Player walks over it to equip the armor.
/// Optionally plays a short dialogue before being destroyed.
/// </summary>
public class ArmorPickup : MonoBehaviour, ICollectible
{
    [SerializeField] private ArmorData armorData;

    [Header("Post-Collect Dialogue (optional)")]
    [SerializeField] private NPCDialogue pickupDialogue;
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private GameObject continuePrompt;

    private bool collected;

    public void Collect(GameObject collector)
    {
        if (collected) return;
        collected = true;

        PlayerCombat combat = collector.GetComponent<PlayerCombat>();
        if (combat != null)
            combat.EquipArmor(armorData);

        if (pickupDialogue != null && dialoguePanel != null && dialogueText != null)
            StartCoroutine(PlayDialogueThenDestroy());
        else
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            Collect(other.gameObject);
    }

    private IEnumerator PlayDialogueThenDestroy()
    {
        PauseController.SetPause(true);
        dialoguePanel.SetActive(true);
        dialoguePanel.transform.SetAsLastSibling();

        for (int i = 0; i < pickupDialogue.dialogue.Length; i++)
        {
            if (nameText != null) nameText.text = pickupDialogue.npcName;
            yield return StartCoroutine(TypeLine(pickupDialogue.dialogue[i], pickupDialogue.typingSpeed));

            if (continuePrompt != null) continuePrompt.SetActive(true);
            yield return new WaitUntil(() => UnityEngine.InputSystem.Keyboard.current.eKey.wasPressedThisFrame);
            if (continuePrompt != null) continuePrompt.SetActive(false);
        }

        dialoguePanel.SetActive(false);
        PauseController.SetPause(false);
        Destroy(gameObject);
    }

    private IEnumerator TypeLine(string line, float speed)
    {
        dialogueText.text = "";
        foreach (char c in line)
        {
            dialogueText.text += c;
            yield return new WaitForSecondsRealtime(speed);
        }
    }
}

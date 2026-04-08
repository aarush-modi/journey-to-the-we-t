using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// World-space armor pickup. Player walks over it to equip the armor.
/// Automatically configures its own SpriteRenderer and trigger collider
/// so it works correctly when spawned at runtime (e.g. dropped by a boss).
/// Optionally plays a short dialogue before being destroyed.
/// </summary>
public class ArmorPickup : MonoBehaviour, ICollectible
{
    [SerializeField] private ArmorData armorData;

    [Header("Visuals")]
    [SerializeField] private Sprite pickupSprite;

    [Header("Post-Collect Dialogue (optional)")]
    [SerializeField] private NPCDialogue pickupDialogue;
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private GameObject continuePrompt;

    private bool collected;

    private void Awake()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null)
            sr = gameObject.AddComponent<SpriteRenderer>();

        if (pickupSprite != null)
            sr.sprite = pickupSprite;
        else if (armorData != null && armorData.armorSprite != null)
            sr.sprite = armorData.armorSprite;

        sr.sortingOrder = 5;

        Collider2D col = GetComponent<Collider2D>();
        if (col == null)
        {
            BoxCollider2D box = gameObject.AddComponent<BoxCollider2D>();
            box.isTrigger = true;
            box.size = new Vector2(1.5f, 1.5f);
        }
        else
        {
            col.isTrigger = true;
        }
    }

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

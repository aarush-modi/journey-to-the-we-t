using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;

public abstract class NPCBase : MonoBehaviour, IInteractable
{
    [Header("NPC Identity")]
    [SerializeField] protected string npcName;
    [SerializeField] protected Sprite faceSprite;

    [Header("Dialogue UI")]
    [SerializeField] protected GameObject dialoguePanel;
    [SerializeField] protected TMP_Text dialogueText;
    [SerializeField] protected TMP_Text nameText;
    [SerializeField] protected Image npcPortraitImage;

    public UnityEvent OnDialogueComplete;

    protected NPCDialogue currentDialogue;
    protected int dialogueIndex;
    protected bool isDialogueActive;
    protected bool isTyping;

    private GameObject playerInRange;

    protected virtual void Start()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }

    protected virtual void Update()
    {
        if (playerInRange == null && !isDialogueActive) return;

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            Interact(playerInRange);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = other.gameObject;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = null;
    }

    public virtual string GetPromptText()
    {
        return isDialogueActive ? "Continue" : $"Talk to {npcName}";
    }

    public abstract void Interact(GameObject player);

    protected void PlayDialogue(NPCDialogue dialogue)
    {
        if (dialogue == null) return;
        if (PauseController.IsGamePaused && !isDialogueActive) return;

        if (isDialogueActive)
        {
            AdvanceLine();
        }
        else
        {
            StartDialogue(dialogue);
        }
    }

    private void StartDialogue(NPCDialogue dialogue)
    {
        currentDialogue = dialogue;
        isDialogueActive = true;
        dialogueIndex = 0;

        nameText.text = dialogue.npcName;
        npcPortraitImage.sprite = dialogue.npcSprite;

        dialoguePanel.SetActive(true);
        PauseController.SetPause(true);

        StartCoroutine(TypeDialogue());
    }

    private void AdvanceLine()
    {
        if (isTyping)
        {
            StopAllCoroutines();
            dialogueText.text = currentDialogue.dialogue[dialogueIndex];
            isTyping = false;
        }
        else if (++dialogueIndex < currentDialogue.dialogue.Length)
        {
            StartCoroutine(TypeDialogue());
        }
        else
        {
            EndDialogue();
        }
    }

    private IEnumerator TypeDialogue()
    {
        isTyping = true;
        dialogueText.text = "";
        foreach (char letter in currentDialogue.dialogue[dialogueIndex].ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSecondsRealtime(currentDialogue.typingSpeed);
        }
        isTyping = false;

        if (currentDialogue.autoProgressLines != null
            && currentDialogue.autoProgressLines.Length > dialogueIndex
            && currentDialogue.autoProgressLines[dialogueIndex])
        {
            yield return new WaitForSecondsRealtime(currentDialogue.autoProgressDelay);
            AdvanceLine();
        }
    }

    protected void EndDialogue()
    {
        StopAllCoroutines();
        isDialogueActive = false;
        dialogueText.text = "";
        dialoguePanel.SetActive(false);
        PauseController.SetPause(false);
        OnDialogueComplete?.Invoke();
    }
}

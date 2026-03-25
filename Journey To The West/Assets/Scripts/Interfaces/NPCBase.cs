using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using UnityEngine.UI;

public abstract class NPCBase : MonoBehaviour, IInteractable
{
    [Header("NPC Identity")]
    [SerializeField] protected string npcName;
    [SerializeField] protected Sprite faceSprite;

    [Header("Interaction")]
    [SerializeField] private GameObject interactionIcon;

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

    protected virtual void Start()
    {
        if (interactionIcon != null)
            interactionIcon.SetActive(false);
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }

    public virtual string GetPromptText()
    {
        return isDialogueActive ? "Continue" : $"Talk to {npcName}";
    }

    public virtual bool CanInteract()
    {
        return !isDialogueActive;
    }

    public virtual void ShowInteractionIcon(bool show)
    {
        if (interactionIcon != null)
            interactionIcon.SetActive(show);
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

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

    [Header("Dialogue Choices")]
    [SerializeField] protected Transform choiceContainer;
    [SerializeField] protected GameObject choiceButtonPrefab;

    public UnityEvent OnDialogueComplete;

    protected NPCDialogue currentDialogue;
    protected int dialogueIndex;
    protected bool isDialogueActive;
    protected bool isTyping;
    private bool isWaitingForChoice;
    protected string lastDialogueOutcome { get; private set; }

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

        ClearChoices();
        DisplayCurrentLine();
    }

    private void AdvanceLine()
    {
        if (isWaitingForChoice) return;

        if (isTyping)
        {
            StopAllCoroutines();
            dialogueText.text = currentDialogue.dialogue[dialogueIndex];
            isTyping = false;
            CheckForChoices();
            return;
        }
        else
        {
            ClearChoices();

            if (currentDialogue.endDialogueOutcomes != null
                && currentDialogue.endDialogueOutcomes.Length > dialogueIndex
                && !string.IsNullOrEmpty(currentDialogue.endDialogueOutcomes[dialogueIndex]))
            {
                EndDialogue();
                return;
            }

            if (CheckForChoices()) return;

            if (currentDialogue.nextLineOverride != null
                && currentDialogue.nextLineOverride.Length > dialogueIndex
                && currentDialogue.nextLineOverride[dialogueIndex] >= 0)
            {
                dialogueIndex = currentDialogue.nextLineOverride[dialogueIndex];
            }
            else
            {
                dialogueIndex++;
            }

            if (dialogueIndex < currentDialogue.dialogue.Length)
            {
                DisplayCurrentLine();
            }
            else
            {
                EndDialogue();
            }
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

    private void DisplayCurrentLine()
    {
        StopAllCoroutines();
        StartCoroutine(TypeDialogue());
    }

    private bool CheckForChoices()
    {
        if (currentDialogue.choices != null)
        {
            foreach (var choice in currentDialogue.choices)
            {
                if (choice.dialogueIndex == dialogueIndex)
                {
                    DisplayChoices(choice);
                    return true;
                }
            }
        }
        return false;
    }

    private void DisplayChoices(DialogueChoice choice)
    {
        isWaitingForChoice = true;
        for (int i = 0; i < choice.choices.Length; i++)
        {
            int nextIndex = choice.nextDialogueIndexes[i];
            CreateChoiceButton(choice.choices[i], nextIndex);
        }
    }

    private void CreateChoiceButton(string text, int nextIndex)
    {
        if (choiceButtonPrefab == null || choiceContainer == null) return;

        GameObject button = Instantiate(choiceButtonPrefab, choiceContainer);

        var buttonText = button.GetComponentInChildren<TMP_Text>();
        if (buttonText != null)
            buttonText.text = text;

        var buttonComponent = button.GetComponent<Button>();
        if (buttonComponent != null)
            buttonComponent.onClick.AddListener(() => ChooseOption(nextIndex));
    }

    private void ChooseOption(int nextIndex)
    {
        dialogueIndex = nextIndex;
        ClearChoices();
        DisplayCurrentLine();
    }

    private void ClearChoices()
    {
        if (choiceContainer == null) return;
        foreach (Transform child in choiceContainer)
        {
            Destroy(child.gameObject);
        }
        isWaitingForChoice = false;
    }

    protected void EndDialogue()
    {
        StopAllCoroutines();
        ClearChoices();

        lastDialogueOutcome = "";
        if (currentDialogue.endDialogueOutcomes != null
            && currentDialogue.endDialogueOutcomes.Length > dialogueIndex)
        {
            lastDialogueOutcome = currentDialogue.endDialogueOutcomes[dialogueIndex];
        }

        isDialogueActive = false;
        dialogueText.text = "";
        dialoguePanel.SetActive(false);
        PauseController.SetPause(false);
        OnDialogueComplete?.Invoke();
    }
}

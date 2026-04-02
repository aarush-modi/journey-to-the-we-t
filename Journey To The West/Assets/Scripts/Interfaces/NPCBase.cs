using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

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
    [SerializeField] private GameObject continuePrompt;

    [Header("Dialogue Choices")]
    [SerializeField] protected Transform choiceContainer;
    [SerializeField] protected GameObject choiceButtonPrefab;

    public UnityEvent OnDialogueComplete;

    protected NPCDialogue currentDialogue;
    protected int dialogueIndex;
    protected bool isDialogueActive;
    protected bool isTyping;
    private bool isWaitingForChoice;
    private PlayerCombat activeDialogueCombat;
    private bool disabledCombatForDialogue;
    protected string lastDialogueOutcome { get; private set; }

    protected virtual void Start()
    {
        if (interactionIcon != null)
            interactionIcon.SetActive(false);
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
        ConfigureDialogueRaycasts();
    }

    public virtual string GetPromptText()
    {
        return isDialogueActive ? "Continue" : $"Talk to {npcName}";
    }

    public bool IsDialogueOpen => isDialogueActive;

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
        dialoguePanel.transform.SetAsLastSibling();
        PauseController.SetPause(true);
        SetDialogueCombatEnabled(false);

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
            if (!CheckForChoices())
                SetContinuePrompt(true);
            return;
        }
        else
        {
            SetContinuePrompt(false);
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

    private void SetContinuePrompt(bool show)
    {
        if (continuePrompt != null)
            continuePrompt.SetActive(show);
    }

    private IEnumerator TypeDialogue()
    {
        isTyping = true;
        SetContinuePrompt(false);
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
        else
        {
            SetContinuePrompt(true);
        }
    }

    protected void DisplayCurrentLine()
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
        Button firstButton = null;
        for (int i = 0; i < choice.choices.Length; i++)
        {
            int nextIndex = choice.nextDialogueIndexes[i];
            Button button = CreateChoiceButton(choice.choices[i], nextIndex);
            if (firstButton == null)
                firstButton = button;
        }

        if (firstButton != null && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(firstButton.gameObject);
            firstButton.Select();
        }
    }

    private Button CreateChoiceButton(string text, int nextIndex)
    {
        if (choiceButtonPrefab == null || choiceContainer == null) return null;

        GameObject button = Instantiate(choiceButtonPrefab, choiceContainer);

        var buttonText = button.GetComponentInChildren<TMP_Text>();
        if (buttonText != null)
            buttonText.text = text;

        var buttonComponent = button.GetComponent<Button>();
        if (buttonComponent != null)
            buttonComponent.onClick.AddListener(() => ChooseOption(nextIndex));

        return buttonComponent;
    }

    private void ConfigureDialogueRaycasts()
    {
        if (dialogueText != null)
            dialogueText.raycastTarget = false;
        if (nameText != null)
            nameText.raycastTarget = false;
        if (npcPortraitImage != null)
            npcPortraitImage.raycastTarget = false;
        if (continuePrompt != null && continuePrompt.TryGetComponent<Graphic>(out var graphic))
            graphic.raycastTarget = false;
    }

    protected virtual void ChooseOption(int nextIndex)
    {
        dialogueIndex = nextIndex;
        ClearChoices();
        DisplayCurrentLine();
    }

    protected void ClearChoices()
    {
        if (choiceContainer == null) return;
        foreach (Transform child in choiceContainer)
        {
            Destroy(child.gameObject);
        }
        isWaitingForChoice = false;
    }

    protected virtual bool ShouldUnpauseOnDialogueEnd()
    {
        return true;
    }

    protected void EndDialogue(bool stopCoroutines = true)
    {
        if (stopCoroutines)
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
        SetDialogueCombatEnabled(true);
        if (ShouldUnpauseOnDialogueEnd())
        {
            PauseController.SetPause(false);
        }
        OnDialogueComplete?.Invoke();
    }

    public void ResumeDialogue()
    {
        isDialogueActive = true;
        dialoguePanel.SetActive(true);
        dialoguePanel.transform.SetAsLastSibling();
        PauseController.SetPause(true);
        SetDialogueCombatEnabled(false);
        DisplayCurrentLine();
    }

    private void SetDialogueCombatEnabled(bool enabled)
    {
        if (!enabled)
        {
            if (activeDialogueCombat == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                    activeDialogueCombat = player.GetComponent<PlayerCombat>();
            }

            if (activeDialogueCombat != null && activeDialogueCombat.enabled)
            {
                activeDialogueCombat.enabled = false;
                disabledCombatForDialogue = true;
            }
            return;
        }

        if (activeDialogueCombat != null && disabledCombatForDialogue)
        {
            activeDialogueCombat.enabled = true;
        }

        disabledCombatForDialogue = false;
        activeDialogueCombat = null;
    }
}

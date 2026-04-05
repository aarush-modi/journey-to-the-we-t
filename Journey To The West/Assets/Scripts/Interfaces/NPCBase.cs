using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public abstract class NPCBase : MonoBehaviour, IInteractable
{
    public static NPCBase CurrentDialogueNpc { get; protected set; }

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
    private List<int> choiceNextIndexes = new List<int>();
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

    protected void EnsureDialogueReferencesFromScene()
    {
        CopyDialogueReferencesFromExistingNpc();

        GameObject[] sceneObjects = Resources.FindObjectsOfTypeAll<GameObject>();

        if (dialoguePanel == null)
        {
            foreach (GameObject sceneObject in sceneObjects)
            {
                if (sceneObject.name == "DialoguePanel" && sceneObject.scene.IsValid())
                {
                    dialoguePanel = sceneObject;
                    break;
                }
            }
        }

        if (dialoguePanel == null)
        {
            return;
        }

        if (dialogueText == null || nameText == null)
        {
            TMP_Text dialogueTextComponent = FindTextInDialoguePanel("DialogueText");
            if (dialogueText == null && dialogueTextComponent != null)
            {
                dialogueText = dialogueTextComponent;
            }

            TMP_Text nameTextComponent = FindTextInDialoguePanel("NameText");
            if (nameTextComponent == null)
            {
                nameTextComponent = FindTextInDialoguePanel("NPCNameText");
            }
            if (nameText == null && nameTextComponent != null)
            {
                nameText = nameTextComponent;
            }
        }

        if (npcPortraitImage == null)
        {
            Image portraitImage = FindImageInDialoguePanel("DialoguePortrait");
            if (portraitImage != null)
            {
                npcPortraitImage = portraitImage;
            }
        }

        if (continuePrompt == null)
        {
            Transform continuePromptTransform = FindChildRecursive(dialoguePanel != null ? dialoguePanel.transform : null, "ContinuePrompt");
            continuePrompt = continuePromptTransform != null ? continuePromptTransform.gameObject : null;
        }

        if (choiceContainer == null)
        {
            choiceContainer = FindChildRecursive(dialoguePanel != null ? dialoguePanel.transform : null, "ChoiceContainer");
        }

        if (choiceButtonPrefab == null)
        {
            CopyDialogueReferencesFromExistingNpc();
        }

        ConfigureDialogueRaycasts();
    }

    private void CopyDialogueReferencesFromExistingNpc()
    {
        foreach (NPCBase npc in Resources.FindObjectsOfTypeAll<NPCBase>())
        {
            if (npc == null || npc == this || !npc.gameObject.scene.IsValid())
            {
                continue;
            }

            if (dialoguePanel == null && npc.dialoguePanel != null)
            {
                dialoguePanel = npc.dialoguePanel;
            }

            if (dialogueText == null && npc.dialogueText != null)
            {
                dialogueText = npc.dialogueText;
            }

            if (nameText == null && npc.nameText != null)
            {
                nameText = npc.nameText;
            }

            if (npcPortraitImage == null && npc.npcPortraitImage != null)
            {
                npcPortraitImage = npc.npcPortraitImage;
            }

            if (continuePrompt == null && npc.continuePrompt != null)
            {
                continuePrompt = npc.continuePrompt;
            }

            if (choiceContainer == null && npc.choiceContainer != null)
            {
                choiceContainer = npc.choiceContainer;
            }

            if (choiceButtonPrefab == null && npc.choiceButtonPrefab != null)
            {
                choiceButtonPrefab = npc.choiceButtonPrefab;
            }

            if (dialoguePanel != null
                && dialogueText != null
                && nameText != null
                && npcPortraitImage != null
                && continuePrompt != null
                && choiceContainer != null
                && choiceButtonPrefab != null)
            {
                return;
            }
        }
    }

    private TMP_Text FindTextInDialoguePanel(string objectName)
    {
        if (dialoguePanel == null)
        {
            return null;
        }

        Transform child = FindChildRecursive(dialoguePanel.transform, objectName);
        return child != null ? child.GetComponent<TMP_Text>() : null;
    }

    private Image FindImageInDialoguePanel(string objectName)
    {
        if (dialoguePanel == null)
        {
            return null;
        }

        Transform child = FindChildRecursive(dialoguePanel.transform, objectName);
        return child != null ? child.GetComponent<Image>() : null;
    }

    private Transform FindChildRecursive(Transform parent, string objectName)
    {
        if (parent == null)
        {
            return null;
        }

        if (parent.name == objectName)
        {
            return parent;
        }

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform result = FindChildRecursive(parent.GetChild(i), objectName);
            if (result != null)
            {
                return result;
            }
        }

        return null;
    }

    protected virtual void Update()
    {
        if (!isWaitingForChoice) return;
        if (Keyboard.current == null) return;

        for (int i = 0; i < choiceNextIndexes.Count && i < 9; i++)
        {
            if (Keyboard.current[(Key)((int)Key.Digit1 + i)].wasPressedThisFrame)
            {
                ChooseOption(choiceNextIndexes[i]);
                return;
            }
        }
    }

    protected virtual void OnDisable()
    {
        if (CurrentDialogueNpc == this)
        {
            CurrentDialogueNpc = null;
        }
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
        EnsureDialogueReferencesFromScene();

        if (dialoguePanel == null || dialogueText == null || nameText == null || npcPortraitImage == null)
        {
            Debug.LogWarning($"[NPCBase] Missing dialogue UI references for {name}. Dialogue aborted.", this);
            return;
        }

        currentDialogue = dialogue;
        isDialogueActive = true;
        dialogueIndex = 0;
        CurrentDialogueNpc = this;

        nameText.enableWordWrapping = false;
        nameText.overflowMode = TextOverflowModes.Overflow;
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

        if (CheckForChoices())
        {
            yield break;
        }

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
        choiceNextIndexes.Clear();
        Button firstButton = null;
        for (int i = 0; i < choice.choices.Length; i++)
        {
            int nextIndex = choice.nextDialogueIndexes[i];
            choiceNextIndexes.Add(nextIndex);
            Button button = CreateChoiceButton(choice.choices[i], nextIndex, i + 1);
            if (firstButton == null)
                firstButton = button;
        }

        if (firstButton != null && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(firstButton.gameObject);
            firstButton.Select();
        }
    }

    private Button CreateChoiceButton(string text, int nextIndex, int displayNumber)
    {
        if (choiceButtonPrefab == null || choiceContainer == null) return null;

        GameObject button = Instantiate(choiceButtonPrefab, choiceContainer);

        var buttonText = button.GetComponentInChildren<TMP_Text>();
        if (buttonText != null)
            buttonText.text = $"[{displayNumber}] {text}";

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
        if (CurrentDialogueNpc == this)
        {
            CurrentDialogueNpc = null;
        }
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
        CurrentDialogueNpc = this;
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

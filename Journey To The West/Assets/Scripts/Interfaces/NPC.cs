using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class NPC : MonoBehaviour, IInteractable
{
    public NPCDialogue dialogue;
    public GameObject dialoguePanel;
    public TMP_Text dialogueText, nameText;
    public Image npcPortraitImage;

    private int dialogueIndex;
    private bool isDialogueActive, isTyping;

    void Start()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }

    public string GetPromptText()
    {
        if (dialogue == null)
            return string.Empty;
        return isDialogueActive ? "Continue" : $"Talk to {dialogue.npcName}";
    }

    public void Interact(GameObject player)
    {
        if (dialogue == null || (PauseController.IsGamePaused && !isDialogueActive))
            return;

        if (isDialogueActive)
            nextLine();
        else
            StartDialogue();
    }

    void StartDialogue()
    {
        isDialogueActive = true;
        dialogueIndex = 0;

        nameText.text = dialogue.npcName;
        npcPortraitImage.sprite = dialogue.npcSprite;

        dialoguePanel.SetActive(true);
        PauseController.SetPause(true);

        StartCoroutine(TypeDialogue());
    }

    void nextLine()
    {
        if (isTyping)
        {
            StopAllCoroutines();
            dialogueText.text = dialogue.dialogue[dialogueIndex];
            isTyping = false;
        }
        else if (++dialogueIndex < dialogue.dialogue.Length)
        {
            StartCoroutine(TypeDialogue());
        }
        else
        {
            EndDialogue();
        }
    }

    IEnumerator TypeDialogue()
    {
        isTyping = true;
        dialogueText.text = "";
        foreach (char letter in dialogue.dialogue[dialogueIndex].ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSecondsRealtime(dialogue.typingSpeed);
        }
        isTyping = false;

        if (dialogue.autoProgressLines != null
            && dialogue.autoProgressLines.Length > dialogueIndex
            && dialogue.autoProgressLines[dialogueIndex])
        {
            yield return new WaitForSecondsRealtime(dialogue.autoProgressDelay);
            nextLine();
        }
    }

    public void EndDialogue()
    {
        StopAllCoroutines();
        isDialogueActive = false;
        dialogueText.text = "";
        dialoguePanel.SetActive(false);
        PauseController.SetPause(false);
    }
}

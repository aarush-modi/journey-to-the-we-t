using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class KingModiBlackjackNPC : NPCBase
{
    private enum ModiState
    {
        Closed,
        WaitingForNotice,
        IntroDialogue,
        WaitingForGambleChoice,
        RefusedGamble,
        Blackjack
    }

    private static readonly string[] IntroDialogueLines =
    {
        "*King Modi rolls out of his pile of 1-Yen notes to address you.*",
        "MONEY MONEY MONEY.\nI LOOOOOVE MONEY.",
        "I AM THE RICHEST MAN ON EARTH, KING MODI.",
        "I POSSESS THE RED PACKET YOU DESIRE, NINJA OF GREED.",
        "I SEE YOU HAVE SOMETHING I DESIRE.",
        "*Modi eyes the 1-Yen coin in your hand.*",
        "TELL ME, ARE YOU A GAMBLING MAN?"
    };

    private readonly BlackjackRound blackjackRound = new BlackjackRound();
    private ModiState modiState = ModiState.Closed;
    private int introDialogueIndex;

    public override void Interact(GameObject player)
    {
        if (modiState == ModiState.Blackjack)
        {
            return;
        }

        if (!isDialogueActive)
        {
            StartModiIntro();
            return;
        }

        if (modiState == ModiState.IntroDialogue)
        {
            AdvanceIntroDialogue();
        }
    }

    private void StartModiIntro()
    {
        isDialogueActive = true;
        CurrentDialogueNpc = this;
        modiState = ModiState.WaitingForNotice;
        introDialogueIndex = 0;

        if (nameText != null)
        {
            nameText.enableWordWrapping = false;
            nameText.overflowMode = TextOverflowModes.Overflow;
            nameText.text = npcName;
        }

        if (npcPortraitImage != null)
        {
            npcPortraitImage.sprite = faceSprite;
        }

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
            dialoguePanel.transform.SetAsLastSibling();
        }

        PauseController.SetPause(true);
        ShowNoticePrompt();
    }

    private void ShowNoticePrompt()
    {
        StopAllCoroutines();
        ClearChoices();

        if (dialogueText != null)
        {
            dialogueText.text = "*You feel a overwhelming surge of greed pulse through your body.*\n" +
                                "*Suddenly, even all the money in the world is not enough for you.*";
        }

        FocusButton(CreateChoiceButton("Wait for Modi to notice you", BeginIntroDialogue));
    }

    private void BeginIntroDialogue()
    {
        modiState = ModiState.IntroDialogue;
        introDialogueIndex = 0;
        ShowCurrentIntroLine();
    }

    private void AdvanceIntroDialogue()
    {
        introDialogueIndex++;

        if (introDialogueIndex >= IntroDialogueLines.Length)
        {
            ShowGambleChoice();
            return;
        }

        ShowCurrentIntroLine();
    }

    private void ShowCurrentIntroLine()
    {
        StopAllCoroutines();
        ClearChoices();

        if (dialogueText != null)
        {
            dialogueText.text = IntroDialogueLines[introDialogueIndex];
        }
    }

    private void ShowGambleChoice()
    {
        StopAllCoroutines();
        ClearChoices();
        modiState = ModiState.WaitingForGambleChoice;

        if (dialogueText != null)
        {
            dialogueText.text = "TELL ME, ARE YOU A GAMBLING MAN?";
        }

        FocusButton(CreateChoiceButton("Yes", StartBlackjackRound));
        CreateChoiceButton("No", ShowRefusedGambleLine);
    }

    private void ShowRefusedGambleLine()
    {
        StopAllCoroutines();
        ClearChoices();
        modiState = ModiState.RefusedGamble;

        if (dialogueText != null)
        {
            dialogueText.text = "*Modi takes all of your money anyways.*";
        }

        FocusButton(CreateChoiceButton("Leave", EndBlackjackDialogue));
    }

    private void StartBlackjackRound()
    {
        StopAllCoroutines();
        ClearChoices();

        modiState = ModiState.Blackjack;
        isDialogueActive = true;
        blackjackRound.ResetRound();
        RefreshBlackjackUI();
    }

    private void RefreshBlackjackUI()
    {
        StopAllCoroutines();
        ClearChoices();

        if (dialogueText != null)
        {
            dialogueText.text = blackjackRound.RenderRoundState();
        }

        if (blackjackRound.IsGameOver)
        {
            Button replayButton = CreateChoiceButton("Play Again", StartBlackjackRound);
            CreateChoiceButton("Leave", EndBlackjackDialogue);
            FocusButton(replayButton);
            return;
        }

        Button hitButton = CreateChoiceButton("Hit", () =>
        {
            blackjackRound.Hit();
            RefreshBlackjackUI();
        });

        CreateChoiceButton("Stand", () =>
        {
            blackjackRound.Stand();
            RefreshBlackjackUI();
        });

        FocusButton(hitButton);
    }

    private Button CreateChoiceButton(string label, UnityEngine.Events.UnityAction onClick)
    {
        if (choiceButtonPrefab == null || choiceContainer == null) return null;

        GameObject buttonObject = Instantiate(choiceButtonPrefab, choiceContainer);

        if (buttonObject.TryGetComponent(out Button button))
        {
            button.onClick.AddListener(onClick);
        }

        TMP_Text buttonText = buttonObject.GetComponentInChildren<TMP_Text>();
        if (buttonText != null)
        {
            buttonText.text = label;
        }

        return button;
    }

    private static void FocusButton(Button button)
    {
        if (button == null || EventSystem.current == null) return;

        EventSystem.current.SetSelectedGameObject(button.gameObject);
        button.Select();
    }

    private void EndBlackjackDialogue()
    {
        StopAllCoroutines();
        ClearChoices();

        if (dialogueText != null)
        {
            dialogueText.text = "";
        }

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        modiState = ModiState.Closed;
        introDialogueIndex = 0;
        isDialogueActive = false;
        if (CurrentDialogueNpc == this)
        {
            CurrentDialogueNpc = null;
        }
        PauseController.SetPause(false);
        ShowInteractionIcon(true);
    }
}

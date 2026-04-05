using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class KingModiBlackjackNPC : NPCBase, IDamageable
{
    private const float ModiMaxHP = 1f;
    private const int ModiFortuneGreedAmount = 1000;
    private const int BlackjackLossGreedPenalty = 100;
    private const int PostWinTalkGreedPenalty = 5;
    private static bool hasRedPacketThisSession;

    private enum ModiState
    {
        Closed,
        WaitingForNotice,
        IntroDialogue,
        WaitingForGambleChoice,
        AcceptedGambleIntro,
        NoMoneyDialogue,
        RefusedGamble,
        Blackjack,
        WinDialogue,
        LossDialogue,
        PostWinDialogue,
        DeathLootDialogue
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

    private static readonly string[] AcceptedGambleLines =
    {
        "PERFECT.",
        "LET'S PLAY BLACKJACK THEN."
    };

    private static readonly string[] LossDialogueLines =
    {
        "*Modi grins ear to ear as he takes your money.*",
        "MONEY MONEY MONEY!!!",
        "TELL ME, JIN. WOULD YOU LIKE TO KEEP LOSING TO ME?"
    };

    private static readonly string[] WinDialogueLines =
    {
        "*Modi sighs as he hands you the red packet.\nIt seems as if the packet was almost as important to him as money.*",
        "*Modi also hands you the all of the money in the Merchant Kingdom.\nYou feel incredibly rich after taking the fortune of the Modi family.*"
    };

    private static readonly string[] PostWinDialogueLines =
    {
        "What do you want from me, you greedy boy?",
        "*Modi takes 5 Yen from you just for bothering him.*"
    };

    private readonly BlackjackRound blackjackRound = new BlackjackRound();
    private ModiState modiState = ModiState.Closed;
    private int introDialogueIndex;
    private int acceptedGambleLineIndex;
    private int winDialogueIndex;
    private int lossDialogueIndex;
    private int postWinDialogueIndex;
    private GreedMeter playerGreedMeter;
    private float currentHP = ModiMaxHP;
    private bool isDead;
    private bool hasPlayerWonPacket;
    private bool isWaitingForLossChoice;

    public static bool HasRedPacket => hasRedPacketThisSession;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetRedPacketState()
    {
        hasRedPacketThisSession = false;
    }

    public override void Interact(GameObject player)
    {
        if (isDead)
        {
            if (modiState == ModiState.DeathLootDialogue)
            {
                EndBlackjackDialogue();
            }

            return;
        }

        if (playerGreedMeter == null && player != null)
        {
            playerGreedMeter = player.GetComponent<GreedMeter>();
        }

        if (modiState == ModiState.Blackjack)
        {
            return;
        }

        if (!isDialogueActive)
        {
            if (hasPlayerWonPacket)
            {
                Debug.Log("[KingModi] Starting post-win dialogue.");
                ShowPostWinDialogue();
                return;
            }

            if (playerGreedMeter != null && playerGreedMeter.GetCurrentGold() <= 0)
            {
                ShowNoMoneyDialogue();
                return;
            }

            StartModiIntro();
            return;
        }

        if (modiState == ModiState.IntroDialogue)
        {
            AdvanceIntroDialogue();
            return;
        }

        if (modiState == ModiState.AcceptedGambleIntro)
        {
            AdvanceAcceptedGambleDialogue();
            return;
        }

        if (modiState == ModiState.WinDialogue)
        {
            AdvanceWinDialogue();
            return;
        }

        if (modiState == ModiState.NoMoneyDialogue)
        {
            EndBlackjackDialogue();
            return;
        }

        if (modiState == ModiState.PostWinDialogue)
        {
            AdvancePostWinDialogue();
            return;
        }

        if (modiState == ModiState.LossDialogue && !isWaitingForLossChoice)
        {
            AdvanceLossDialogue();
        }
    }

    public void TakeDamage(float amount)
    {
        if (isDead)
        {
            return;
        }

        currentHP -= amount;
        if (currentHP <= 0f)
        {
            Die();
        }
    }

    public void Die()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;
        currentHP = 0f;

        foreach (Collider2D modiCollider in GetComponents<Collider2D>())
        {
            modiCollider.enabled = false;
        }

        SpriteRenderer modiSprite = GetComponent<SpriteRenderer>();
        if (modiSprite != null)
        {
            modiSprite.enabled = false;
        }

        ModiGuard.AlertAllGuards();
        ShowDeathLootDialogue();
    }

    public bool IsDead() => isDead;

    private void SetPlayerGreedToModiFortune()
    {
        if (playerGreedMeter == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerGreedMeter = player.GetComponent<GreedMeter>();
            }
        }

        if (playerGreedMeter == null)
        {
            return;
        }

        int goldDelta = ModiFortuneGreedAmount - playerGreedMeter.GetCurrentGold();
        if (goldDelta > 0)
        {
            playerGreedMeter.AddGold(goldDelta);
        }
        else if (goldDelta < 0)
        {
            playerGreedMeter.RemoveGold(-goldDelta);
        }
    }

    private void StartModiIntro()
    {
        isDialogueActive = true;
        CurrentDialogueNpc = this;
        modiState = ModiState.WaitingForNotice;
        introDialogueIndex = 0;
        isWaitingForLossChoice = false;

        SetSpeakerChromeVisible(true);

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

        FocusButton(CreateChoiceButton("Yes", BeginAcceptedGambleDialogue));
        CreateChoiceButton("No", ShowRefusedGambleLine);
    }

    private void BeginAcceptedGambleDialogue()
    {
        StopAllCoroutines();
        ClearChoices();
        modiState = ModiState.AcceptedGambleIntro;
        acceptedGambleLineIndex = 0;
        ShowCurrentAcceptedGambleLine();
    }

    private void AdvanceAcceptedGambleDialogue()
    {
        acceptedGambleLineIndex++;

        if (acceptedGambleLineIndex >= AcceptedGambleLines.Length)
        {
            StartBlackjackRound();
            return;
        }

        ShowCurrentAcceptedGambleLine();
    }

    private void ShowCurrentAcceptedGambleLine()
    {
        StopAllCoroutines();
        ClearChoices();

        if (dialogueText != null)
        {
            dialogueText.text = AcceptedGambleLines[acceptedGambleLineIndex];
        }
    }

    private void ShowRefusedGambleLine()
    {
        StopAllCoroutines();
        ClearChoices();
        modiState = ModiState.RefusedGamble;

        if (playerGreedMeter != null)
        {
            playerGreedMeter.RemoveGold(playerGreedMeter.GetCurrentGold());
        }

        if (dialogueText != null)
        {
            dialogueText.text = "*Modi takes all of your money anyways.*";
        }

        FocusButton(CreateChoiceButton("Leave", EndBlackjackDialogue));
    }

    private void ShowNoMoneyDialogue()
    {
        StopAllCoroutines();
        ClearChoices();
        modiState = ModiState.NoMoneyDialogue;
        isDialogueActive = true;
        CurrentDialogueNpc = this;
        isWaitingForLossChoice = false;

        SetSpeakerChromeVisible(true);

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

        if (dialogueText != null)
        {
            dialogueText.text = "*Modi ignores you, knowing you have no money for him*";
        }

        PauseController.SetPause(true);
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
            ShowFinishedRoundDialogue();
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

    private void ShowFinishedRoundDialogue()
    {
        StopAllCoroutines();
        ClearChoices();

        if (blackjackRound.Outcome == BlackjackOutcome.PlayerWin)
        {
            hasPlayerWonPacket = true;
            hasRedPacketThisSession = true;
            SetPlayerGreedToModiFortune();
            ModiGuard.AlertAllGuards();
            modiState = ModiState.WinDialogue;
            winDialogueIndex = 0;
            Debug.Log("[KingModi] Player won blackjack. Post-win dialogue unlocked.");
            ShowCurrentWinDialogueLine();
            return;
        }

        if (blackjackRound.Outcome == BlackjackOutcome.DealerWin)
        {
            if (playerGreedMeter != null)
            {
                playerGreedMeter.RemoveGold(BlackjackLossGreedPenalty);
            }

            modiState = ModiState.LossDialogue;
            lossDialogueIndex = -1;
            Debug.Log($"[KingModi] Player lost blackjack. Reason line: {blackjackRound.RenderLossReason()}");
            ShowCurrentLossDialogueLine();
            return;
        }

        if (dialogueText != null)
        {
            dialogueText.text = blackjackRound.RenderRoundState();
        }

        Button replayButton = CreateChoiceButton("Play Again", StartBlackjackRound);
        CreateChoiceButton("Leave", EndBlackjackDialogue);
        FocusButton(replayButton);
    }

    private void AdvanceLossDialogue()
    {
        lossDialogueIndex++;
        if (lossDialogueIndex >= LossDialogueLines.Length)
        {
            lossDialogueIndex = LossDialogueLines.Length - 1;
            ShowCurrentLossDialogueLine();
            return;
        }

        ShowCurrentLossDialogueLine();
    }

    private void AdvanceWinDialogue()
    {
        winDialogueIndex++;
        if (winDialogueIndex >= WinDialogueLines.Length)
        {
            EndBlackjackDialogue();
            return;
        }

        ShowCurrentWinDialogueLine();
    }

    private void ShowCurrentWinDialogueLine()
    {
        StopAllCoroutines();
        ClearChoices();

        if (dialogueText != null)
        {
            dialogueText.text = WinDialogueLines[winDialogueIndex];
        }
    }

    private void ShowCurrentLossDialogueLine()
    {
        StopAllCoroutines();
        ClearChoices();
        isWaitingForLossChoice = lossDialogueIndex == LossDialogueLines.Length - 1;

        if (dialogueText != null)
        {
            dialogueText.text = lossDialogueIndex < 0
                ? blackjackRound.RenderLossReason()
                : LossDialogueLines[lossDialogueIndex];
        }

        if (!isWaitingForLossChoice)
        {
            return;
        }

        FocusButton(CreateChoiceButton("Yes", StartBlackjackRound));
        CreateChoiceButton("No", EndBlackjackDialogue);
    }

    private void ShowPostWinDialogue()
    {
        StopAllCoroutines();
        ClearChoices();
        modiState = ModiState.PostWinDialogue;
        postWinDialogueIndex = 0;
        isDialogueActive = true;
        CurrentDialogueNpc = this;
        isWaitingForLossChoice = false;

        SetSpeakerChromeVisible(true);

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

        ShowCurrentPostWinDialogueLine();
        PauseController.SetPause(true);
    }

    private void AdvancePostWinDialogue()
    {
        postWinDialogueIndex++;
        if (postWinDialogueIndex >= PostWinDialogueLines.Length)
        {
            EndBlackjackDialogue();
            return;
        }

        ShowCurrentPostWinDialogueLine();
    }

    private void ShowCurrentPostWinDialogueLine()
    {
        StopAllCoroutines();
        ClearChoices();

        if (dialogueText != null)
        {
            dialogueText.text = PostWinDialogueLines[postWinDialogueIndex];
        }

        if (postWinDialogueIndex == 1 && playerGreedMeter != null)
        {
            Debug.Log("[KingModi] Deducting 5 greed for post-win interaction.");
            playerGreedMeter.RemoveGold(PostWinTalkGreedPenalty);
        }
    }

    private void ShowDeathLootDialogue()
    {
        StopAllCoroutines();
        ClearChoices();
        isDialogueActive = true;
        CurrentDialogueNpc = this;
        modiState = ModiState.DeathLootDialogue;
        isWaitingForLossChoice = false;

        SetSpeakerChromeVisible(false);

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
            dialoguePanel.transform.SetAsLastSibling();
        }

        if (dialogueText != null)
        {
            dialogueText.text = hasPlayerWonPacket
                ? "You kill Modi for absolutely no reason."
                : "You take the red packet and great fortune of Modi from his corpse.";
        }

        hasRedPacketThisSession = true;
        SetPlayerGreedToModiFortune();

        PauseController.SetPause(true);
    }

    private void SetSpeakerChromeVisible(bool show)
    {
        if (nameText != null)
        {
            nameText.gameObject.SetActive(show);
            if (show)
            {
                nameText.text = npcName;
            }
        }

        if (npcPortraitImage != null)
        {
            npcPortraitImage.gameObject.SetActive(show);
            if (show)
            {
                npcPortraitImage.sprite = faceSprite;
            }
        }
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
        acceptedGambleLineIndex = 0;
        winDialogueIndex = 0;
        lossDialogueIndex = 0;
        postWinDialogueIndex = 0;
        isWaitingForLossChoice = false;
        isDialogueActive = false;
        if (CurrentDialogueNpc == this)
        {
            CurrentDialogueNpc = null;
        }
        SetSpeakerChromeVisible(true);
        PauseController.SetPause(false);

        if (isDead)
        {
            gameObject.SetActive(false);
            return;
        }

        ShowInteractionIcon(true);
    }
}

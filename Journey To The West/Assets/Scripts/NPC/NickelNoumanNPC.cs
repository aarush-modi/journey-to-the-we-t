using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class NickelNoumanNPC : NPCBase, IDamageable
{
    private const float NickelMaxHP = 1f;
    private const int CorrectAnswerLineIndex = 13;
    private const int FirstAnswerResponseLineIndex = 13;
    private const int LastAnswerResponseLineIndex = 16;
    private const string CorrectAnswerOutcome = "answer_moneygrubber";
    private static bool isTeleporterUnlockedThisSession;
    private static bool isNickelDeadThisSession;

    [Header("Dialogue")]
    [SerializeField] private NPCDialogue dialogue;

    [Header("Locked Teleporter Emote")]
    [SerializeField] private Sprite lockedTeleporterEmote;
    [SerializeField] private Vector3 lockedTeleporterEmoteOffset = new Vector3(0f, 1.25f, 0f);
    [SerializeField] private float lockedTeleporterEmoteDuration = 0.75f;

    public bool IsTeleporterUnlocked => isTeleporterUnlockedThisSession || isNickelDeadThisSession;
    public bool IsNickelDead => isNickelDeadThisSession;

    private SpriteRenderer lockedTeleporterEmoteRenderer;
    private Coroutine lockedTeleporterEmoteRoutine;
    private int pendingAnswerResponseLineIndex = -1;
    private bool isRedPacketEscapeWarningActive;
    private bool isDeathDialogueActive;
    private Action onRedPacketEscapeWarningComplete;
    private float currentHP = NickelMaxHP;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetTeleporterUnlockState()
    {
        isTeleporterUnlockedThisSession = false;
        isNickelDeadThisSession = false;
    }

    protected override void Start()
    {
        base.Start();
        BuildLockedTeleporterEmote();
    }

    private void Update()
    {
        if (Keyboard.current == null
            || (!Keyboard.current.digit9Key.wasPressedThisFrame && !Keyboard.current.numpad9Key.wasPressedThisFrame))
        {
            return;
        }

        SetTeleporterUnlocked(!IsTeleporterUnlocked);
        Debug.Log($"[NickelNouman] Debug toggle with 9 pressed. Teleporter 1+ unlocked = {IsTeleporterUnlocked}", this);
    }

    public override void Interact(GameObject player)
    {
        if (isDeathDialogueActive)
        {
            FinishDeathDialogue();
            return;
        }

        if (isRedPacketEscapeWarningActive)
        {
            FinishRedPacketEscapeWarning();
            return;
        }

        if (isDialogueActive
            && !isTyping
            && dialogueIndex == pendingAnswerResponseLineIndex
            && pendingAnswerResponseLineIndex >= FirstAnswerResponseLineIndex
            && pendingAnswerResponseLineIndex <= LastAnswerResponseLineIndex)
        {
            pendingAnswerResponseLineIndex = -1;
            EndDialogue();
            return;
        }

        OnDialogueComplete.RemoveListener(HandleDialogueComplete);
        OnDialogueComplete.AddListener(HandleDialogueComplete);
        PlayDialogue(dialogue);
    }

    public void TakeDamage(float amount)
    {
        if (isNickelDeadThisSession)
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
        if (isNickelDeadThisSession)
        {
            return;
        }

        isNickelDeadThisSession = true;
        isTeleporterUnlockedThisSession = true;
        currentHP = 0f;

        if (lockedTeleporterEmoteRoutine != null)
        {
            StopCoroutine(lockedTeleporterEmoteRoutine);
            lockedTeleporterEmoteRoutine = null;
        }

        if (lockedTeleporterEmoteRenderer != null)
        {
            lockedTeleporterEmoteRenderer.enabled = false;
        }

        foreach (Collider2D nickelCollider in GetComponents<Collider2D>())
        {
            nickelCollider.enabled = false;
        }

        SpriteRenderer nickelSprite = GetComponent<SpriteRenderer>();
        if (nickelSprite != null)
        {
            nickelSprite.enabled = false;
        }

        ModiGuard.AlertAllGuards();
        ShowDeathDialogue();
    }

    public void PlayRedPacketEscapeWarning(Action onComplete)
    {
        if (isDialogueActive || isNickelDeadThisSession)
        {
            onComplete?.Invoke();
            return;
        }

        isDialogueActive = true;
        isRedPacketEscapeWarningActive = true;
        onRedPacketEscapeWarningComplete = onComplete;
        CurrentDialogueNpc = this;

        if (nameText != null)
        {
            nameText.enableWordWrapping = false;
            nameText.overflowMode = TMPro.TextOverflowModes.Overflow;
            nameText.text = npcName;
            nameText.gameObject.SetActive(true);
        }

        if (npcPortraitImage != null)
        {
            npcPortraitImage.sprite = faceSprite;
            npcPortraitImage.gameObject.SetActive(true);
        }

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
            dialoguePanel.transform.SetAsLastSibling();
        }

        if (dialogueText != null)
        {
            dialogueText.text = "STOP HIM! HE HAS THE RED PACKET!";
        }

        PauseController.SetPause(true);
    }

    public void PlayLockedTeleporterEmote()
    {
        if (lockedTeleporterEmoteRenderer == null)
        {
            return;
        }

        if (lockedTeleporterEmoteRoutine != null)
        {
            StopCoroutine(lockedTeleporterEmoteRoutine);
        }

        lockedTeleporterEmoteRoutine = StartCoroutine(ShowLockedTeleporterEmote());
    }

    protected override void ChooseOption(int nextIndex)
    {
        pendingAnswerResponseLineIndex = nextIndex;

        if (nextIndex == CorrectAnswerLineIndex)
        {
            SetTeleporterUnlocked(true);
            Debug.Log("[NickelNouman] Correct answer clicked. Teleporter 1+ unlocked for this run.", this);
        }

        base.ChooseOption(nextIndex);
    }

    private void HandleDialogueComplete()
    {
        OnDialogueComplete.RemoveListener(HandleDialogueComplete);

        if (lastDialogueOutcome != CorrectAnswerOutcome)
        {
            Debug.Log($"[NickelNouman] Dialogue ended with '{lastDialogueOutcome}'. Teleporter remains locked = {!IsTeleporterUnlocked}", this);
            return;
        }

        SetTeleporterUnlocked(true);
        Debug.Log("[NickelNouman] Correct answer received. Teleporter 1+ unlocked permanently.", this);
    }

    private void SetTeleporterUnlocked(bool unlocked)
    {
        isTeleporterUnlockedThisSession = unlocked;
    }

    private void FinishRedPacketEscapeWarning()
    {
        StopAllCoroutines();
        ClearChoices();

        isRedPacketEscapeWarningActive = false;
        isDialogueActive = false;

        if (CurrentDialogueNpc == this)
        {
            CurrentDialogueNpc = null;
        }

        if (dialogueText != null)
        {
            dialogueText.text = "";
        }

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        PauseController.SetPause(false);
        ShowInteractionIcon(false);

        Action callback = onRedPacketEscapeWarningComplete;
        onRedPacketEscapeWarningComplete = null;
        callback?.Invoke();
    }

    private void ShowDeathDialogue()
    {
        StopAllCoroutines();
        ClearChoices();

        isDialogueActive = true;
        isDeathDialogueActive = true;
        isRedPacketEscapeWarningActive = false;
        onRedPacketEscapeWarningComplete = null;
        CurrentDialogueNpc = this;

        if (nameText != null)
        {
            nameText.gameObject.SetActive(false);
        }

        if (npcPortraitImage != null)
        {
            npcPortraitImage.gameObject.SetActive(false);
        }

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
            dialoguePanel.transform.SetAsLastSibling();
        }

        if (dialogueText != null)
        {
            dialogueText.text = "You killed the annoying prince. The guards attack!";
        }

        PauseController.SetPause(true);
        ShowInteractionIcon(false);
    }

    private void FinishDeathDialogue()
    {
        StopAllCoroutines();
        ClearChoices();

        isDeathDialogueActive = false;
        isDialogueActive = false;

        if (CurrentDialogueNpc == this)
        {
            CurrentDialogueNpc = null;
        }

        if (dialogueText != null)
        {
            dialogueText.text = "";
        }

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        if (nameText != null)
        {
            nameText.gameObject.SetActive(true);
        }

        if (npcPortraitImage != null)
        {
            npcPortraitImage.gameObject.SetActive(true);
        }

        PauseController.SetPause(false);
        gameObject.SetActive(false);
    }

    private void BuildLockedTeleporterEmote()
    {
        if (lockedTeleporterEmote == null)
        {
            return;
        }

        GameObject emoteObject = new GameObject("LockedTeleporterEmote");
        emoteObject.transform.SetParent(transform, false);
        emoteObject.transform.localPosition = lockedTeleporterEmoteOffset;

        lockedTeleporterEmoteRenderer = emoteObject.AddComponent<SpriteRenderer>();
        lockedTeleporterEmoteRenderer.sprite = lockedTeleporterEmote;

        SpriteRenderer npcSpriteRenderer = GetComponent<SpriteRenderer>();
        if (npcSpriteRenderer != null)
        {
            lockedTeleporterEmoteRenderer.sortingLayerID = npcSpriteRenderer.sortingLayerID;
            lockedTeleporterEmoteRenderer.sortingOrder = npcSpriteRenderer.sortingOrder + 1;
        }

        lockedTeleporterEmoteRenderer.enabled = false;
    }

    private IEnumerator ShowLockedTeleporterEmote()
    {
        lockedTeleporterEmoteRenderer.enabled = true;
        yield return new WaitForSecondsRealtime(lockedTeleporterEmoteDuration);
        lockedTeleporterEmoteRenderer.enabled = false;
        lockedTeleporterEmoteRoutine = null;
    }
}

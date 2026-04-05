using System.Collections;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoamingMerchantNPC : NPCBase, IDamageable
{
    private const float MaxHP = 1f;
    private const string EmoteSpriteName = "emote22_0";
    private const int ChoiceGamble = 1001;
    private const int ChoiceIgnore = 1002;
    private const int ChoiceGambleAgain = 2001;
    private const int ChoiceLeave = 2002;
    private static readonly string[] DefaultDialogueOptions =
    {
        "If you win, it's luck... but if I win, it's skill!!",
        "Step right up! Statistically, one of us wins!",
        "Trust me, I only cheat a little.",
        "Win some money, lose some- I mean win guaranteed!",
        "Let's make poor decisions together.",
        "Roll high and we're friends! Roll low and... we were!"
    };

    [Header("Merchant Emote")]
    [SerializeField] private Sprite approachEmote;
    [SerializeField] private Vector3 approachEmoteOffset = new Vector3(0f, 1.25f, 0f);

    [Header("Merchant Movement")]
    [SerializeField] private float moveSpeed = 1.1f;
    [SerializeField] private float wanderRadius = 2f;
    [SerializeField] private Vector2 pauseBetweenMoves = new Vector2(0.6f, 1.4f);

    [Header("Merchant Animation")]
    [SerializeField] private bool animateWithSpritesheet;
    [SerializeField] private float walkAnimationFps = 8f;

    [Header("Merchant Loot")]
    [SerializeField] private GameObject droppedGoldPrefab;
    [SerializeField] private int goldDropAmount = 25;
    [SerializeField] private float deathFlashDuration = 0.15f;
    [SerializeField] private Color deathFlashColor = Color.red;

    private readonly NPCDialogue[] runtimeDialogues = new NPCDialogue[DefaultDialogueOptions.Length];

    private SpriteRenderer merchantSprite;
    private SpriteRenderer approachEmoteRenderer;
    private Rigidbody2D merchantBody;
    private BoxCollider2D merchantCollider;
    private Vector3 spawnPosition;
    private Vector3 currentDestination;
    private float pauseTimer;
    private float currentHP = MaxHP;
    private bool isDead;
    private bool isDeathDialogueActive;
    private Sprite[] directionalWalkSprites;
    private int currentAnimationDirection;
    private float animationTimer;
    private int currentBet = 50;
    private MerchantDialogueMode dialogueMode;
    private GreedMeter playerGreed;
    private int gamblingSessionStartingGold = -1;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureMerchantSceneSetup()
    {
        SpriteRenderer[] merchantSprites = Object.FindObjectsOfType<SpriteRenderer>();
        foreach (SpriteRenderer spriteRenderer in merchantSprites)
        {
            if (spriteRenderer == null)
            {
                continue;
            }

            string objectName = spriteRenderer.gameObject.name;
            if (!objectName.StartsWith("MerchantF") && !objectName.StartsWith("MerchantM"))
            {
                continue;
            }

            if (spriteRenderer.GetComponent<RoamingMerchantNPC>() == null)
            {
                spriteRenderer.gameObject.AddComponent<RoamingMerchantNPC>();
            }
        }
    }

    private void Awake()
    {
        merchantSprite = GetComponent<SpriteRenderer>();
        merchantBody = GetComponent<Rigidbody2D>();
        merchantCollider = GetComponent<BoxCollider2D>();

        npcName = "Merchant";

        if (gameObject.name.StartsWith("MerchantM"))
        {
            animateWithSpritesheet = true;
        }

        if (faceSprite == null && merchantSprite != null)
        {
            faceSprite = merchantSprite.sprite;
        }

        ResolveRuntimeDependencies();
        EnsurePhysicsSetup();
        EnsureDialogueReferencesFromScene();
        BuildApproachEmote();
        BuildRuntimeDialogues();
        CacheDirectionalWalkSprites();
        spawnPosition = transform.position;
        currentDestination = spawnPosition;
    }

    protected override void Start()
    {
        base.Start();
        EnsureDialogueReferencesFromScene();
    }

    private void FixedUpdate()
    {
        if (PauseController.IsGamePaused || isDialogueActive || isDead || merchantBody == null)
        {
            UpdateAnimation(Vector2.zero);
            return;
        }
        UpdateAnimation(Vector2.zero);
    }

    public override void Interact(GameObject player)
    {
        EnsureDialogueReferencesFromScene();
        if (playerGreed == null && player != null)
        {
            playerGreed = player.GetComponent<GreedMeter>();
        }

        if (isDeathDialogueActive)
        {
            FinishDeathDialogue();
            return;
        }

        PlayDialogue(BuildChoiceDialogue(
            DefaultDialogueOptions[Random.Range(0, runtimeDialogues.Length)],
            new[] { "Let's gamble.", "Ignore" },
            new[] { ChoiceGamble, ChoiceIgnore },
            MerchantDialogueMode.IntroChoice));
    }

    public override void ShowInteractionIcon(bool show)
    {
        if (approachEmoteRenderer == null)
        {
            ResolveRuntimeDependencies();
            BuildApproachEmote();
        }

        if (approachEmoteRenderer != null)
        {
            approachEmoteRenderer.enabled = show && !isDead;
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
        pauseTimer = 0f;

        if (merchantBody != null)
        {
            merchantBody.linearVelocity = Vector2.zero;
        }

        if (merchantCollider != null)
        {
            merchantCollider.enabled = false;
        }

        ShowInteractionIcon(false);
        DropGold();
        StartCoroutine(PlayDeathSequence());
    }

    public bool IsDead() => isDead;

    private void EnsurePhysicsSetup()
    {
        if (merchantBody == null)
        {
            merchantBody = gameObject.AddComponent<Rigidbody2D>();
        }

        merchantBody.bodyType = RigidbodyType2D.Kinematic;
        merchantBody.gravityScale = 0f;
        merchantBody.constraints = RigidbodyConstraints2D.FreezeRotation;

        if (merchantCollider == null)
        {
            merchantCollider = gameObject.AddComponent<BoxCollider2D>();
        }

        if (merchantSprite != null && merchantSprite.sprite != null)
        {
            Vector2 spriteSize = merchantSprite.sprite.bounds.size;
            merchantCollider.size = new Vector2(Mathf.Max(0.45f, spriteSize.x * 0.85f), Mathf.Max(0.7f, spriteSize.y * 0.9f));
            merchantCollider.offset = new Vector2(0f, (merchantCollider.size.y - spriteSize.y) * 0.1f);
        }
    }

    private void ResolveRuntimeDependencies()
    {
        if (approachEmote == null)
        {
            Sprite[] loadedSprites = Resources.FindObjectsOfTypeAll<Sprite>();
            foreach (Sprite loadedSprite in loadedSprites)
            {
                if (loadedSprite != null && loadedSprite.name == EmoteSpriteName)
                {
                    approachEmote = loadedSprite;
                    break;
                }
            }

            if (approachEmote == null)
            {
                Texture2D[] loadedTextures = Resources.FindObjectsOfTypeAll<Texture2D>();
                foreach (Texture2D loadedTexture in loadedTextures)
                {
                    if (loadedTexture == null || loadedTexture.name != "emote22")
                    {
                        continue;
                    }

                    approachEmote = Sprite.Create(
                        loadedTexture,
                        new Rect(0f, 0f, loadedTexture.width, loadedTexture.height),
                        new Vector2(0.5f, 0.5f),
                        16f);
                    approachEmote.name = EmoteSpriteName;
                    break;
                }
            }
        }

        if (droppedGoldPrefab == null)
        {
            PlayerCombat playerCombat = Object.FindObjectOfType<PlayerCombat>();
            if (playerCombat != null)
            {
                FieldInfo droppedGoldField = typeof(PlayerCombat).GetField("droppedGoldPrefab", BindingFlags.NonPublic | BindingFlags.Instance);
                if (droppedGoldField != null)
                {
                    droppedGoldPrefab = droppedGoldField.GetValue(playerCombat) as GameObject;
                }
            }
        }
    }

    private void BuildApproachEmote()
    {
        if (approachEmote == null || approachEmoteRenderer != null)
        {
            return;
        }

        GameObject emoteObject = new GameObject("MerchantApproachEmote");
        emoteObject.transform.SetParent(transform, false);
        emoteObject.transform.localPosition = approachEmoteOffset;

        approachEmoteRenderer = emoteObject.AddComponent<SpriteRenderer>();
        approachEmoteRenderer.sprite = approachEmote;

        if (merchantSprite != null)
        {
            approachEmoteRenderer.sortingLayerID = merchantSprite.sortingLayerID;
            approachEmoteRenderer.sortingOrder = merchantSprite.sortingOrder + 1;
        }

        approachEmoteRenderer.enabled = false;
    }

    private void BuildRuntimeDialogues()
    {
        for (int i = 0; i < DefaultDialogueOptions.Length; i++)
        {
            if (runtimeDialogues[i] != null)
            {
                continue;
            }

            runtimeDialogues[i] = ScriptableObject.CreateInstance<NPCDialogue>();
            runtimeDialogues[i].npcName = npcName;
            runtimeDialogues[i].npcSprite = faceSprite;
            runtimeDialogues[i].dialogue = new[] { DefaultDialogueOptions[i] };
            runtimeDialogues[i].typingSpeed = 0.03f;
            runtimeDialogues[i].endDialogueOutcomes = new[] { "merchant_line_complete" };
        }
    }

    protected override void ChooseOption(int nextIndex)
    {
        switch (nextIndex)
        {
            case ChoiceGamble:
                ClearChoices();
                StartGambleRound();
                return;
            case ChoiceIgnore:
                ClearChoices();
                dialogueMode = MerchantDialogueMode.None;
                EndDialogue();
                return;
            case ChoiceGambleAgain:
                ClearChoices();
                StartGambleRound();
                return;
            case ChoiceLeave:
                ClearChoices();
                ShowFinalGoldDialogue();
                return;
        }

        switch (dialogueMode)
        {
            case MerchantDialogueMode.IntroChoice:
                HandleIntroChoice(nextIndex);
                return;
            case MerchantDialogueMode.PostRoundChoice:
                HandlePostRoundChoice(nextIndex);
                return;
        }

        base.ChooseOption(nextIndex);
    }

    private void HandleIntroChoice(int nextIndex)
    {
        ClearChoices();

        if (nextIndex == ChoiceIgnore)
        {
            dialogueMode = MerchantDialogueMode.None;
            EndDialogue();
            return;
        }

        if (nextIndex == ChoiceGamble)
        {
            StartGambleRound();
            return;
        }
    }

    private void HandlePostRoundChoice(int nextIndex)
    {
        ClearChoices();

        switch (nextIndex)
        {
            case ChoiceGambleAgain:
                StartGambleRound();
                return;
            case ChoiceLeave:
                ShowFinalGoldDialogue();
                return;
        }
    }

    private void StartGambleRound()
    {
        dialogueMode = MerchantDialogueMode.None;

        if (playerGreed == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerGreed = player.GetComponent<GreedMeter>();
            }
        }

        if (playerGreed == null)
        {
            ShowSimpleDialogue("No wallet, no wager.");
            return;
        }

        if (gamblingSessionStartingGold < 0)
        {
            gamblingSessionStartingGold = playerGreed.GetCurrentGold();
        }

        if (playerGreed.GetCurrentGold() <= 0 || playerGreed.GetCurrentGold() < currentBet)
        {
            ShowSimpleDialogue("Get away from here, peon.");
            return;
        }

        playerGreed.RemoveGold(currentBet);

        int playerDie1 = Random.Range(1, 7);
        int playerDie2 = Random.Range(1, 7);
        int houseDie1 = Random.Range(1, 7);
        int houseDie2 = Random.Range(1, 7);

        int playerTotal = playerDie1 + playerDie2;
        int houseTotal = houseDie1 + houseDie2;

        int payout = 0;
        int netGoldChange = -currentBet;
        string outcomeLine;

        if (houseDie1 == 6 && houseDie2 == 6)
        {
            playerGreed.RemoveGold(currentBet);
            netGoldChange -= currentBet;
            outcomeLine = $"House rolled boxcars. Brutal. You lose {currentBet * 2} gold.";
        }
        else if (playerDie1 == 1 && playerDie2 == 1)
        {
            playerGreed.RemoveGold(currentBet);
            netGoldChange -= currentBet;
            outcomeLine = $"Snake eyes. That's double down in the wrong direction. You lose {currentBet * 2} gold.";
        }
        else if (playerDie1 == 6 && playerDie2 == 6)
        {
            payout = currentBet * 3;
            playerGreed.AddGold(payout);
            netGoldChange += payout;
            outcomeLine = $"Boxcars. Beautiful. You win {currentBet * 2} gold.";
        }
        else if (playerTotal > houseTotal)
        {
            payout = currentBet * 2;
            playerGreed.AddGold(payout);
            netGoldChange += payout;
            outcomeLine = $"You beat the house. You win {currentBet} gold.";
        }
        else if (playerTotal < houseTotal)
        {
            outcomeLine = $"House takes it. You lose {currentBet} gold.";
        }
        else
        {
            payout = currentBet;
            playerGreed.AddGold(payout);
            netGoldChange += payout;
            outcomeLine = "Push. Nobody wins. Nobody loses.";
        }

        if (playerGreed.GetCurrentGold() <= 0)
        {
            ShowSimpleDialogueSequence(new[]
            {
                $"Ante up. {currentBet} gold on the felt.",
                $"You rolled: {playerDie1} + {playerDie2} = {playerTotal}",
                $"House rolled: {houseDie1} + {houseDie2} = {houseTotal}",
                $"{outcomeLine}\nGold total: 0\nYou're broke."
            });
            return;
        }

        ShowPostRoundDialogue(
            $"Ante up. {currentBet} gold on the felt.",
            $"You rolled: {playerDie1} + {playerDie2} = {playerTotal}",
            $"House rolled: {houseDie1} + {houseDie2} = {houseTotal}",
            $"{outcomeLine}\nGold total: {playerGreed.GetCurrentGold()}");
    }

    private void ShowPostRoundDialogue(string anteLine, string playerRollLine, string houseRollLine, string outcomeLine)
    {
        OpenMerchantDialogue(BuildMultiLineChoiceDialogue(
            new[]
            {
                anteLine,
                playerRollLine,
                houseRollLine,
                outcomeLine
            },
            new[] { "Gamble again", "Leave" },
            new[] { ChoiceGambleAgain, ChoiceLeave },
            MerchantDialogueMode.PostRoundChoice));
    }

    private void ShowFinalGoldDialogue()
    {
        int finalGold = playerGreed != null ? playerGreed.GetCurrentGold() : 0;
        dialogueMode = MerchantDialogueMode.None;
        int netSessionGold = gamblingSessionStartingGold >= 0 ? finalGold - gamblingSessionStartingGold : 0;
        string merchantReaction = netSessionGold > 0
            ? "*The degenerate merchant realized you made money off of them and curses you under their breath.*"
            : netSessionGold < 0
                ? "*The merchant grins at you greedily, collecting their winnings.*"
                : "*The merchant eyes the table, annoyed that nobody came out ahead.*";

        OpenMerchantDialogue(BuildSimpleDialogue($"{merchantReaction}\nFinal gold: {finalGold}"));
        gamblingSessionStartingGold = -1;
    }

    private void ShowSimpleDialogue(string message)
    {
        dialogueMode = MerchantDialogueMode.None;
        OpenMerchantDialogue(BuildSimpleDialogue(message));
    }

    private void ShowSimpleDialogueSequence(string[] lines)
    {
        dialogueMode = MerchantDialogueMode.None;
        OpenMerchantDialogue(BuildMultiLineDialogue(lines));
    }

    private NPCDialogue BuildChoiceDialogue(string message, string[] options, int[] nextIndexes, MerchantDialogueMode mode)
    {
        NPCDialogue dialogue = ScriptableObject.CreateInstance<NPCDialogue>();
        dialogue.npcName = npcName;
        dialogue.npcSprite = faceSprite;
        dialogue.dialogue = new[] { message };
        dialogue.typingSpeed = 0.02f;
        dialogue.autoProgressLines = new[] { true };
        dialogue.autoProgressDelay = 0.05f;
        dialogue.choices = new[]
        {
            new DialogueChoice
            {
                dialogueIndex = 0,
                choices = options,
                nextDialogueIndexes = nextIndexes
            }
        };

        dialogueMode = mode;
        return dialogue;
    }

    private NPCDialogue BuildSimpleDialogue(string message)
    {
        NPCDialogue dialogue = ScriptableObject.CreateInstance<NPCDialogue>();
        dialogue.npcName = npcName;
        dialogue.npcSprite = faceSprite;
        dialogue.dialogue = new[] { message };
        dialogue.typingSpeed = 0.02f;
        return dialogue;
    }

    private NPCDialogue BuildMultiLineDialogue(string[] lines)
    {
        NPCDialogue dialogue = ScriptableObject.CreateInstance<NPCDialogue>();
        dialogue.npcName = npcName;
        dialogue.npcSprite = faceSprite;
        dialogue.dialogue = lines;
        dialogue.typingSpeed = 0.02f;
        return dialogue;
    }

    private NPCDialogue BuildMultiLineChoiceDialogue(string[] lines, string[] options, int[] nextIndexes, MerchantDialogueMode mode)
    {
        NPCDialogue dialogue = BuildMultiLineDialogue(lines);
        dialogue.choices = new[]
        {
            new DialogueChoice
            {
                dialogueIndex = lines.Length - 1,
                choices = options,
                nextDialogueIndexes = nextIndexes
            }
        };

        dialogueMode = mode;
        return dialogue;
    }

    private void OpenMerchantDialogue(NPCDialogue dialogue)
    {
        if (isDialogueActive)
        {
            EndDialogue();
        }

        PlayDialogue(dialogue);
    }

    private void CacheDirectionalWalkSprites()
    {
        if (!animateWithSpritesheet || merchantSprite == null || merchantSprite.sprite == null || merchantSprite.sprite.texture == null)
        {
            return;
        }

        Sprite[] candidateSprites = Resources.FindObjectsOfTypeAll<Sprite>();
        Sprite[] matchingTextureSprites = System.Array.FindAll(candidateSprites, sprite =>
            sprite != null
            && sprite.texture == merchantSprite.sprite.texture
            && sprite.rect.width > 0f
            && sprite.rect.height > 0f);

        System.Array.Sort(matchingTextureSprites, (left, right) =>
        {
            int topToBottom = right.rect.y.CompareTo(left.rect.y);
            return topToBottom != 0 ? topToBottom : left.rect.x.CompareTo(right.rect.x);
        });

        if (matchingTextureSprites.Length < 24)
        {
            return;
        }

        directionalWalkSprites = matchingTextureSprites;
        currentAnimationDirection = 0;
        merchantSprite.sprite = directionalWalkSprites[0];
        merchantSprite.flipX = false;
    }

    private void UpdateAnimation(Vector2 movement)
    {
        if (!animateWithSpritesheet || merchantSprite == null || directionalWalkSprites == null || directionalWalkSprites.Length < 24)
        {
            return;
        }

        if (movement.sqrMagnitude < 0.0001f)
        {
            animationTimer = 0f;
            merchantSprite.sprite = directionalWalkSprites[currentAnimationDirection * 6];
            return;
        }

        currentAnimationDirection = GetAnimationDirection(movement);
        animationTimer += Time.fixedDeltaTime * walkAnimationFps;

        int frame = Mathf.FloorToInt(animationTimer) % 6;
        merchantSprite.sprite = directionalWalkSprites[currentAnimationDirection * 6 + frame];
    }

    private int GetAnimationDirection(Vector2 movement)
    {
        if (Mathf.Abs(movement.x) > Mathf.Abs(movement.y))
        {
            return movement.x > 0f ? 2 : 3;
        }

        return movement.y > 0f ? 1 : 0;
    }

    private void ChooseNextDestination()
    {
        Vector2[] directions =
        {
            Vector2.left,
            Vector2.right,
            Vector2.up,
            Vector2.down
        };

        Vector2 direction = directions[Random.Range(0, directions.Length)];
        float distance = Random.Range(Mathf.Max(0.5f, wanderRadius * 0.5f), wanderRadius);
        Vector3 candidate = spawnPosition + (Vector3)(direction * distance);

        candidate.x = Mathf.Clamp(candidate.x, spawnPosition.x - wanderRadius, spawnPosition.x + wanderRadius);
        candidate.y = Mathf.Clamp(candidate.y, spawnPosition.y - wanderRadius, spawnPosition.y + wanderRadius);
        candidate.z = spawnPosition.z;

        currentDestination = candidate;
    }

    private void DropGold()
    {
        if (droppedGoldPrefab == null || goldDropAmount <= 0)
        {
            return;
        }

        GameObject drop = Instantiate(droppedGoldPrefab, transform.position, Quaternion.identity);
        DroppedGold droppedGold = drop.GetComponent<DroppedGold>();
        if (droppedGold != null)
        {
            droppedGold.SetGoldAmount(goldDropAmount);
        }
    }

    private IEnumerator PlayDeathSequence()
    {
        Color originalColor = merchantSprite != null ? merchantSprite.color : Color.white;

        if (merchantSprite != null)
        {
            merchantSprite.color = deathFlashColor;
        }

        yield return new WaitForSecondsRealtime(deathFlashDuration);

        if (merchantSprite != null)
        {
            merchantSprite.color = originalColor;
            merchantSprite.enabled = false;
        }

        if (approachEmoteRenderer != null)
        {
            approachEmoteRenderer.enabled = false;
        }

        ShowDeathDialogue();
    }

    private void ShowDeathDialogue()
    {
        StopAllCoroutines();
        ClearChoices();

        EnsureDialogueReferencesFromScene();
        isDialogueActive = true;
        isDeathDialogueActive = true;
        CurrentDialogueNpc = this;

        if (nameText != null)
        {
            nameText.text = string.Empty;
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
            dialogueText.text = "You killed and robbed the merchant! Shame on you.";
        }

        PauseController.SetPause(true);
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
            dialogueText.text = string.Empty;
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
}

public enum MerchantDialogueMode
{
    None = 0,
    IntroChoice = 1,
    PostRoundChoice = 2
}

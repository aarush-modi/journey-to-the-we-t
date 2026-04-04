using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PickpocketThief : MonoBehaviour, IDamageable
{
    private const float MaxHP = 1f;
    private static readonly int RunDownStateHash = Animator.StringToHash("PickpocketRunDown");
    private static readonly int RunUpStateHash = Animator.StringToHash("PickpocketRunUp");
    private static readonly int RunLeftStateHash = Animator.StringToHash("PickpocketRunLeft");
    private static readonly int RunRightStateHash = Animator.StringToHash("PickpocketRunRight");

    private enum PickpocketState
    {
        ChasingPlayer,
        FleeingUpward,
        FleeingPlayer,
        Cornered
    }

    private enum PostCatchDialogueStep
    {
        None,
        OpeningChoice,
        ThreatenResponse,
        WhoSentChoice,
        RevealSender,
        TeachSprint,
        SprintMiracle,
        PostSprintReminder
    }

    [SerializeField] private float chaseSpeed = 2.5f;
    [SerializeField] private float fleeSpeed = 3.5f;
    [SerializeField] private float fleeUpwardDistance = 7f;
    [SerializeField] private float obstacleProbeDistance = 0.9f;
    [SerializeField] private float obstacleProbeRadius = 0.2f;
    [SerializeField] private float touchDamage = 1f;
    [SerializeField] private float talkDistance = 1.5f;
    [SerializeField] private float sprintLessonMultiplier = 2f;
    [SerializeField] private int recoveredGreedAmount = 600;
    [SerializeField] private Sprite faceSprite;
    [SerializeField] private Sprite interactionIconSprite;
    [SerializeField] private Vector3 interactionIconOffset = new Vector3(0f, 1.25f, 0f);
    [SerializeField] private GameObject choiceButtonPrefab;
    [SerializeField] private string corneredDialogue = "You corner the pickpocket and take back your money.";
    [SerializeField] private float flashDuration = 0.1f;
    [SerializeField] private Color flashColor = Color.red;

    private Rigidbody2D thiefBody;
    private Animator thiefAnimator;
    private SpriteRenderer thiefSprite;
    private SpriteRenderer interactionIconRenderer;
    private Transform playerTarget;
    private PlayerController playerController;
    private PlayerCombat playerCombat;
    private GreedMeter playerGreedMeter;
    private PickpocketState pickpocketState = PickpocketState.ChasingPlayer;
    private PostCatchDialogueStep postCatchDialogueStep = PostCatchDialogueStep.None;
    private bool waitingForPlayerSeparation;
    private bool isCorneredDialogueOpen;
    private bool isPostCatchDialogueOpen;
    private bool isDeathDialogueOpen;
    private bool hasTaughtSprint;
    private bool isDead;
    private float currentHP = MaxHP;
    private float fleeUpwardTargetY;
    private int currentMoveStateHash;
    private Coroutine hurtFlashRoutine;

    private GameObject dialoguePanel;
    private TMP_Text dialogueText;
    private TMP_Text nameText;
    private Image npcPortraitImage;
    private Transform choiceContainer;
    private readonly Vector2[] avoidanceProbeDirections = new Vector2[7];

    private void Awake()
    {
        thiefBody = GetComponent<Rigidbody2D>();
        if (thiefBody == null)
        {
            thiefBody = gameObject.AddComponent<Rigidbody2D>();
        }

        thiefBody.bodyType = RigidbodyType2D.Dynamic;
        thiefBody.gravityScale = 0f;
        thiefBody.freezeRotation = true;
        thiefBody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        thiefBody.interpolation = RigidbodyInterpolation2D.Interpolate;

        thiefAnimator = GetComponent<Animator>();
        thiefSprite = GetComponent<SpriteRenderer>();
        BuildInteractionIcon();
    }

    private void Start()
    {
        FindPlayerReferences();
        BindDialogueUi();
        playerController?.SetForcedForwardMovement(true);
    }

    private void Update()
    {
        if (playerTarget == null && pickpocketState != PickpocketState.Cornered)
        {
            FindPlayerReferences();
        }

        if (isDeathDialogueOpen)
        {
            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                CloseDeathDialogue();
            }

            return;
        }

        if (isCorneredDialogueOpen)
        {
            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                CloseCorneredDialogue();
            }

            return;
        }

        if (isPostCatchDialogueOpen)
        {
            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                AdvancePostCatchDialogue();
            }

            return;
        }

        if (pickpocketState != PickpocketState.Cornered || playerTarget == null)
        {
            ShowInteractionIcon(false);
            return;
        }

        bool playerInTalkRange = Vector2.Distance(playerTarget.position, transform.position) <= talkDistance;
        ShowInteractionIcon(playerInTalkRange);

        if (playerInTalkRange && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            StartPostCatchDialogue();
        }
    }

    public void TakeDamage(float amount)
    {
        if (isDead)
        {
            return;
        }

        if (!hasTaughtSprint)
        {
            Debug.Log("[PickpocketThief] Hit ignored because sprint lesson has not been taught yet.", this);
            return;
        }

        Debug.Log($"[PickpocketThief] TakeDamage received. HP before={currentHP}, damage={amount}", this);

        if (thiefSprite != null)
        {
            if (hurtFlashRoutine != null)
            {
                StopCoroutine(hurtFlashRoutine);
            }

            hurtFlashRoutine = StartCoroutine(HurtFlash());
        }

        currentHP -= amount;
        if (currentHP <= 0f)
        {
            Die();
        }
    }

    public void Die()
    {
        if (isDead || !hasTaughtSprint)
        {
            return;
        }

        isDead = true;
        currentHP = 0f;
        isCorneredDialogueOpen = false;
        isPostCatchDialogueOpen = false;
        postCatchDialogueStep = PostCatchDialogueStep.None;
        ShowInteractionIcon(false);
        ClearChoices();

        foreach (Collider2D thiefCollider in GetComponents<Collider2D>())
        {
            thiefCollider.enabled = false;
        }

        if (thiefSprite != null)
        {
            thiefSprite.enabled = false;
        }

        thiefBody.linearVelocity = Vector2.zero;
        thiefBody.bodyType = RigidbodyType2D.Static;

        if (thiefAnimator != null)
        {
            thiefAnimator.enabled = false;
        }

        ShowDeathDialogue();
    }

    private void FixedUpdate()
    {
        if (pickpocketState == PickpocketState.Cornered || playerTarget == null)
        {
            thiefBody.linearVelocity = Vector2.zero;
            return;
        }

        if (pickpocketState == PickpocketState.FleeingUpward)
        {
            if (transform.position.y >= fleeUpwardTargetY)
            {
                pickpocketState = PickpocketState.FleeingPlayer;
            }
            else
            {
                Vector2 upwardVelocity = GetObstacleAwareVelocity(Vector2.up, fleeSpeed);
                thiefBody.linearVelocity = upwardVelocity;
                PlayMoveAnimation(upwardVelocity);
                return;
            }
        }

        Vector2 toPlayer = (Vector2)playerTarget.position - thiefBody.position;
        if (toPlayer.sqrMagnitude < 0.0001f)
        {
            thiefBody.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 moveDirection = toPlayer.normalized;
        Vector2 velocity = pickpocketState == PickpocketState.ChasingPlayer
            ? GetObstacleAwareVelocity(moveDirection, chaseSpeed)
            : GetObstacleAwareVelocity(-moveDirection, fleeSpeed);

        thiefBody.linearVelocity = velocity;
        PlayMoveAnimation(velocity);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryHandlePlayerTouch(collision.gameObject);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        TryHandlePlayerTouch(collision.gameObject);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            waitingForPlayerSeparation = false;
        }
    }

    private void TryHandlePlayerTouch(GameObject other)
    {
        if (pickpocketState == PickpocketState.Cornered || !other.CompareTag("Player"))
        {
            return;
        }

        CachePlayerComponents(other);

        if (pickpocketState == PickpocketState.ChasingPlayer)
        {
            playerController?.SetForcedForwardMovement(false);
            playerCombat?.TakeDamage(touchDamage);
            RemoveAllGreed();
            fleeUpwardTargetY = transform.position.y + fleeUpwardDistance;
            pickpocketState = PickpocketState.FleeingUpward;
            waitingForPlayerSeparation = true;
            return;
        }

        if (waitingForPlayerSeparation)
        {
            return;
        }

        RestoreGreedTo(recoveredGreedAmount);
        ShowCorneredDialogue();
        FreezeInPlace();
        pickpocketState = PickpocketState.Cornered;
    }

    private void FindPlayerReferences()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject == null)
        {
            return;
        }

        playerTarget = playerObject.transform;
        CachePlayerComponents(playerObject);
    }

    private void CachePlayerComponents(GameObject playerObject)
    {
        if (playerObject == null)
        {
            return;
        }

        if (playerController == null)
        {
            playerController = playerObject.GetComponent<PlayerController>();
        }

        if (playerCombat == null)
        {
            playerCombat = playerObject.GetComponent<PlayerCombat>();
        }

        if (playerGreedMeter == null)
        {
            playerGreedMeter = playerObject.GetComponent<GreedMeter>();
        }
    }

    private void RemoveAllGreed()
    {
        if (playerGreedMeter == null)
        {
            return;
        }

        playerGreedMeter.RemoveGold(playerGreedMeter.GetCurrentGold());
    }

    private void RestoreGreedTo(int targetGreed)
    {
        if (playerGreedMeter == null)
        {
            return;
        }

        int currentGreed = playerGreedMeter.GetCurrentGold();
        if (currentGreed < targetGreed)
        {
            playerGreedMeter.AddGold(targetGreed - currentGreed);
        }
        else if (currentGreed > targetGreed)
        {
            playerGreedMeter.RemoveGold(currentGreed - targetGreed);
        }
    }

    private void BindDialogueUi()
    {
        foreach (Transform sceneTransform in Resources.FindObjectsOfTypeAll<Transform>())
        {
            if (sceneTransform == null)
            {
                continue;
            }

            switch (sceneTransform.name)
            {
                case "DialoguePanel":
                    dialoguePanel = sceneTransform.gameObject;
                    break;
                case "DialogueText":
                    dialogueText = sceneTransform.GetComponent<TMP_Text>();
                    break;
                case "NPCNameText":
                    nameText = sceneTransform.GetComponent<TMP_Text>();
                    break;
                case "DialoguePortrait":
                    npcPortraitImage = sceneTransform.GetComponent<Image>();
                    break;
                case "ChoiceContainer":
                    choiceContainer = sceneTransform;
                    break;
            }
        }
    }

    private void ShowCorneredDialogue()
    {
        BindDialogueUi();
        isCorneredDialogueOpen = true;
        ShowNarrationLine(corneredDialogue);
        PauseController.SetPause(true);
    }

    private void CloseCorneredDialogue()
    {
        isCorneredDialogueOpen = false;
        HideDialoguePanel();
        PauseController.SetPause(false);
    }

    private void StartPostCatchDialogue()
    {
        BindDialogueUi();
        isPostCatchDialogueOpen = true;
        ShowInteractionIcon(false);

        if (hasTaughtSprint)
        {
            postCatchDialogueStep = PostCatchDialogueStep.PostSprintReminder;
            ShowSpeakerLine("What do you want? I already told you everything I know!");
            PauseController.SetPause(true);
            return;
        }

        postCatchDialogueStep = PostCatchDialogueStep.OpeningChoice;
        ShowSpeakerLine("Aw man, you caught me! Please don't hurt me!");
        ShowChoiceButtons(("Threaten him", ChooseThreaten), ("Ignore him", ChooseIgnore));
        PauseController.SetPause(true);
    }

    private void AdvancePostCatchDialogue()
    {
        switch (postCatchDialogueStep)
        {
            case PostCatchDialogueStep.ThreatenResponse:
                postCatchDialogueStep = PostCatchDialogueStep.WhoSentChoice;
                ShowSpeakerLine("Wait wait wait!");
                ShowChoiceButtons(("Who sent you?", ChooseWhoSentYou));
                break;
            case PostCatchDialogueStep.RevealSender:
                postCatchDialogueStep = PostCatchDialogueStep.TeachSprint;
                ShowSpeakerLine("You wanna run as fast as me? Let me teach you! Just don't hurt me!");
                break;
            case PostCatchDialogueStep.TeachSprint:
                postCatchDialogueStep = PostCatchDialogueStep.SprintMiracle;
                TeachPlayerSprint();
                ShowNarrationLine("*You learned to sprint.*\n*What a miracle. Did nobody teach you this...?*\n*Hold SHIFT to sprint.*");
                break;
            case PostCatchDialogueStep.SprintMiracle:
            case PostCatchDialogueStep.PostSprintReminder:
                EndPostCatchDialogue();
                break;
        }
    }

    private void ChooseThreaten()
    {
        ClearChoices();
        postCatchDialogueStep = PostCatchDialogueStep.ThreatenResponse;
        ShowSpeakerLine("Wait wait wait!");
    }

    private void ChooseIgnore()
    {
        EndPostCatchDialogue();
    }

    private void ChooseWhoSentYou()
    {
        ClearChoices();
        postCatchDialogueStep = PostCatchDialogueStep.RevealSender;
        ShowSpeakerLine("I don't know his real name! He calls himself the Greedy Grummer!");
    }

    private void EndPostCatchDialogue()
    {
        isPostCatchDialogueOpen = false;
        postCatchDialogueStep = PostCatchDialogueStep.None;
        HideDialoguePanel();
        PauseController.SetPause(false);
    }

    private void ShowDeathDialogue()
    {
        BindDialogueUi();
        isDeathDialogueOpen = true;
        ShowNarrationLine("You kill the pickpocket for absolutely no reason.");
        PauseController.SetPause(true);
    }

    private void CloseDeathDialogue()
    {
        isDeathDialogueOpen = false;
        HideDialoguePanel();
        PauseController.SetPause(false);
        gameObject.SetActive(false);
    }

    private void ShowSpeakerLine(string line)
    {
        EnsureDialoguePanelActive();
        ClearChoices();

        if (nameText != null)
        {
            nameText.text = "Pickpocket";
            nameText.gameObject.SetActive(true);
        }

        if (npcPortraitImage != null)
        {
            npcPortraitImage.sprite = faceSprite;
            npcPortraitImage.gameObject.SetActive(faceSprite != null);
        }

        if (dialogueText != null)
        {
            dialogueText.text = line;
        }
    }

    private void ShowNarrationLine(string line)
    {
        EnsureDialoguePanelActive();
        ClearChoices();

        if (nameText != null)
        {
            nameText.gameObject.SetActive(false);
        }

        if (npcPortraitImage != null)
        {
            npcPortraitImage.gameObject.SetActive(false);
        }

        if (dialogueText != null)
        {
            dialogueText.text = line;
        }
        else
        {
            Debug.LogWarning("[PickpocketThief] Could not find DialogueText for dialogue.", this);
        }
    }

    private void EnsureDialoguePanelActive()
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
            dialoguePanel.transform.SetAsLastSibling();
        }
    }

    private void HideDialoguePanel()
    {
        ClearChoices();

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
    }

    private void ShowChoiceButtons(params (string label, UnityEngine.Events.UnityAction onClick)[] choices)
    {
        ClearChoices();

        if (choiceContainer == null || choiceButtonPrefab == null)
        {
            return;
        }

        Button firstButton = null;
        foreach ((string label, UnityEngine.Events.UnityAction onClick) in choices)
        {
            GameObject choiceObject = Instantiate(choiceButtonPrefab, choiceContainer);
            TMP_Text choiceText = choiceObject.GetComponentInChildren<TMP_Text>();
            if (choiceText != null)
            {
                choiceText.text = label;
            }

            Button choiceButton = choiceObject.GetComponent<Button>();
            if (choiceButton != null)
            {
                choiceButton.onClick.RemoveAllListeners();
                choiceButton.onClick.AddListener(onClick);
                if (firstButton == null)
                {
                    firstButton = choiceButton;
                }
            }
        }

        if (firstButton != null && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(firstButton.gameObject);
            firstButton.Select();
        }
    }

    private void ClearChoices()
    {
        if (choiceContainer == null)
        {
            return;
        }

        foreach (Transform child in choiceContainer)
        {
            Destroy(child.gameObject);
        }
    }

    private void TeachPlayerSprint()
    {
        if (hasTaughtSprint)
        {
            return;
        }

        hasTaughtSprint = true;

        if (playerController == null)
        {
            FindPlayerReferences();
        }

        playerController?.ApplySprintLesson(sprintLessonMultiplier);
        Debug.Log("[PickpocketThief] Sprint lesson granted. Pickpocket can now be killed.", this);
    }

    private void FreezeInPlace()
    {
        thiefBody.linearVelocity = Vector2.zero;
        thiefBody.bodyType = RigidbodyType2D.Static;

        if (thiefAnimator != null)
        {
            thiefAnimator.enabled = false;
        }
    }

    private void BuildInteractionIcon()
    {
        if (interactionIconSprite == null)
        {
            return;
        }

        GameObject iconObject = new GameObject("InteractionIcon");
        iconObject.transform.SetParent(transform, false);
        iconObject.transform.localPosition = interactionIconOffset;

        interactionIconRenderer = iconObject.AddComponent<SpriteRenderer>();
        interactionIconRenderer.sprite = interactionIconSprite;

        if (thiefSprite != null)
        {
            interactionIconRenderer.sortingLayerID = thiefSprite.sortingLayerID;
            interactionIconRenderer.sortingOrder = thiefSprite.sortingOrder + 1;
        }

        interactionIconRenderer.enabled = false;
    }

    private void ShowInteractionIcon(bool show)
    {
        if (interactionIconRenderer != null)
        {
            interactionIconRenderer.enabled = show;
        }
    }

    private void PlayMoveAnimation(Vector2 velocity)
    {
        if (thiefAnimator == null || velocity.sqrMagnitude < 0.0001f)
        {
            return;
        }

        int nextStateHash = GetMoveStateHash(velocity);
        if (nextStateHash == currentMoveStateHash)
        {
            return;
        }

        thiefAnimator.Play(nextStateHash);
        currentMoveStateHash = nextStateHash;
    }

    private int GetMoveStateHash(Vector2 velocity)
    {
        if (Mathf.Abs(velocity.x) > Mathf.Abs(velocity.y))
        {
            return velocity.x < 0f ? RunLeftStateHash : RunRightStateHash;
        }

        return velocity.y > 0f ? RunUpStateHash : RunDownStateHash;
    }

    private Vector2 GetObstacleAwareVelocity(Vector2 preferredDirection, float speed)
    {
        if (preferredDirection.sqrMagnitude < 0.0001f)
        {
            return Vector2.zero;
        }

        Vector2 normalizedDirection = preferredDirection.normalized;
        avoidanceProbeDirections[0] = normalizedDirection;
        avoidanceProbeDirections[1] = RotateDirection(normalizedDirection, 35f);
        avoidanceProbeDirections[2] = RotateDirection(normalizedDirection, -35f);
        avoidanceProbeDirections[3] = RotateDirection(normalizedDirection, 70f);
        avoidanceProbeDirections[4] = RotateDirection(normalizedDirection, -70f);
        avoidanceProbeDirections[5] = RotateDirection(normalizedDirection, 110f);
        avoidanceProbeDirections[6] = RotateDirection(normalizedDirection, -110f);

        foreach (Vector2 candidateDirection in avoidanceProbeDirections)
        {
            if (!HasBlockingObstacle(candidateDirection))
            {
                return candidateDirection * speed;
            }
        }

        return normalizedDirection * speed;
    }

    private bool HasBlockingObstacle(Vector2 direction)
    {
        RaycastHit2D[] hits = Physics2D.CircleCastAll(
            thiefBody.position,
            obstacleProbeRadius,
            direction,
            obstacleProbeDistance
        );

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider == null
                || hit.collider.isTrigger
                || hit.collider.attachedRigidbody == thiefBody
                || hit.collider.CompareTag("Player")
                || (playerTarget != null && hit.collider.transform.IsChildOf(playerTarget)))
            {
                continue;
            }

            return true;
        }

        return false;
    }

    private static Vector2 RotateDirection(Vector2 direction, float degrees)
    {
        float radians = degrees * Mathf.Deg2Rad;
        float sin = Mathf.Sin(radians);
        float cos = Mathf.Cos(radians);

        return new Vector2(
            (direction.x * cos) - (direction.y * sin),
            (direction.x * sin) + (direction.y * cos)
        ).normalized;
    }

    private IEnumerator HurtFlash()
    {
        Color originalColor = thiefSprite.color;
        thiefSprite.color = flashColor;
        yield return new WaitForSeconds(flashDuration);

        if (!isDead && thiefSprite != null)
        {
            thiefSprite.color = originalColor;
        }

        hurtFlashRoutine = null;
    }

}

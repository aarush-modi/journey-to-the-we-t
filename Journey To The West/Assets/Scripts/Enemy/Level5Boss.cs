using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Level 5 boss. Plays intro dialogue when the player enters the arena,
/// then chases and periodically lunges. On death drops an ArmorPickup prefab.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class Level5Boss : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    [SerializeField] private float maxHP = 200f;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float contactDamage = 20f;
    [SerializeField] private float contactCooldown = 0.5f;

    [Header("Lunge Attack")]
    [SerializeField] private float lungeRange = 5f;
    [SerializeField] private float lungeSpeed = 7f;
    [SerializeField] private float lungeDuration = 0.4f;
    [SerializeField] private float lungeCooldown = 3.5f;

    [Header("Boss Drop")]
    [SerializeField] private GameObject armorPickupPrefab;

    [Header("Sprites")]
    [SerializeField] private Sprite idleSprite;
    [SerializeField] private Sprite walkSprite;
    [SerializeField] private Sprite attackSprite;

    [Header("Hurt Feedback")]
    [SerializeField] private float flashDuration = 0.15f;
    [SerializeField] private Color flashColor = Color.red;

    [Header("Intro Dialogue")]
    [SerializeField] private NPCDialogue introDialogue;
    [SerializeField] private float introTriggerRadius = 8f;
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private GameObject continuePrompt;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private Transform playerTarget;

    private float currentHP;
    private bool isDead;
    private bool isActive;
    private bool introPlayed;
    private float nextDamageTime;
    private float nextLungeTime;
    private bool isLunging;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        currentHP = maxHP;
    }

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTarget = player.transform;

        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (continuePrompt != null) continuePrompt.SetActive(false);

        // Boss waits for intro if dialogue is set; otherwise starts active.
        isActive = (introDialogue == null);
    }

    private void Update()
    {
        if (isDead || introPlayed || playerTarget == null) return;

        float dist = Vector2.Distance(transform.position, playerTarget.position);
        if (dist <= introTriggerRadius)
        {
            introPlayed = true;
            if (introDialogue != null && dialoguePanel != null)
                StartCoroutine(PlayIntroCutscene());
            else
                isActive = true;
        }
    }

    private void FixedUpdate()
    {
        if (isDead || !isActive || playerTarget == null || isLunging) return;

        Vector2 dir = ((Vector2)playerTarget.position - rb.position).normalized;
        rb.linearVelocity = dir * moveSpeed;

        // Flip sprite to face movement direction.
        if (dir.x != 0f && spriteRenderer != null)
            spriteRenderer.flipX = dir.x < 0f;

        if (spriteRenderer != null && walkSprite != null)
            spriteRenderer.sprite = walkSprite;

        if (animator != null)
            animator.SetBool("IsWalking", true);

        float dist = Vector2.Distance(rb.position, playerTarget.position);
        if (dist <= lungeRange && Time.time >= nextLungeTime)
            StartCoroutine(Lunge(dir));
    }

    private IEnumerator Lunge(Vector2 dir)
    {
        isLunging = true;
        nextLungeTime = Time.time + lungeCooldown;

        if (spriteRenderer != null && attackSprite != null)
            spriteRenderer.sprite = attackSprite;
        if (animator != null) animator.SetBool("IsLunging", true);

        rb.linearVelocity = dir * lungeSpeed;
        yield return new WaitForSeconds(lungeDuration);

        isLunging = false;
        if (spriteRenderer != null && walkSprite != null)
            spriteRenderer.sprite = walkSprite;
        if (animator != null) animator.SetBool("IsLunging", false);
    }

    private void OnCollisionEnter2D(Collision2D collision) => TryDamagePlayer(collision.collider);
    private void OnCollisionStay2D(Collision2D collision)  => TryDamagePlayer(collision.collider);

    private void TryDamagePlayer(Collider2D other)
    {
        if (isDead || !isActive || other == null || !other.CompareTag("Player")) return;
        if (Time.time < nextDamageTime) return;
        if (other.TryGetComponent(out PlayerCombat pc))
        {
            pc.TakeDamage(contactDamage);
            nextDamageTime = Time.time + contactCooldown;
        }
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;
        currentHP -= amount;
        StartCoroutine(HurtFlash());
        if (currentHP <= 0f) Die();
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        rb.linearVelocity = Vector2.zero;

        if (animator != null)
        {
            animator.SetBool("IsWalking", false);
            animator.SetBool("IsLunging", false);
        }

        if (armorPickupPrefab != null)
            Instantiate(armorPickupPrefab, transform.position, Quaternion.identity);

        gameObject.SetActive(false);
    }

    public bool IsDead() => isDead;

    private IEnumerator HurtFlash()
    {
        if (spriteRenderer == null) yield break;
        Color original = spriteRenderer.color;
        spriteRenderer.color = flashColor;
        yield return new WaitForSeconds(flashDuration);
        if (!isDead && spriteRenderer != null)
            spriteRenderer.color = original;
    }

    private IEnumerator PlayIntroCutscene()
    {
        PauseController.SetPause(true);
        dialoguePanel.SetActive(true);
        dialoguePanel.transform.SetAsLastSibling();

        for (int i = 0; i < introDialogue.dialogue.Length; i++)
        {
            if (nameText != null) nameText.text = introDialogue.npcName;
            yield return StartCoroutine(TypeLine(introDialogue.dialogue[i], introDialogue.typingSpeed));

            if (continuePrompt != null) continuePrompt.SetActive(true);
            yield return new WaitUntil(() => UnityEngine.InputSystem.Keyboard.current.eKey.wasPressedThisFrame);
            if (continuePrompt != null) continuePrompt.SetActive(false);
        }

        dialoguePanel.SetActive(false);
        PauseController.SetPause(false);
        if (spriteRenderer != null && walkSprite != null)
            spriteRenderer.sprite = walkSprite;
        isActive = true;
    }

    private IEnumerator TypeLine(string line, float speed)
    {
        dialogueText.text = "";
        foreach (char c in line)
        {
            dialogueText.text += c;
            yield return new WaitForSecondsRealtime(speed);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, introTriggerRadius);
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, lungeRange);
    }
}

using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour, IDamageable
{
    public UnityEvent<float, float> OnHPChanged;
    public UnityEvent<float> OnSkillActivated;
    [Header("HP")]
    [SerializeField] private float maxHP = 100f;
    private float currentHP;

    [Header("Attack")]
    [SerializeField] private float baseDamage = 10f;
    [SerializeField] private float attackCooldown = 0.5f;
    [SerializeField] private float hitboxDuration = 0.15f;
    [SerializeField] private GameObject meleeHitbox;

    [Header("Equipment")]
    [SerializeField] private SkillData equippedSkill;
    [SerializeField] private ArmorData equippedArmor;

    [Header("Death")]
    [SerializeField] private GameObject droppedGoldPrefab;

    [Header("Hurt Feedback")]
    [SerializeField] private float flashDuration = 0.1f;
    [SerializeField] private Color flashColor = Color.red;

    private GreedMeter greedMeter;
    private PlayerController playerController;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private float attackCooldownTimer;
    private float skillCooldownTimer;
    private Vector3 lastCheckpoint;
    private bool isDead;

    private void Awake()
    {
        greedMeter = GetComponent<GreedMeter>();
        playerController = GetComponent<PlayerController>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        currentHP = maxHP;
        lastCheckpoint = transform.position;
        meleeHitbox.SetActive(false);
        OnHPChanged?.Invoke(currentHP, maxHP);
    }

    private void Update()
    {
        if (attackCooldownTimer > 0f)
            attackCooldownTimer -= Time.deltaTime;

        if (skillCooldownTimer > 0f)
            skillCooldownTimer -= Time.deltaTime;
    }

    // --- Input ---

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (!context.performed || isDead) return;
        if (attackCooldownTimer > 0f) return;

        attackCooldownTimer = attackCooldown;

        // Update last direction so the attack blend tree picks the right clip
        Vector2 moveInput = animator.GetBool("isWalking")
            ? new Vector2(animator.GetFloat("InputX"), animator.GetFloat("InputY"))
            : Vector2.zero;

        if (moveInput != Vector2.zero)
        {
            animator.SetFloat("LastInputX", moveInput.x);
            animator.SetFloat("LastInputY", moveInput.y);
        }

        animator.SetTrigger("Attack");
        StartCoroutine(ActivateHitbox());
    }

    public void OnSkill(InputAction.CallbackContext context)
    {
        if (!context.performed || isDead) return;
        ActivateSkill();
    }

    // --- Attack ---

    private IEnumerator ActivateHitbox()
    {
        meleeHitbox.SetActive(true);
        yield return new WaitForSeconds(hitboxDuration);
        meleeHitbox.SetActive(false);
    }

    public float GetAttackDamage()
    {
        return baseDamage * greedMeter.GetDamageMultiplier();
    }

    // --- IDamageable ---

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHP -= amount;
        StartCoroutine(HurtFlash());
        OnHPChanged?.Invoke(currentHP, maxHP);

        if (currentHP <= 0f)
        {
            currentHP = 0f;
            OnHPChanged?.Invoke(currentHP, maxHP);
            Die();
        }
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;

        // Drop gold
        int gold = greedMeter.GetCurrentGold();
        if (gold > 0 && droppedGoldPrefab != null)
        {
            GameObject drop = Instantiate(droppedGoldPrefab, transform.position, Quaternion.identity);
            drop.GetComponent<DroppedGold>().SetGoldAmount(gold);
            greedMeter.RemoveGold(gold);
        }

        // Respawn
        StartCoroutine(RespawnAfterDelay(0.5f));
    }

    private IEnumerator RespawnAfterDelay(float delay)
    {
        // Brief pause before respawn
        yield return new WaitForSeconds(delay);

        playerController.Respawn(lastCheckpoint);
        currentHP = maxHP;
        isDead = false;
        OnHPChanged?.Invoke(currentHP, maxHP);
    }

    private IEnumerator HurtFlash()
    {
        Color original = spriteRenderer.color;
        spriteRenderer.color = flashColor;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = original;
    }

    // --- Equipment ---

    public void EquipSkill(SkillData skill)
    {
        equippedSkill = skill;
    }

    public void EquipArmor(ArmorData armor)
    {
        equippedArmor = armor;
        if (armor != null && armor.armorSprite != null)
        {
            spriteRenderer.sprite = armor.armorSprite;
        }
    }

    public void ActivateSkill()
    {
        if (equippedSkill == null) return;
        if (skillCooldownTimer > 0f) return;

        skillCooldownTimer = equippedSkill.cooldown;
        OnSkillActivated?.Invoke(equippedSkill.cooldown);

        // Skill activation logic will be filled by later tickets
        Debug.Log($"Activated skill: {equippedSkill.skillName}");
    }

    // --- Checkpoint ---

    public void SetCheckpoint(Vector3 position)
    {
        lastCheckpoint = position;
    }

    // --- Getters ---

    public float GetCurrentHP() => currentHP;
    public float GetMaxHP() => maxHP;
    public bool IsDead() => isDead;
    public SkillData GetEquippedSkill() => equippedSkill;
    public bool IsAttacking()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        return stateInfo.IsTag("Attack");
    }
}

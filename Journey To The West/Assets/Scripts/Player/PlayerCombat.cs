using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class PlayerCombat : MonoBehaviour, IDamageable
{
    public UnityEvent<float, float> OnHPChanged;
    public UnityEvent<float> OnSkillActivated;
    [Header("HP")]
    [FormerlySerializedAs("maxHP")]
    [SerializeField] private float baseMaxHP = 100f;
    private float currentHP;
    private float effectiveMaxHP;
    private float maxHPModifier = 1f;

    [Header("Attack")]
    [SerializeField] private float baseDamage = 10f;
    [SerializeField] private float attackCooldown = 0.5f;

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
        effectiveMaxHP = baseMaxHP * maxHPModifier;
        currentHP = effectiveMaxHP;
    }

    private void Start()
    {
        effectiveMaxHP = baseMaxHP * maxHPModifier;
        currentHP = effectiveMaxHP;
        lastCheckpoint = transform.position;
        HustleStyleManager.Instance?.RefreshStyleEffects();
        OnHPChanged?.Invoke(currentHP, effectiveMaxHP);
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
        if (PauseController.IsGamePaused) return;
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
    }

    public void OnSkill(InputAction.CallbackContext context)
    {
        if (PauseController.IsGamePaused) return;
        if (!context.performed || isDead) return;
        ActivateSkill();
    }

    // --- Attack ---

    public float GetAttackDamage()
    {
        return baseDamage * greedMeter.GetDamageMultiplier();
    }

    // --- IDamageable ---

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        Debug.Log($"TakeDamage: HP before={currentHP}, damage={amount}");
        currentHP = Mathf.Max(0f, currentHP - amount);
        StartCoroutine(HurtFlash());
        OnHPChanged?.Invoke(currentHP, effectiveMaxHP);

        if (currentHP <= 0f)
        {
            currentHP = 0f;
            OnHPChanged?.Invoke(currentHP, effectiveMaxHP);
            Die();
        }
    }

    public void Heal(float amount)
    {
        if (isDead) return;

        currentHP = Mathf.Min(currentHP + amount, effectiveMaxHP);
        OnHPChanged?.Invoke(currentHP, effectiveMaxHP);
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
        currentHP = effectiveMaxHP;
        isDead = false;
        OnHPChanged?.Invoke(currentHP, effectiveMaxHP);
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
        if (playerController == null) return;

        skillCooldownTimer = equippedSkill.cooldown;
        OnSkillActivated?.Invoke(equippedSkill.cooldown);

        equippedSkill.Activate(playerController);
        Debug.Log($"Activated skill: {equippedSkill.skillName}");
    }

    public void ApplyMaxHPModifier(float modifier)
    {
        float sanitizedModifier = modifier > 0f ? modifier : 1f;
        float previousMaxHP = effectiveMaxHP > 0f ? effectiveMaxHP : baseMaxHP;
        float healthPercent = previousMaxHP > 0f ? currentHP / previousMaxHP : 1f;

        maxHPModifier = sanitizedModifier;
        effectiveMaxHP = baseMaxHP * maxHPModifier;
        currentHP = Mathf.Clamp(effectiveMaxHP * healthPercent, 0f, effectiveMaxHP);

        OnHPChanged?.Invoke(currentHP, effectiveMaxHP);
    }

    // --- Checkpoint ---

    public void SetCheckpoint(Vector3 position)
    {
        lastCheckpoint = position;
    }

    // --- Getters ---

    public float GetCurrentHP() => currentHP;
    public float GetBaseMaxHP() => baseMaxHP;
    public float GetMaxHP() => effectiveMaxHP > 0f ? effectiveMaxHP : baseMaxHP * maxHPModifier;
    public bool IsDead() => isDead;
    public SkillData GetEquippedSkill() => equippedSkill;
    public bool IsAttacking()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        return stateInfo.IsTag("Attack");
    }
}

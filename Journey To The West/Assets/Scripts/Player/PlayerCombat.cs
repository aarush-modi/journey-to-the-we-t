using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class PlayerCombat : MonoBehaviour, IDamageable
{
    public UnityEvent<float, float> OnHPChanged;
    public UnityEvent<float> OnSkillActivated;
    public UnityEvent OnSkillCooldownReset;
    public UnityEvent<SkillData> OnSkillEquipped;
    [Header("HP")]
    [FormerlySerializedAs("maxHP")]
    [SerializeField] private float baseMaxHP = 100f;
    private float currentHP;
    private float effectiveMaxHP;
    private float maxHPModifier = 1f;

    [Header("Attack")]
    [SerializeField] private float baseDamage = 10f;
    [SerializeField] private float attackCooldown = 0.5f;
    private float damageBoost = 1f;

    [Header("Equipment")]
    [SerializeField] private SkillData equippedSkill;
    [SerializeField] private ArmorData equippedArmor;
    [SerializeField] private SkillData defaultSkill;

    [Header("Death")]
    [SerializeField] private GameObject droppedGoldPrefab;
    [SerializeField, Tooltip("Flat amount of gold lost and dropped upon death")] 
    private int deathGoldLossAmount = 20;

    [Header("Dash")]
    [SerializeField] private DashAttackHandler dashAttackHandler;

    [Header("Hurt Feedback")]
    [SerializeField] private float flashDuration = 0.1f;
    [SerializeField] private Color flashColor = Color.red;

    private GreedMeter greedMeter;
    private PlayerShield playerShield;
    private PlayerController playerController;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private float attackCooldownTimer;
    private float skillCooldownTimer;
    private float invulnerableUntil;
    private Vector3 lastCheckpoint;
    private bool isDead;
    private bool chainDashReady;

    private void Awake()
    {
        greedMeter = GetComponent<GreedMeter>();
        playerShield = GetComponent<PlayerShield>();
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

        // No skill equipped until player selects one from the hotbar
        equippedSkill = null;
        if (dashAttackHandler != null)
            dashAttackHandler.SetReticleActive(false);
    }

    private void Update()
    {
        if (attackCooldownTimer > 0f)
            attackCooldownTimer -= Time.deltaTime;

        if (skillCooldownTimer > 0f)
        {
            skillCooldownTimer -= Time.deltaTime;
            if (skillCooldownTimer <= 0f)
            {
                skillCooldownTimer = 0f;
                Debug.Log("[Skill] Cooldown expired — skill ready again");
                OnSkillCooldownReset?.Invoke();
            }
        }

        // Direct right-click check for skill activation
        if (!PauseController.IsGamePaused && !isDead && !IsActionLocked())
        {
            if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
            {
                ActivateSkill();
            }
        }
    }

    // --- Input ---

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (PauseController.IsGamePaused) return;
        if (!context.performed || isDead) return;
        if (attackCooldownTimer > 0f) return;
        if (dashAttackHandler != null && dashAttackHandler.IsLocked()) return;

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
        if (!chainDashReady && IsAttacking()) return;
        ActivateSkill();
    }

    // --- Attack ---

    public float GetAttackDamage()
    {
        float dmg = baseDamage * greedMeter.GetDamageMultiplier() * damageBoost;
        Debug.Log($"[Combat] baseDamage={baseDamage}, greedMult={greedMeter.GetDamageMultiplier()}, damageBoost={damageBoost}, total={dmg}");
        return dmg;
    }

    public void ApplyDamageBoost(float multiplier)
    {
        damageBoost = multiplier;
    }

    public void GrantIframes(float duration)
    {
        invulnerableUntil = Time.time + duration;
    }

    // --- IDamageable ---

    public void TakeDamage(float amount)
    {
        if (isDead) return;
        if (dashAttackHandler != null && dashAttackHandler.IsDashing) return;
        if (Time.time < invulnerableUntil) return;

        if (playerShield != null && playerShield.TryAbsorbHit()) return;

        if (equippedArmor != null)
            amount = Mathf.Max(0f, amount - equippedArmor.damageReduction);

        Debug.Log($"TakeDamage: HP before={currentHP}, damage={amount}");
        currentHP = Mathf.Max(0f, currentHP - amount);
        if (hurtFlashRoutine != null) StopCoroutine(hurtFlashRoutine);
        hurtFlashRoutine = StartCoroutine(HurtFlash());
        DamageVignette.Instance?.Flash();
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

        // Drop a portion of gold
        int currentGold = greedMeter.GetCurrentGold();
        int goldToDrop = Mathf.Min(currentGold, deathGoldLossAmount);

        if (goldToDrop > 0)
        {
            if (droppedGoldPrefab != null)
            {
                GameObject drop = Instantiate(droppedGoldPrefab, transform.position, Quaternion.identity);
                drop.GetComponent<DroppedGold>().SetGoldAmount(goldToDrop);
            }
            
            // Remove the dropped amount from the player's inventory
            greedMeter.RemoveGold(goldToDrop);
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

    private Coroutine hurtFlashRoutine;

    private IEnumerator HurtFlash()
    {
        spriteRenderer.color = flashColor;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = Color.white;
        hurtFlashRoutine = null;
    }

    // --- Equipment ---

    public void EquipSkill(SkillData skill)
    {
        // Hide reticle from previous skill if it was a dash skill
        if (dashAttackHandler != null && equippedSkill is DashAttackSkill)
        {
            dashAttackHandler.SetReticleActive(false);
        }

        equippedSkill = skill;
        OnSkillEquipped?.Invoke(skill);

        // Show reticle if new skill is a dash skill
        if (dashAttackHandler != null && skill is DashAttackSkill newDashSkill)
        {
            dashAttackHandler.SetReticleActive(true, newDashSkill);
        }
    }

    public void EquipArmor(ArmorData armor)
    {
        equippedArmor = armor;
    }

    public void ActivateSkill()
    {
        if (equippedSkill == null) return;
        if (skillCooldownTimer > 0f) return;
        if (playerController == null) return;

        chainDashReady = false;
        skillCooldownTimer = equippedSkill.cooldown;
        Debug.Log($"[Skill] Used: {equippedSkill.skillName} — cooldown {equippedSkill.cooldown}s");
        OnSkillActivated?.Invoke(equippedSkill.cooldown);

        equippedSkill.Activate(playerController);
    }

    public void SetSkillCooldown(float duration)
    {
        Debug.Log($"[Skill] Cooldown extended to {duration}s (was {skillCooldownTimer}s)");
        float newTimer = Mathf.Max(skillCooldownTimer, duration);
        if (newTimer > skillCooldownTimer)
        {
            skillCooldownTimer = newTimer;
            OnSkillActivated?.Invoke(skillCooldownTimer);
        }
        else
        {
            skillCooldownTimer = newTimer;
        }
    }

    public void ResetSkillCooldown()
    {
        skillCooldownTimer = 0f;
        chainDashReady = true;
        Debug.Log("[Skill] Cooldown reset (chain dash) — skill ready again");
        OnSkillCooldownReset?.Invoke();
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

    public bool IsActionLocked()
    {
        if (chainDashReady) return false;
        if (IsAttacking()) return true;
        if (dashAttackHandler != null && dashAttackHandler.IsLocked()) return true;
        return false;
    }
}

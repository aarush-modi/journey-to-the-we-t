using System.Collections;
using UnityEngine;

public class EnemyController : MonoBehaviour, IDamageable
{
    [SerializeField] private EnemyData enemyData;
    [SerializeField] private GameObject droppedGoldPrefab;

    [Header("Hurt Feedback")]
    [SerializeField] private float flashDuration = 0.1f;
    [SerializeField] private Color flashColor = Color.red;

    private float currentHP;
    private SpriteRenderer spriteRenderer;
    private bool isDead;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        currentHP = enemyData.maxHP;

        if (enemyData.sprite != null)
        {
            spriteRenderer.sprite = enemyData.sprite;
        }
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHP -= amount;
        StartCoroutine(HurtFlash());

        if (currentHP <= 0f)
        {
            currentHP = 0f;
            Die();
        }
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;

        DropGold();
        gameObject.SetActive(false);
    }

    private void DropGold()
    {
        if (droppedGoldPrefab == null) return;

        float modifier = 1f;
        if (HustleStyleManager.Instance != null)
        {
            modifier = HustleStyleManager.Instance.GetCombatGoldModifier();
        }

        int finalGold = Mathf.RoundToInt(enemyData.baseGoldDrop * modifier);

        if (finalGold > 0)
        {
            GameObject drop = Instantiate(droppedGoldPrefab, transform.position, Quaternion.identity);
            drop.GetComponent<DroppedGold>().SetGoldAmount(finalGold);
        }
    }

    private IEnumerator HurtFlash()
    {
        Color original = spriteRenderer.color;
        spriteRenderer.color = flashColor;
        yield return new WaitForSeconds(flashDuration);
        if (!isDead)
            spriteRenderer.color = original;
    }
}

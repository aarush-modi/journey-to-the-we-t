using UnityEngine;

public class EnemyController : MonoBehaviour, IDamageable
{
    [Header("Data")]
    public EnemyData enemyData;

    private int currentHP;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        // Initialize from ScriptableObject
        currentHP = enemyData.maxHP;
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && enemyData.sprite != null)
            spriteRenderer.sprite = enemyData.sprite;
    }

    public void TakeDamage(float amount)
    {
        currentHP -= (int)amount;
        StartCoroutine(HurtFlash()); // visual feedback

        if (currentHP <= 0)
            Die();
    }

    private System.Collections.IEnumerator HurtFlash()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = Color.white;
        }
    }

    public void Die()
    {
        DropGold();
        gameObject.SetActive(false);
    }

    private void DropGold()
    {
        // Find the player to read their hustle style modifier
        GameObject player = GameObject.FindWithTag("Player");
        float goldModifier = 1f;

        /*
        if (player != null)
        {
            HustleStyleData hustleData = player.GetComponent<HustleStyleData>();
            // or however HustleStyleData is accessed on your player
            if (hustleData != null)
                goldModifier = hustleData.combatGoldModifier;
        }
        */

        int goldAmount = Mathf.RoundToInt(enemyData.baseGoldDrop * goldModifier);

        // Spawn the DroppedGold prefab — reuse your existing prefab
        // Load it from Resources or assign via [SerializeField]
        // Example with a serialized field:
        if (goldPickupPrefab != null)
        {
            GameObject pickup = Instantiate(goldPickupPrefab, transform.position, Quaternion.identity);
            // If your DroppedGold has a script that holds a gold value, set it here:
            DroppedGold droppedGold = pickup.GetComponent<DroppedGold>();
            if (droppedGold != null)
                droppedGold.SetGoldAmount(goldAmount);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            Debug.Log("K pressed on " + gameObject.name);
            TakeDamage(10f);
        }
    }


    [Header("Prefab Reference")]
    [SerializeField] private GameObject goldPickupPrefab;
}
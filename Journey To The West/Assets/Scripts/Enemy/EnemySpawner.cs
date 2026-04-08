using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    [Tooltip("The Enemy Prefab to spawn (e.g., MeleeEnemy or RangedEnemy)")]
    [SerializeField] private GameObject enemyPrefab;
    
    [Tooltip("How many seconds to wait after the enemy dies before respawning")]
    [SerializeField] private float respawnTime = 60f;

    [Tooltip("Spawn precisely at this spawner's location when the game starts?")]
    [SerializeField] private bool spawnOnStart = true;

    private GameObject currentSpawnedEnemy;
    private bool isRespawning;

    private void Start()
    {
        // Hide the spawner's visual body in the actual game if you added a sprite to the Spawner object itself
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.enabled = false;

        if (spawnOnStart)
        {
            SpawnEnemy();
        }
    }

    private void Update()
    {
        if (isRespawning) return;

        bool needsRespawn = false;

        // Catch if the enemy was destroyed completely
        if (currentSpawnedEnemy == null)
        {
            // Only respawn if game started
            if (spawnOnStart) needsRespawn = true;
        }
        // Catch if the enemy disabled itself due to dying
        else if (!currentSpawnedEnemy.activeInHierarchy)
        {
            needsRespawn = true;
        }
        // Catch if the enemy is marked dead but hasn't fully despawned yet
        else if (currentSpawnedEnemy.TryGetComponent(out IDamageable damageable) && damageable.IsDead())
        {
            needsRespawn = true;
        }

        if (needsRespawn && spawnOnStart) // Only allow respawn cycles if the spawner has formally started
        {
            // Destroy the dead body so we don't leak memory or leave fake enemies around
            if (currentSpawnedEnemy != null)
            {
                Destroy(currentSpawnedEnemy);
                currentSpawnedEnemy = null;
            }

            StartCoroutine(RespawnRoutine());
        }
    }

    private void SpawnEnemy()
    {
        if (enemyPrefab != null)
        {
            currentSpawnedEnemy = Instantiate(enemyPrefab, transform.position, transform.rotation);
        }
        else
        {
            Debug.LogWarning($"EnemySpawner at {transform.position} is missing an enemyPrefab!");
        }
    }

    private IEnumerator RespawnRoutine()
    {
        isRespawning = true;
        
        yield return new WaitForSeconds(respawnTime);
        
        SpawnEnemy();
        
        isRespawning = false;
    }

    private void OnDrawGizmos()
    {
        // Draw a red circle in the Unity Scene view so you can see your invisible spawners
        Gizmos.color = new Color(1f, 0f, 0f, 0.4f);
        Gizmos.DrawSphere(transform.position, 0.4f);
    }
}

using System.Collections;
using UnityEngine;

public class EnemyDeathEffect : MonoBehaviour
{
    [SerializeField] private GameObject deathParticlePrefab;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void PlayDeath()
    {
        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {
        if (spriteRenderer == null)
        {
            gameObject.SetActive(false);
            yield break;
        }

        // Instantly hide the enemy
        spriteRenderer.enabled = false;

        // Spawn explosion VFX
        if (deathParticlePrefab != null)
        {
            Instantiate(deathParticlePrefab, transform.position, Quaternion.identity);
        }

        // Wait one frame for VFX to spawn before deactivating
        yield return null;

        // Reset for reuse and deactivate
        spriteRenderer.enabled = true;
        gameObject.SetActive(false);
    }
}

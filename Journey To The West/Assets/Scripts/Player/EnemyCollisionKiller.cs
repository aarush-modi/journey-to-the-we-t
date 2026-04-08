using UnityEngine;

public class EnemyCollisionKiller : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Check for enemy collision
        IDamageable enemy = collision.gameObject.GetComponent<IDamageable>();
        if (enemy != null && !enemy.IsDead())
        {
            // Break all shields first
            EnemyShield shield = collision.gameObject.GetComponent<EnemyShield>();
            if (shield != null)
            {
                while (shield.HasShields())
                {
                    shield.TryAbsorbHit();
                }
            }
            enemy.TakeDamage(20f); // Deal 14 damage (30% less than base dash damage)
        }
    }
}
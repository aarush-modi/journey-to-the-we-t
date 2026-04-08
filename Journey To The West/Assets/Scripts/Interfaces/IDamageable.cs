//using UnityEngine;

public interface IDamageable
{
    void TakeDamage(float amount);
    void Die();
    bool IsDead();
}

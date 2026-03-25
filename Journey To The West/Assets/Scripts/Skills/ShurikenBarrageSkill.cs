using UnityEngine;

[CreateAssetMenu(fileName = "ShurikenBarrageSkill", menuName = "Skills/ShurikenBarrage")]
public class ShurikenBarrageSkill : SkillData
{
    [Header("Projectile")]
    [SerializeField] private ShurikenProjectile projectilePrefab;
    [SerializeField] private float projectileSpeed = 12f;
    [SerializeField] private float damage = 15f;

    [Header("Spread")]
    [SerializeField] private int projectileCount = 5;
    [SerializeField] private float spreadAngle = 45f;
    [SerializeField] private float spawnOffset = 0.35f;

    public override void Activate(PlayerController user)
    {
        if (user == null || projectilePrefab == null || projectileCount <= 0)
        {
            return;
        }

        Vector2 facingDirection = user.GetFacingDirection();
        if (facingDirection.sqrMagnitude <= 0f)
        {
            facingDirection = Vector2.down;
        }

        float baseAngle = Mathf.Atan2(facingDirection.y, facingDirection.x) * Mathf.Rad2Deg;
        float stepAngle = projectileCount == 1 ? 0f : spreadAngle / (projectileCount - 1);
        float startAngle = baseAngle - (spreadAngle * 0.5f);
        Vector3 spawnPosition = user.transform.position + (Vector3)(facingDirection.normalized * spawnOffset);

        for (int i = 0; i < projectileCount; i++)
        {
            float angle = startAngle + (stepAngle * i);
            Quaternion rotation = Quaternion.Euler(0f, 0f, angle);
            Vector2 direction = rotation * Vector2.right;

            ShurikenProjectile projectile = Instantiate(projectilePrefab, spawnPosition, rotation);
            projectile.Initialize(user.gameObject, direction, projectileSpeed, damage);
        }
    }
}

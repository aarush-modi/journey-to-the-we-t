using UnityEngine;

public class EnemyGroupCondition : MonoBehaviour
{
    [SerializeField] private ObjectiveTracker tracker;

    private IDamageable[] enemies;

    private void Start()
    {
        enemies = GetComponentsInChildren<IDamageable>(true);
    }

    private void Update()
    {
        if (tracker == null || tracker.IsComplete) return;
        if (enemies == null || enemies.Length == 0)
        {
            tracker.SetComplete();
            return;
        }

        foreach (IDamageable enemy in enemies)
        {
            if (enemy != null && !enemy.IsDead())
                return;
        }

        tracker.SetComplete();
    }
}

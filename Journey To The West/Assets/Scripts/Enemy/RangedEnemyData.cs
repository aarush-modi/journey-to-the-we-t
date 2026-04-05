using UnityEngine;

[CreateAssetMenu(fileName = "NewRangedEnemy", menuName = "Scriptable Objects/Ranged Enemy Data")]
public class RangedEnemyData : ScriptableObject
{
    [Header("Stats")]
    public float maxHP = 40f;
    public float contactDamage = 8f;
    public float contactCooldown = 0.5f;

    [Header("Movement")]
    public float patrolSpeed = 1.5f;
    public float chaseSpeed = 3f;
    public float combatMoveSpeed = 2.5f;
    public float retreatSpeed = 3.5f;

    [Header("Patrol")]
    public float waypointReachDist = 0.3f;
    public float waypointPauseDuration = 1f;

    [Header("Combat - Ranges")]
    public float preferredDistance = 7f;
    public float minSafeDistance = 3f;
    public float maxEngageDistance = 12f;

    [Header("Combat - Shooting")]
    public float projectileSpeed = 8f;
    public float projectileDamage = 12f;
    public float shootCooldown = 1.2f;
    public float aimPauseDuration = 0.3f;

    [Header("Combat - Movement")]
    public float strafeDuration = 0.8f;
    public float strafeChangeChance = 0.3f;

    [Header("Search")]
    public float searchDuration = 4f;
    public float lostSightChaseTime = 2f;

    [Header("Pathfinding")]
    public float pathUpdateInterval = 0.4f;

    [Header("Retreat Avoidance")]
    public float obstacleProbeDistance = 1.5f;
    public float obstacleProbeRadius = 0.35f;

    [Header("Gold")]
    public int baseGoldDrop = 20;
}

using UnityEngine;

[CreateAssetMenu(fileName = "NewMeleeEnemy", menuName = "Scriptable Objects/Melee Enemy Data")]
public class MeleeEnemyData : ScriptableObject
{
    [Header("Stats")]
    public float maxHP = 40f;
    public float contactDamage = 10f;
    public float contactCooldown = 0.5f;

    [Header("Movement")]
    public float patrolSpeed = 1.5f;
    public float chaseSpeed = 3f;

    [Header("Patrol")]
    public float waypointReachDist = 0.3f;
    public float waypointPauseDuration = 1f;

    [Header("Search")]
    public float searchDuration = 4f;
    public float lostSightChaseTime = 2f;

    [Header("Pathfinding")]
    public float pathUpdateInterval = 0.4f;

    [Header("Gold")]
    public int baseGoldDrop = 15;
}

using UnityEngine;

[CreateAssetMenu(fileName = "DashAttackSkill", menuName = "Skills/DashAttack")]
public class DashAttackSkill : SkillData
{
    [Header("Dash")]
    public float dashSpeed = 25f;
    public float baseDashDamage = 20f;
    public float dashRange = 7f;
    public float whiffCooldown = 1.5f;
    public float detectionRadius = 1.5f;

    public override void Activate(PlayerController user)
    {
        if (user == null) return;

        DashAttackHandler handler = user.GetComponent<DashAttackHandler>();
        if (handler != null)
        {
            handler.ExecuteDash(this);
        }
        else
        {
            Debug.LogWarning("DashAttackSkill: No DashAttackHandler found on player.");
        }
    }
}

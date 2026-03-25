using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemy", menuName = "Scriptable Objects/Enemy Data")]
public class EnemyData : ScriptableObject
{
    public string enemyName;
    public int maxHP;
    public int damage;
    public int baseGoldDrop;
    public float moveSpeed;
    public Sprite sprite;
}

using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Scriptable Objects/EnemyData")]
public class EnemyData : ScriptableObject
{
    public string enemyName;
    public int maxHP;
    public int damage;
    public int baseGoldDrop;
    public float moveSpeed;
    public Sprite sprite;

}

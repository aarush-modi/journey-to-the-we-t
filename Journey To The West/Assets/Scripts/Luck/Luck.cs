using UnityEngine;

[DisallowMultipleComponent]
public class Luck : MonoBehaviour
{
    [SerializeField]
    [Range(0f, 100f)]
    private float luckPercent;

    public float GetLuckPercent() => luckPercent;

    public bool ShouldNegateDamage()
    {
        return Random.Range(0f, 100f) < luckPercent;
    }
}

using UnityEngine;

public class HitVFX : MonoBehaviour
{
    [SerializeField] private float lifetime = 0.3f;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }
}

using UnityEngine;
using UnityEngine.InputSystem;

public class JinShurikenThrower : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private ShurikenProjectile shurikenProjectilePrefab;

    [Header("Tuning")]
    [SerializeField] private float projectileSpeed = 12f;
    [SerializeField] private float damage = 15f;
    [SerializeField] private float spawnOffset = 0.35f;
    [SerializeField] private float cooldownSeconds = 0.25f;

    [Header("Input")]
    [SerializeField] private bool useRightClick = true;

    [Header("Camera (optional)")]
    [SerializeField] private Camera worldCamera;

    private float nextFireTime;

    private void Awake()
    {
        if (worldCamera == null)
            worldCamera = Camera.main != null ? Camera.main : FindAnyObjectByType<Camera>();
    }

    private void Update()
    {
        if (shurikenProjectilePrefab == null) return;
        if (Time.time < nextFireTime) return;

        Mouse mouse = Mouse.current;
        if (mouse == null) return;

        bool pressed = useRightClick ? mouse.rightButton.wasPressedThisFrame : mouse.leftButton.wasPressedThisFrame;
        if (!pressed) return;

        if (worldCamera == null)
            worldCamera = Camera.main != null ? Camera.main : FindAnyObjectByType<Camera>();
        if (worldCamera == null) return;

        Vector2 mouseScreen = mouse.position.ReadValue();
        Vector3 mouseWorld = worldCamera.ScreenToWorldPoint(new Vector3(mouseScreen.x, mouseScreen.y, 0f));
        Vector2 dir = (Vector2)(mouseWorld - transform.position);
        if (dir.sqrMagnitude <= 0.0001f) return;

        Vector3 spawnPos = transform.position + (Vector3)(dir.normalized * spawnOffset);
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Quaternion rot = Quaternion.Euler(0f, 0f, angle);

        ShurikenProjectile projectile = Instantiate(shurikenProjectilePrefab, spawnPos, rot);
        projectile.Initialize(gameObject, dir, projectileSpeed, damage);

        nextFireTime = Time.time + Mathf.Max(0f, cooldownSeconds);
    }
}


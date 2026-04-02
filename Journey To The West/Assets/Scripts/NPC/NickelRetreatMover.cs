using System.Collections;
using UnityEngine;

public class NickelRetreatMover : MonoBehaviour
{
    [SerializeField] private Vector2 retreatOffset = new Vector2(-2f, 0f);
    [SerializeField] private float retreatDuration = 0.25f;

    private bool isMoving;
    private Collider2D[] colliders;

    public bool IsMoving => isMoving;

    private void Awake()
    {
        colliders = GetComponents<Collider2D>();
    }

    public void Retreat()
    {
        if (isMoving) return;
        StartCoroutine(RetreatRoutine());
    }

    private IEnumerator RetreatRoutine()
    {
        isMoving = true;
        SetCollidersEnabled(false);

        Vector3 start = transform.position;
        Vector3 end = start + (Vector3)retreatOffset;
        float elapsed = 0f;

        while (elapsed < retreatDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / retreatDuration);
            transform.position = Vector3.Lerp(start, end, t);
            yield return null;
        }

        transform.position = end;
        SetCollidersEnabled(true);
        isMoving = false;
    }

    private void SetCollidersEnabled(bool enabled)
    {
        if (colliders == null) return;

        foreach (Collider2D collider in colliders)
        {
            if (collider != null)
                collider.enabled = enabled;
        }
    }
}

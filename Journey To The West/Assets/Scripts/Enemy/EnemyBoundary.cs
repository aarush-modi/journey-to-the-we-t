using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Place on a GameObject with a Collider2D (set to trigger).
/// All enemies automatically stay within this boundary.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class EnemyBoundary : MonoBehaviour
{
    public static EnemyBoundary Instance { get; private set; }

    public Collider2D BoundsCollider { get; private set; }

    private static readonly List<EnemyBoundary> allBoundaries = new List<EnemyBoundary>();

    private void Awake()
    {
        allBoundaries.Add(this);
        BoundsCollider = GetComponent<Collider2D>();
        BoundsCollider.isTrigger = true;
        if (Instance == null)
            Instance = this;
    }

    private void OnDestroy()
    {
        allBoundaries.Remove(this);
        if (Instance == this)
            Instance = null;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            Instance = this;
    }

    public static EnemyBoundary FindContaining(Vector2 position)
    {
        foreach (EnemyBoundary boundary in allBoundaries)
        {
            if (boundary.BoundsCollider != null && boundary.BoundsCollider.OverlapPoint(position))
                return boundary;
        }
        return Instance;
    }
}

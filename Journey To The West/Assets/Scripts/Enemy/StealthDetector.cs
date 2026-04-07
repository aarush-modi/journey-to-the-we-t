using System;
using UnityEngine;

public enum DetectionState { Unaware, Suspicious, Alerted }
public enum SuspicionLevel { None, Low, Medium, High }

/// <summary>
/// Tracks how aware this enemy is of the player using a fill meter.
/// Attach alongside any enemy script. Other components subscribe to OnStateChanged.
///
/// The meter is split into graduated suspicion tiers while building toward full alert:
///   0           → Unaware  (None)
///   0   – 0.33  → Suspicious (Low   — !)
///   0.33– 0.66  → Suspicious (Medium— !!)
///   0.66– 1.0   → Suspicious (High  — !!!)
///   1.0         → Alerted   (attacking)
/// </summary>
public class StealthDetector : MonoBehaviour
{
    [Header("Detection Range")]
    [SerializeField] private float unawareRange = 4f;
    [SerializeField] private float alertedRange = 10f;
    [SerializeField] private LayerMask obstacleLayers;

    [Header("Detection Timing")]
    [SerializeField] private float timeToAlert = 3f;
    [SerializeField] private float drainDelay = 0.5f;
    [SerializeField] private float timeToCalm = 4f;

    [Header("Suspicion Thresholds")]
    [SerializeField] private float mediumThreshold = 0.33f;
    [SerializeField] private float highThreshold = 0.66f;

    public event Action<DetectionState> OnStateChanged;
    public event Action<SuspicionLevel> OnSuspicionLevelChanged;

    private DetectionState _state = DetectionState.Unaware;
    private SuspicionLevel _suspicionLevel = SuspicionLevel.None;
    private float _meter;
    private float _drainTimer;
    private Transform _playerTransform;

    public DetectionState State => _state;
    public SuspicionLevel CurrentSuspicionLevel => _suspicionLevel;
    public float DetectionMeter => _meter;
    public LayerMask ObstacleLayers => obstacleLayers;

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) _playerTransform = player.transform;
    }

    private void Update()
    {
        if (_playerTransform == null) return;

        bool canSee = CanSeePlayer();

        if (canSee)
        {
            _drainTimer = drainDelay;
            _meter += Time.deltaTime / timeToAlert;
        }
        else if (_drainTimer > 0f)
        {
            _drainTimer -= Time.deltaTime;
        }
        else
        {
            _meter -= Time.deltaTime / timeToCalm;
        }

        _meter = Mathf.Clamp01(_meter);
        EvaluateState();
    }

    private bool CanSeePlayer()
    {
        Vector2 toPlayer = (Vector2)_playerTransform.position - (Vector2)transform.position;
        float dist = toPlayer.magnitude;

        float range = _state == DetectionState.Alerted ? alertedRange : unawareRange;
        if (dist > range) return false;

        if (obstacleLayers != 0)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, toPlayer.normalized, dist, obstacleLayers);
            if (hit.collider != null) return false;
        }

        return true;
    }

    private void EvaluateState()
    {
        DetectionState nextState;
        SuspicionLevel nextLevel;

        if (_meter >= 1f)
        {
            nextState = DetectionState.Alerted;
            nextLevel = SuspicionLevel.High;
        }
        else if (_meter > 0f)
        {
            nextState = DetectionState.Suspicious;

            if (_meter >= highThreshold)
                nextLevel = SuspicionLevel.High;
            else if (_meter >= mediumThreshold)
                nextLevel = SuspicionLevel.Medium;
            else
                nextLevel = SuspicionLevel.Low;
        }
        else
        {
            nextState = DetectionState.Unaware;
            nextLevel = SuspicionLevel.None;
        }

        if (nextLevel != _suspicionLevel)
        {
            _suspicionLevel = nextLevel;
            OnSuspicionLevelChanged?.Invoke(_suspicionLevel);
        }

        if (nextState != _state)
        {
            _state = nextState;
            OnStateChanged?.Invoke(_state);
        }
    }

    public void ForceAlert()
    {
        _meter = 1f;
        EvaluateState();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 1f, 0f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, unawareRange);
        Gizmos.color = new Color(1f, 0f, 0f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, alertedRange);
    }
}

using System;
using UnityEngine;

public enum DetectionState { Unaware, Suspicious, Alerted }

/// <summary>
/// Tracks how aware this enemy is of the player using a fill meter.
/// Attach alongside any enemy script. Other components subscribe to OnStateChanged.
/// </summary>
public class StealthDetector : MonoBehaviour
{
    [Header("Detection Range")]
    [SerializeField] private float unawareRange = 4f;
    [SerializeField] private float alertedRange = 10f;
    [SerializeField] private LayerMask obstacleLayers;

    [Header("Detection Timing")]
    [SerializeField] private float timeToAlert = 1.5f;
    [SerializeField] private float drainDelay = 0.5f;
    [SerializeField] private float timeToCalm = 4f;

    [Header("Detection Icons (optional)")]
    [SerializeField] private GameObject suspicionIcon;
    [SerializeField] private GameObject alertIcon;

    public event Action<DetectionState> OnStateChanged;

    private DetectionState _state = DetectionState.Unaware;
    private float _meter = 0f;
    private float _drainTimer = 0f;
    private Transform _playerTransform;

    public DetectionState State => _state;
    public float DetectionMeter => _meter;

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) _playerTransform = player.transform;
        RefreshIcons(_state);
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
        DetectionState next;
        if (_meter >= 1f)       next = DetectionState.Alerted;
        else if (_meter > 0f)   next = DetectionState.Suspicious;
        else                    next = DetectionState.Unaware;

        if (next == _state) return;

        _state = next;
        OnStateChanged?.Invoke(_state);
        RefreshIcons(_state);
    }

    private void RefreshIcons(DetectionState state)
    {
        if (suspicionIcon != null) suspicionIcon.SetActive(state == DetectionState.Suspicious);
        if (alertIcon != null)     alertIcon.SetActive(state == DetectionState.Alerted);
    }

    /// <summary>Force this detector to fully alert state (e.g. hit by player, heard noise).</summary>
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

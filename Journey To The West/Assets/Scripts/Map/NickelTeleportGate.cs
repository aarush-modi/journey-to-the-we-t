using UnityEngine;

[RequireComponent(typeof(MapTransitions))]
public class NickelTeleportGate : MonoBehaviour
{
    [SerializeField] private NickelNoumanNPC nickel;
    [SerializeField] private MapTransitions transition;

    private void Awake()
    {
        if (transition == null)
            transition = GetComponent<MapTransitions>();

        SyncTransitionState();
    }

    private void Update()
    {
        SyncTransitionState();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;
        if (nickel == null || nickel.HasSolvedRiddle) return;

        if (transition != null)
            transition.enabled = false;

        nickel.Interact(collision.gameObject);
    }

    private void SyncTransitionState()
    {
        if (transition == null) return;

        bool canTeleport = nickel != null && nickel.HasSolvedRiddle;
        if (transition.enabled != canTeleport)
            transition.enabled = canTeleport;
    }
}

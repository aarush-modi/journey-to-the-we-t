using UnityEngine;

public class ObjectiveTracker : MonoBehaviour
{
    public bool IsComplete { get; private set; }

    public void SetComplete()
    {
        IsComplete = true;
    }
}

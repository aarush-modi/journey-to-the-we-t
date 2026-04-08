using UnityEngine;

public class PersistentObject : MonoBehaviour
{
    // Each GameObject gets its own instance tracked by name
    private static readonly System.Collections.Generic.Dictionary<string, PersistentObject> instances
        = new System.Collections.Generic.Dictionary<string, PersistentObject>();

    private void Awake()
    {
        string key = gameObject.name;

        if (instances.TryGetValue(key, out PersistentObject existing) && existing != null && existing != this)
        {
            // A persistent copy already exists
            Destroy(gameObject);
            return;
        }

        instances[key] = this;
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
    }
}
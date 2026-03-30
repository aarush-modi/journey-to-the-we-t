using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SceneTeleporter : MonoBehaviour
{
#if UNITY_EDITOR
    public SceneAsset targetSceneAsset;
#endif

    public string targetScene;

    void OnValidate()
    {
#if UNITY_EDITOR
        if (targetSceneAsset != null)
            targetScene = targetSceneAsset.name;
#endif
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            SceneManager.LoadScene(targetScene);
        }
    }
}
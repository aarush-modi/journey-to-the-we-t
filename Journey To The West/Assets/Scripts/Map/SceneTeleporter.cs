using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SceneTeleporter : MonoBehaviour
{
#if UNITY_EDITOR
    public SceneAsset targetSceneAsset;
#endif

    public string targetScene;
    private bool isTransitioning = false;

    void OnValidate()
    {
#if UNITY_EDITOR
        if (targetSceneAsset != null)
            targetScene = targetSceneAsset.name;
#endif
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isTransitioning)
        {
            isTransitioning = true;

            PlayerController pc = other.GetComponent<PlayerController>();
            if (pc != null)
                pc.SetMovementLocked(true);

            _ = TransitionToScene();
        }
    }

    async Task TransitionToScene()
    {
        await ScreenFader.Instance.FadeOut();
        SceneManager.LoadScene(targetScene);
    }
}
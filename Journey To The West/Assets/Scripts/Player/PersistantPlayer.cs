using UnityEngine;
using UnityEngine.SceneManagement;

public class PersistentPlayer : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    async void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        GameObject spawn = GameObject.FindWithTag("SpawnPoint");
        if (spawn != null)
            transform.position = spawn.transform.position;

        // Wait a frame to make sure ScreenFader.Instance is ready
        await System.Threading.Tasks.Task.Yield();

        if (ScreenFader.Instance != null)
            await ScreenFader.Instance.FadeIn();
    }
}
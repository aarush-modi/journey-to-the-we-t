using UnityEngine;
using UnityEngine.SceneManagement;

public class PersistentPlayer : MonoBehaviour
{
    private const string MerchantTownSceneName = "MerchantTown";
    private const int MerchantTownMaxGreedGold = 600;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    async void Start()
    {
        await System.Threading.Tasks.Task.Yield();
        ApplySceneStartupStats(SceneManager.GetActiveScene());
    }

    async void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        GameObject spawn = GameObject.FindWithTag("SpawnPoint");
        if (spawn != null)
            transform.position = spawn.transform.position;

        // Wait a frame to make sure ScreenFader.Instance is ready
        await System.Threading.Tasks.Task.Yield();

        ApplySceneStartupStats(scene);

        if (ScreenFader.Instance != null)
            await ScreenFader.Instance.FadeIn();
    }

    private void ApplySceneStartupStats(Scene scene)
    {
        if (scene.name != MerchantTownSceneName)
        {
            return;
        }

        GreedMeter greedMeter = GetComponent<GreedMeter>();
        if (greedMeter != null)
        {
            greedMeter.AddGold(MerchantTownMaxGreedGold - greedMeter.GetCurrentGold());
        }

        HustleStyleManager.Instance?.RefreshStyleEffects();

        PlayerCombat playerCombat = GetComponent<PlayerCombat>();
        if (playerCombat != null)
        {
            playerCombat.Heal(playerCombat.GetMaxHP());
        }
    }
}

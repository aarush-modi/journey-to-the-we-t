using UnityEngine;
using UnityEngine.SceneManagement;

public class PersistentPlayer : MonoBehaviour
{
    private const string MerchantTownSceneName = "MerchantTown";
    private const int MerchantTownMaxGreedGold = 1200;

    public static string PendingSpawnId = "default";

    private static PersistentPlayer instance;
    private static bool hasSpawned = false;
    private PlayerController playerController;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        playerController = GetComponent<PlayerController>();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    async void Start()
    {
        await System.Threading.Tasks.Task.Yield();
        ApplySceneStartupStats(SceneManager.GetActiveScene());
    }

    async void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (playerController != null)
        {
            playerController.SetMovementLocked(true);
            playerController.ResetInput();
        }

        UnityEngine.InputSystem.PlayerInput playerInput = GetComponent<UnityEngine.InputSystem.PlayerInput>();
        if (playerInput != null)
        {
            playerInput.enabled = false;
            playerInput.enabled = true;
        }

        if (hasSpawned)
        {
            SpawnPoint spawnPoint = FindSpawnById(PendingSpawnId);
            if (spawnPoint != null)
                playerController.Respawn(spawnPoint.transform.position);
            else
                Debug.LogWarning($"No SpawnPoint with id '{PendingSpawnId}' found in scene: {scene.name}");
        }

        hasSpawned = true;

        await System.Threading.Tasks.Task.Yield();

        ApplySceneStartupStats(scene);

        if (ScreenFader.Instance != null)
            await ScreenFader.Instance.FadeIn();

        if (playerController != null)
            playerController.SetMovementLocked(false);
    }

    private SpawnPoint FindSpawnById(string id)
    {
        foreach (SpawnPoint sp in FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None))
        {
            if (sp.spawnId == id)
                return sp;
        }
        return null;
    }

    private void ApplySceneStartupStats(Scene scene)
    {
        if (scene.name != MerchantTownSceneName)
            return;

        GreedMeter greedMeter = GetComponent<GreedMeter>();
        if (greedMeter != null)
            greedMeter.AddGold(MerchantTownMaxGreedGold - greedMeter.GetCurrentGold());

        HustleStyleManager.Instance?.RefreshStyleEffects();

        PlayerCombat playerCombat = GetComponent<PlayerCombat>();
        if (playerCombat != null)
            playerCombat.Heal(playerCombat.GetMaxHP());
    }
}
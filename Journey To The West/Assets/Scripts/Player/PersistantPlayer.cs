using UnityEngine;
using UnityEngine.SceneManagement;

public class PersistentPlayer : MonoBehaviour
{
    private const string MerchantTownSceneName = "MerchantTown";
    private const int MerchantTownMaxGreedGold = 600;

    private static PersistentPlayer instance;
    private static bool hasSpawned = false; // tracks if we've teleported at least once
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

        // Only move to SpawnPoint if we teleported, not on first game load
        if (hasSpawned)
        {
            GameObject spawn = GameObject.FindWithTag("SpawnPoint");
            if (spawn != null)
                playerController.Respawn(spawn.transform.position);
            else
                Debug.LogWarning("No SpawnPoint found in scene: " + scene.name);
        }

        hasSpawned = true; // any subsequent scene load is a teleport

        await System.Threading.Tasks.Task.Yield();

        ApplySceneStartupStats(scene);

        if (ScreenFader.Instance != null)
            await ScreenFader.Instance.FadeIn();

        if (playerController != null)
            playerController.SetMovementLocked(false);
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
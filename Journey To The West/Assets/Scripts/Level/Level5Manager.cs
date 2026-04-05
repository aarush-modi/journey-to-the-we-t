using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Level 5 only. Watches for player death and reloads the entire scene instead
/// of letting the normal checkpoint respawn happen. No shared scripts are modified.
/// Drop this on an empty GameObject in the Level 5 scene only.
/// </summary>
public class Level5Manager : MonoBehaviour
{
    [Header("Death Screen (optional)")]
    [SerializeField] private CanvasGroup deathOverlay;
    [SerializeField] private float fadeInDuration = 1f;
    [SerializeField] private float holdDuration = 1.5f;

    private PlayerCombat playerCombat;
    private bool handlingDeath;

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerCombat = player.GetComponent<PlayerCombat>();

        if (deathOverlay != null) deathOverlay.alpha = 0f;
    }

    private void Update()
    {
        if (handlingDeath || playerCombat == null) return;

        // IsDead() flips to true the moment PlayerCombat.Die() is called.
        // We intercept here before RespawnAfterDelay can teleport the player.
        if (playerCombat.IsDead())
        {
            handlingDeath = true;
            StartCoroutine(RestartLevel());
        }
    }

    private IEnumerator RestartLevel()
    {
        if (deathOverlay != null)
        {
            float elapsed = 0f;
            while (elapsed < fadeInDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                deathOverlay.alpha = Mathf.Clamp01(elapsed / fadeInDuration);
                yield return null;
            }
            yield return new WaitForSecondsRealtime(holdDuration);
        }
        else
        {
            yield return new WaitForSecondsRealtime(1f);
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}

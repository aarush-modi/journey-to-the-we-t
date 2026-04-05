using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Cinemachine;

public class PersistentCamera : MonoBehaviour
{
    private CinemachineCamera cinemachineCamera;
    private CinemachineConfiner2D confiner;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        cinemachineCamera = GetComponent<CinemachineCamera>();
        confiner = GetComponent<CinemachineConfiner2D>();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Make sure the main camera has a CinemachineBrain
        Camera mainCam = Camera.main;
        if (mainCam != null && mainCam.GetComponent<CinemachineBrain>() == null)
        {
            mainCam.gameObject.AddComponent<CinemachineBrain>();
        }

        // Reassign the player as the tracking target
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            cinemachineCamera.Target.TrackingTarget = player.transform;
            RefreshConfinerForPlayer(scene, player.transform.position);
            cinemachineCamera.ForceCameraPosition(
                player.transform.position + new Vector3(0f, 0f, -10f),
                Quaternion.identity
            );
        }
    }

    private void RefreshConfinerForPlayer(Scene scene, Vector3 playerPosition)
    {
        if (confiner == null)
        {
            return;
        }

        PolygonCollider2D fallbackBoundary = null;
        foreach (GameObject rootObject in scene.GetRootGameObjects())
        {
            if (rootObject.name != "MapBounds")
            {
                continue;
            }

            foreach (PolygonCollider2D boundary in rootObject.GetComponentsInChildren<PolygonCollider2D>(true))
            {
                if (boundary == null || !boundary.enabled)
                {
                    continue;
                }

                if (fallbackBoundary == null)
                {
                    fallbackBoundary = boundary;
                }

                if (boundary.OverlapPoint(playerPosition))
                {
                    confiner.BoundingShape2D = boundary;
                    confiner.InvalidateBoundingShapeCache();
                    return;
                }
            }
        }

        if (fallbackBoundary != null)
        {
            confiner.BoundingShape2D = fallbackBoundary;
            confiner.InvalidateBoundingShapeCache();
        }
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Cinemachine;

public class PersistentCamera : MonoBehaviour
{
    private CinemachineCamera cinemachineCamera;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        cinemachineCamera = GetComponent<CinemachineCamera>();
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
        }
    }
}
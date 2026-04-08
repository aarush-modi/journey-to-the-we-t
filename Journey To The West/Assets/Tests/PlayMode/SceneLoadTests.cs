using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;

// NOTE: All scenes tested here must be added to the Build Settings scene list
// (File > Build Settings > Scenes In Build) for SceneManager.LoadScene() to work.
// If a scene is missing, the test will fail with a "Scene not found" error.

[TestFixture]
public class SceneLoadTests
{
    [UnityTest]
    public IEnumerator MainScene_LoadsWithoutErrors()
    {
        SceneManager.LoadScene("Main");
        yield return null; // wait one frame for scene load
        yield return null; // wait another frame for Start() calls
        LogAssert.NoUnexpectedReceived();
    }

    [UnityTest]
    public IEnumerator SnowyTown_LoadsWithoutErrors()
    {
        SceneManager.LoadScene("SnowyTown");
        yield return null;
        yield return null;
        LogAssert.NoUnexpectedReceived();
    }

    [UnityTest]
    public IEnumerator FishingPort_LoadsWithoutErrors()
    {
        SceneManager.LoadScene("FishingPort");
        yield return null;
        yield return null;
        LogAssert.NoUnexpectedReceived();
    }

    [UnityTest]
    public IEnumerator MerchantTown_LoadsWithoutErrors()
    {
        SceneManager.LoadScene("MerchantTown");
        yield return null;
        yield return null;
        LogAssert.NoUnexpectedReceived();
    }

    [UnityTest]
    public IEnumerator MainScene_PlayerExists()
    {
        SceneManager.LoadScene("Main");
        yield return null;
        yield return null;
        var player = GameObject.FindWithTag("Player");
        Assert.IsNotNull(player, "Player GameObject should exist in Main scene");
    }

    [UnityTest]
    public IEnumerator MainScene_QuestManagerInitialized()
    {
        SceneManager.LoadScene("Main");
        yield return null;
        yield return null;
        Assert.IsNotNull(QuestManager.Instance, "QuestManager singleton should be initialized");
    }
}

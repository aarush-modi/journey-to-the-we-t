using UnityEngine;

/// <summary>
/// Global pause via Time.timeScale. Supports nested SetPause(true) calls (e.g. menu + dialogue).
/// </summary>
public static class PauseController
{
    static int _pauseDepth;
    static float _savedTimeScale = 1f;

    public static bool IsGamePaused => _pauseDepth > 0;

    public static void SetPause(bool paused)
    {
        if (paused)
        {
            if (_pauseDepth == 0)
            {
                _savedTimeScale = Time.timeScale;
                Time.timeScale = 0f;
            }

            _pauseDepth++;
        }
        else
        {
            if (_pauseDepth == 0)
                return;

            _pauseDepth--;
            if (_pauseDepth == 0)
                Time.timeScale = _savedTimeScale > 0f ? _savedTimeScale : 1f;
        }
    }
}

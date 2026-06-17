using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    private const int MIN_WINDOW_WIDTH = 800;
    private const int MIN_WINDOW_HEIGHT = 600;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        LoadAllSettings();
    }

    public void LoadAllSettings()
    {
        ApplyFullscreen(PlayerPrefs.GetInt("Fullscreen", Screen.fullScreen ? 1 : 0) == 1);
        ApplyLimitFPS(PlayerPrefs.GetInt("LimitFPS", 0) == 1);
        ApplyShowFPS(PlayerPrefs.GetInt("ShowFPS", 0) == 1);
        ApplyMuteMusic(PlayerPrefs.GetInt("MuteMusic", 0) == 1);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        ApplyFullscreen(isFullscreen);
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void SetLimitFPS(bool limitFPS)
    {
        ApplyLimitFPS(limitFPS);
        PlayerPrefs.SetInt("LimitFPS", limitFPS ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void SetShowFPS(bool showFPS)
    {
        ApplyShowFPS(showFPS);
        PlayerPrefs.SetInt("ShowFPS", showFPS ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void SetMuteMusic(bool muteMusic)
    {
        ApplyMuteMusic(muteMusic);
        PlayerPrefs.SetInt("MuteMusic", muteMusic ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void ApplyFullscreen(bool isFullscreen)
    {
        if (isFullscreen)
        {
            Resolution currentRes = Screen.currentResolution;
            Screen.SetResolution(currentRes.width, currentRes.height, FullScreenMode.FullScreenWindow);
        }
        else
        {
            int width = Mathf.Max(1280, MIN_WINDOW_WIDTH);
            int height = Mathf.Max(800, MIN_WINDOW_HEIGHT);
            Screen.SetResolution(width, height, FullScreenMode.Windowed);
        }
    }

    private void ApplyLimitFPS(bool limitFPS)
    {
        if (limitFPS)
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;
        }
        else
        {
            QualitySettings.vSyncCount = 1;
            Application.targetFrameRate = -1;
        }
    }

    private void ApplyShowFPS(bool showFPS)
    {
        if (FPSCounter.Instance != null)
        {
            FPSCounter.Instance.SetShowFPS(showFPS);
        }
    }

    private void ApplyMuteMusic(bool muteMusic)
    {
        AudioListener.volume = muteMusic ? 0f : 1f;
    }
}
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections.Generic;

// Gestor del menu principal
// Maneja logica de partidas guardadas, logros y configuracion
// NOTA: Toda la UI ha sido delegada a UIMenuManager
public class MenuManager : MonoBehaviour
{
    public string CharacterCreationSceneName = "CharacterCreation";

    private const int MIN_WINDOW_WIDTH = 800;
    private const int MIN_WINDOW_HEIGHT = 600;

    private int m_DeleteConfirmationCount = 0;

    void Start()
    {
        LoadSettings();
    }

    // Refresca la lista de logros ordenada por porcentaje
    public void RefreshAchievementsList()
    {
        if (UIMenuManager.Instance != null)
        {
            UIMenuManager.Instance.RefreshAchievementsList();
        }
    }

    // Carga la configuracion guardada
    void LoadSettings()
    {
        bool isFullscreen = PlayerPrefs.GetInt("Fullscreen", Screen.fullScreen ? 1 : 0) == 1;

        if (isFullscreen)
        {
            Resolution currentRes = Screen.currentResolution;
            Screen.SetResolution(currentRes.width, currentRes.height, FullScreenMode.FullScreenWindow);
        }

        bool limitFPS = PlayerPrefs.GetInt("LimitFPS", 0) == 1;

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

        bool showFPS = PlayerPrefs.GetInt("ShowFPS", 0) == 1;

        bool muteMusic = PlayerPrefs.GetInt("MuteMusic", 0) == 1;
        AudioListener.volume = muteMusic ? 0f : 1f;
    }

    // Refresca la lista de partidas guardadas
    public void RefreshSaveList()
    {
        if (UIMenuManager.Instance != null)
        {
            UIMenuManager.Instance.RefreshSaveList();
        }
    }

    // Carga una partida guardada
    public void OnSaveFileSelected(string filePath)
    {
        string fileName = Path.GetFileName(filePath);
        PlayerData loadedData = SaveManager.LoadPlayerData(fileName);

        if (loadedData != null && SessionManager.Instance != null)
        {
            SessionManager.Instance.CurrentPlayerData = loadedData;
            SceneManager.LoadScene("GameScene");
        }
        else
        {
            Debug.LogError("Error al cargar el archivo: " + filePath);
        }
    }

    // Handlers de botones del menu
    public void OnNewGameClicked()
    {
        SceneManager.LoadScene(CharacterCreationSceneName);
    }

    public void OnLoadGameClicked()
    {
        if (UIMenuManager.Instance != null)
        {
            UIMenuManager.Instance.OnLoadGameClicked();
        }
    }

    public void OnAchievementsClicked()
    {
        RefreshAchievementsList();
    }

    public void OnSettingsClicked()
    {
        if (UIMenuManager.Instance != null)
        {
            UIMenuManager.Instance.OnSettingsClicked();
        }
    }

    public void OnExitClicked()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // Cambia entre pantalla completa y ventana
    public void OnFullscreenChanged(bool isFullscreen)
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

        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }

    // Limita o no los FPS a 60
    public void OnLimitFPSChanged(bool limitFPS)
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

        PlayerPrefs.SetInt("LimitFPS", limitFPS ? 1 : 0);
        PlayerPrefs.Save();
    }

    // Muestra u oculta el contador de FPS
    public void OnShowFPSChanged(bool showFPS)
    {
        if (FPSCounter.Instance != null)
        {
            FPSCounter.Instance.SetShowFPS(showFPS);
        }
    }

    // Silencia o activa la musica
    public void OnMuteMusicChanged(bool muteMusic)
    {
        AudioListener.volume = muteMusic ? 0f : 1f;
        PlayerPrefs.SetInt("MuteMusic", muteMusic ? 1 : 0);
        PlayerPrefs.Save();
    }

    // Maneja el borrado de todas las partidas con confirmacion
    public void OnDeleteAllSavesClicked()
    {
        m_DeleteConfirmationCount++;

        if (m_DeleteConfirmationCount == 1)
        {
            // El botón debe mostrar el mensaje de confirmación
            // Esto se maneja en UIMenuManager
        }
        else if (m_DeleteConfirmationCount >= 2)
        {
            DeleteAllSaves();
            m_DeleteConfirmationCount = 0;
        }
    }

    // Borra todos los archivos de guardado
    void DeleteAllSaves()
    {
        string[] saveFiles = SaveManager.GetAllSaveFiles();

        foreach (string filePath in saveFiles)
        {
            try
            {
                File.Delete(filePath);
                Debug.Log($"Archivo borrado: {filePath}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error al borrar {filePath}: {e.Message}");
            }
        }
    }

    // Resetea el contador de confirmacion
    public void ResetDeleteConfirmation()
    {
        m_DeleteConfirmationCount = 0;
    }
}
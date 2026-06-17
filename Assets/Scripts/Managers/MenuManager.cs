using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance { get; private set; }
    public string CharacterCreationSceneName = "CharacterCreation";

    private int m_DeleteConfirmationCount = 0;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
    }

    public void RefreshAchievementsList()
    {
        if (UIMenuManager.Instance != null)
        {
            UIMenuManager.Instance.RefreshAchievementsList();
        }
    }

    public void RefreshSaveList()
    {
        if (UIMenuManager.Instance != null)
        {
            UIMenuManager.Instance.RefreshSaveFilesList();
        }
    }

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
        if (UIMenuManager.Instance != null)
        {
            UIMenuManager.Instance.ShowAchievementsPanel();
        }
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

    public void OnDeleteAllSavesClicked()
    {
        m_DeleteConfirmationCount++;
        if (m_DeleteConfirmationCount == 1)
        {
        }
        else if (m_DeleteConfirmationCount >= 2)
        {
            DeleteAllSaves();
            m_DeleteConfirmationCount = 0;
        }
    }

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

    public void ResetDeleteConfirmation()
    {
        m_DeleteConfirmationCount = 0;
        if (UIMenuManager.Instance != null)
            UIMenuManager.Instance.UpdateDeleteAllSavesButtonText("Borrar Todas las Partidas");
    }

    public int GetDeleteConfirmationCount()
    {
        return m_DeleteConfirmationCount;
    }
}
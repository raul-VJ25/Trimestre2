using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

// Gestor del menu principal
// Maneja logica de partidas guardadas, logros y borrado
// NOTA: Toda la UI y configuracion han sido delegadas a UIMenuManager y SettingsManager
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
        // La carga de configuracion inicial ahora la hace SettingsManager
    }

    // Refresca la lista de logros ordenada por porcentaje
    public void RefreshAchievementsList()
    {
        if (UIMenuManager.Instance != null)
        {
            UIMenuManager.Instance.RefreshAchievementsList();
        }
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
using UnityEngine;
using UnityEngine.SceneManagement;

// Sistema de gestión de niveles (noches)
public class LevelSystem : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private int m_LevelUpInterval = 5;

    [Header("Referencias")]
    [SerializeField] private BoardManager m_BoardManager;
    [SerializeField] private PlayerController m_PlayerController;
    [SerializeField] private LifeSystem m_LifeSystem;
    [SerializeField] private XPSystem m_XPSystem;

    private int m_CurrentLevel = 1;
    public event System.Action<int> OnLevelChanged;
    public int CurrentLevel => m_CurrentLevel;

    public void Init(int startingLevel)
    {
        m_CurrentLevel = startingLevel;
        RaiseLevelChanged();
    }

    // Avanza al siguiente nivel y regenera el mapa si no es pantalla de mejora
    public bool AdvanceLevel()
    {
        m_CurrentLevel++;
        if (SessionManager.Instance != null)
            SessionManager.Instance.CurrentNight = m_CurrentLevel;

        RaiseLevelChanged();

        // Cada X niveles va a pantalla de mejora
        if (m_CurrentLevel % m_LevelUpInterval == 0 && m_CurrentLevel > 0)
        {
            HandleLevelUpScreen();
            return false;
        }

        // Si no es pantalla de mejora, regeneramos el tablero inmediatamente
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RefreshLevel();
        }

        return true;
    }

    private void HandleLevelUpScreen()
    {
        if (SessionManager.Instance != null)
        {
            SessionManager.Instance.IsLevelUp = true;
            SessionManager.Instance.CurrentNight = m_CurrentLevel;
            if (m_XPSystem != null) SessionManager.Instance.CurrentXP = m_XPSystem.CurrentXP;
            if (Random.value < 0.5f) SessionManager.Instance.BoardWidth++;
            else SessionManager.Instance.BoardHeight++;
            SessionManager.Instance.EnemyHealthBonus++;
            if (SessionManager.Instance.CurrentPlayerData != null && m_LifeSystem != null)
                SessionManager.Instance.CurrentPlayerData.SavedLife = m_LifeSystem.CurrentLife;

            // GUARDAR LOGROS ACTUALES ANTES DE IR AL MENÚ DE MEJORA
            if (SessionManager.Instance.CurrentPlayerData != null && AchievementManager.Instance != null)
                SessionManager.Instance.CurrentPlayerData.Achievements = AchievementManager.Instance.Logros;
        }
        Time.timeScale = 1f;
        SceneManager.LoadScene("CharacterCreation");
    }

    private void RaiseLevelChanged() => OnLevelChanged?.Invoke(m_CurrentLevel);
}
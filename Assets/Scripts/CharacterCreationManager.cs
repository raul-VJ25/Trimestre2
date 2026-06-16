using UnityEngine;
using UnityEngine.SceneManagement;

// Gestor de creacion y mejora de personajes
// Permite asignar puntos de estadisticas al inicio o al subir de nivel
// NOTA: Toda la UI ha sido delegada a UICharacterCreationManager
public class CharacterCreationManager : MonoBehaviour
{
    // Constantes del sistema
    private const int MAX_INITIAL_POINTS = 7;
    private const int MIN_STAT_VALUE = 1;
    private const int XP_PER_POINT = 100;

    // Estados del juego
    private bool m_IsRetrying = false;
    private bool m_IsLevelUp = false;
    private string m_RetryName = "";
    private int m_AvailablePoints = MAX_INITIAL_POINTS;
    private int m_CurrentXP = 0;

    // Estadisticas actuales
    private int m_Strength = MIN_STAT_VALUE;
    private int m_Agility = MIN_STAT_VALUE;
    private int m_Intelligence = MIN_STAT_VALUE;
    private int m_Health = MIN_STAT_VALUE;
    private string m_PlayerName = "";

    // Inicializacion y configuracion segun el modo de juego
    private void Start()
    {
        // Determina si es reintento o level up
        if (SessionManager.Instance != null)
        {
            m_IsRetrying = SessionManager.Instance.IsRetrying;
            m_IsLevelUp = SessionManager.Instance.IsLevelUp;

            if ((m_IsRetrying || m_IsLevelUp) && SessionManager.Instance.CurrentPlayerData != null)
            {
                m_RetryName = SessionManager.Instance.CurrentPlayerData.Name;
            }

            if (m_IsLevelUp)
            {
                m_CurrentXP = SessionManager.Instance.CurrentXP;
            }
        }

        // Configura la pantalla segun el modo
        if (m_IsLevelUp)
        {
            SetupLevelUpMode();
        }
        else if (m_IsRetrying)
        {
            SetupRetryMode();
        }
        else
        {
            SetupNewGameMode();
        }

        // Inicializar UI
        if (UICharacterCreationManager.Instance != null)
        {
            UICharacterCreationManager.Instance.InitializeUI(
                m_IsRetrying, m_IsLevelUp, m_RetryName,
                m_AvailablePoints, m_CurrentXP
            );
        }

        RefreshUI();
    }

    // Configura la pantalla para modo mejora de nivel
    private void SetupLevelUpMode()
    {
        var data = SessionManager.Instance.CurrentPlayerData;

        m_Strength = data.Strength;
        m_Agility = data.Agility;
        m_Intelligence = data.Intelligence;
        m_Health = data.Health;

        m_AvailablePoints = SessionManager.Instance.AvailableSkillPoints;
    }

    // Configura la pantalla para modo reintento
    private void SetupRetryMode()
    {
        m_Strength = MIN_STAT_VALUE;
        m_Agility = MIN_STAT_VALUE;
        m_Intelligence = MIN_STAT_VALUE;
        m_Health = MIN_STAT_VALUE;

        m_AvailablePoints = MAX_INITIAL_POINTS;
    }

    // Configura la pantalla para nueva partida
    private void SetupNewGameMode()
    {
        m_Strength = MIN_STAT_VALUE;
        m_Agility = MIN_STAT_VALUE;
        m_Intelligence = MIN_STAT_VALUE;
        m_Health = MIN_STAT_VALUE;

        m_AvailablePoints = MAX_INITIAL_POINTS;
    }

    // Modifica el valor de una estadistica
    public void ModifyStat(string statName, int amount)
    {
        int currentValue = 0;

        switch (statName)
        {
            case "Strength": currentValue = m_Strength; break;
            case "Agility": currentValue = m_Agility; break;
            case "Intelligence": currentValue = m_Intelligence; break;
            case "Health": currentValue = m_Health; break;
        }

        if (amount > 0)
        {
            int basePoints = m_IsLevelUp ?
                (SessionManager.Instance.CurrentPlayerData.Strength +
                 SessionManager.Instance.CurrentPlayerData.Agility +
                 SessionManager.Instance.CurrentPlayerData.Intelligence +
                 SessionManager.Instance.CurrentPlayerData.Health) :
                (MIN_STAT_VALUE * 4);

            int currentTotal = m_Strength + m_Agility + m_Intelligence + m_Health;
            int pointsSpent = currentTotal - basePoints;

            if (pointsSpent >= m_AvailablePoints) return;
        }
        else
        {
            if (m_IsLevelUp)
            {
                var data = SessionManager.Instance.CurrentPlayerData;
                int minValue = statName == "Strength" ? data.Strength :
                              statName == "Agility" ? data.Agility :
                              statName == "Intelligence" ? data.Intelligence :
                              data.Health;

                if (currentValue <= minValue) return;
            }
            else
            {
                if (currentValue <= MIN_STAT_VALUE) return;
            }
        }

        // Aplicar cambio
        switch (statName)
        {
            case "Strength": m_Strength += amount; break;
            case "Agility": m_Agility += amount; break;
            case "Intelligence": m_Intelligence += amount; break;
            case "Health": m_Health += amount; break;
        }

        RefreshUI();
    }

    // Actualiza todos los elementos de la UI
    private void RefreshUI()
    {
        int basePoints;

        if (m_IsLevelUp)
        {
            var data = SessionManager.Instance.CurrentPlayerData;
            basePoints = data.Strength + data.Agility + data.Intelligence + data.Health;
        }
        else
        {
            basePoints = MIN_STAT_VALUE * 4;
        }

        int currentTotal = m_Strength + m_Agility + m_Intelligence + m_Health;
        int pointsSpent = currentTotal - basePoints;
        int pointsLeft = m_AvailablePoints - pointsSpent;

        // Actualizar UI
        if (UICharacterCreationManager.Instance != null)
        {
            UICharacterCreationManager.Instance.UpdateStatsUI(
                m_Strength, m_Agility, m_Intelligence, m_Health,
                pointsLeft
            );
        }

        ValidatePlayButton(pointsLeft);
    }

    // Valida el boton de jugar
    private void ValidatePlayButton(int pointsLeft)
    {
        bool canPlay = true;

        if (!m_IsRetrying && !m_IsLevelUp)
        {
            canPlay = !string.IsNullOrWhiteSpace(m_PlayerName);
        }

        if (UICharacterCreationManager.Instance != null)
        {
            UICharacterCreationManager.Instance.SetPlayButtonEnabled(canPlay);
        }
    }

    // Actualiza el nombre del jugador
    public void SetPlayerName(string name)
    {
        m_PlayerName = name;
        ValidatePlayButton(m_AvailablePoints);
    }

    // Intercambia XP por puntos de habilidad
    public void ExchangeXPForPoint()
    {
        if (m_CurrentXP >= XP_PER_POINT)
        {
            m_CurrentXP -= XP_PER_POINT;
            m_AvailablePoints++;

            if (SessionManager.Instance != null)
            {
                SessionManager.Instance.CurrentXP = m_CurrentXP;
                SessionManager.Instance.AvailableSkillPoints = m_AvailablePoints;
            }

            RefreshUI();
        }
    }

    // Maneja el click del boton Jugar/Continuar
    public void OnPlayButtonClicked()
    {
        string playerName;

        if (m_IsRetrying || m_IsLevelUp)
        {
            playerName = m_RetryName;
        }
        else
        {
            playerName = m_PlayerName;
        }

        PlayerData newData = new PlayerData(
            playerName,
            m_Strength,
            m_Agility,
            m_Intelligence,
            m_Health
        );

        if (SessionManager.Instance != null)
        {
            var oldData = SessionManager.Instance.CurrentPlayerData;

            if (oldData != null && (m_IsRetrying || m_IsLevelUp))
            {
                newData.BestLevel = oldData.BestLevel;
                newData.BestXP = oldData.BestXP;
                newData.Achievements = oldData.Achievements;

                if (m_IsLevelUp)
                {
                    newData.SavedLife = oldData.SavedLife;
                    newData.SavedNight = SessionManager.Instance.CurrentNight;

                    int basePoints = oldData.Strength + oldData.Agility +
                                  oldData.Intelligence + oldData.Health;
                    int newTotal = m_Strength + m_Agility +
                                  m_Intelligence + m_Health;
                    int pointsUsed = newTotal - basePoints;

                    SessionManager.Instance.AvailableSkillPoints -= pointsUsed;
                    SessionManager.Instance.CurrentXP = m_CurrentXP;
                }
            }

            SessionManager.Instance.CurrentPlayerData = newData;
            SessionManager.Instance.IsRetrying = false;
        }

        SceneManager.LoadScene("GameScene");
    }

    // Vuelve al menu principal
    public void OnBackButtonClicked()
    {
        SceneManager.LoadScene("Menu");
    }

    // Obtiene el valor actual de una estadistica
    public int GetStatValue(string statName)
    {
        switch (statName)
        {
            case "Strength": return m_Strength;
            case "Agility": return m_Agility;
            case "Intelligence": return m_Intelligence;
            case "Health": return m_Health;
            default: return MIN_STAT_VALUE;
        }
    }

    // Obtiene los puntos disponibles
    public int GetAvailablePoints()
    {
        return m_AvailablePoints;
    }

    // Obtiene el XP actual
    public int GetCurrentXP()
    {
        return m_CurrentXP;
    }
}
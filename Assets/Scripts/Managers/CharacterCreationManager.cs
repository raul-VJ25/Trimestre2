using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterCreationManager : MonoBehaviour
{
    public static CharacterCreationManager Instance { get; private set; }

    private const int MAX_INITIAL_POINTS = 7;
    private const int MIN_STAT_VALUE = 1;
    private const int XP_PER_POINT = 100;

    private bool m_IsRetrying = false;
    private bool m_IsLevelUp = false;
    private string m_RetryName = "";
    private int m_AvailablePoints = MAX_INITIAL_POINTS;
    private int m_CurrentXP = 0;

    private int m_Strength = MIN_STAT_VALUE;
    private int m_Agility = MIN_STAT_VALUE;
    private int m_Intelligence = MIN_STAT_VALUE;
    private int m_Health = MIN_STAT_VALUE;
    private string m_PlayerName = "";

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
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

        if (m_IsLevelUp) SetupLevelUpMode();
        else if (m_IsRetrying) SetupRetryMode();
        else SetupNewGameMode();

        RefreshUI();
    }

    private void SetupLevelUpMode()
    {
        var data = SessionManager.Instance.CurrentPlayerData;
        m_Strength = data.Strength;
        m_Agility = data.Agility;
        m_Intelligence = data.Intelligence;
        m_Health = data.Health;
        m_AvailablePoints = SessionManager.Instance.AvailableSkillPoints;
    }

    private void SetupRetryMode()
    {
        m_Strength = MIN_STAT_VALUE;
        m_Agility = MIN_STAT_VALUE;
        m_Intelligence = MIN_STAT_VALUE;
        m_Health = MIN_STAT_VALUE;
        m_AvailablePoints = MAX_INITIAL_POINTS;
    }

    private void SetupNewGameMode()
    {
        m_Strength = MIN_STAT_VALUE;
        m_Agility = MIN_STAT_VALUE;
        m_Intelligence = MIN_STAT_VALUE;
        m_Health = MIN_STAT_VALUE;
        m_AvailablePoints = MAX_INITIAL_POINTS;
    }

    public void ModifyStat(StatType stat, int amount)
    {
        int currentValue = GetStatValue(stat);

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
                int minValue = stat == StatType.Strength ? data.Strength :
                               stat == StatType.Agility ? data.Agility :
                               stat == StatType.Intelligence ? data.Intelligence : data.Health;
                if (currentValue <= minValue) return;
            }
            else
            {
                if (currentValue <= MIN_STAT_VALUE) return;
            }
        }

        switch (stat)
        {
            case StatType.Strength: m_Strength += amount; break;
            case StatType.Agility: m_Agility += amount; break;
            case StatType.Intelligence: m_Intelligence += amount; break;
            case StatType.Health: m_Health += amount; break;
        }
        RefreshUI();
    }

    public void SetPlayerName(string name)
    {
        m_PlayerName = name;
        RefreshUI();
    }

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

    public void OnPlayButtonClicked()
    {
        string playerName = (m_IsRetrying || m_IsLevelUp) ? m_RetryName : m_PlayerName;
        PlayerData newData = new PlayerData(playerName, m_Strength, m_Agility, m_Intelligence, m_Health);

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
                    int basePoints = oldData.Strength + oldData.Agility + oldData.Intelligence + oldData.Health;
                    int newTotal = m_Strength + m_Agility + m_Intelligence + m_Health;
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

    public void OnBackButtonClicked()
    {
        SceneManager.LoadScene("Menu");
    }

    public int GetStatValue(StatType stat)
    {
        switch (stat)
        {
            case StatType.Strength: return m_Strength;
            case StatType.Agility: return m_Agility;
            case StatType.Intelligence: return m_Intelligence;
            case StatType.Health: return m_Health;
            default: return MIN_STAT_VALUE;
        }
    }

    public int GetAvailablePoints() { return m_AvailablePoints; }
    public int GetCurrentXP() { return m_CurrentXP; }
    public bool IsLevelUp() { return m_IsLevelUp; }
    public bool IsRetrying() { return m_IsRetrying; }

    private void RefreshUI()
    {
        int basePoints = m_IsLevelUp ?
            (SessionManager.Instance.CurrentPlayerData.Strength + SessionManager.Instance.CurrentPlayerData.Agility +
             SessionManager.Instance.CurrentPlayerData.Intelligence + SessionManager.Instance.CurrentPlayerData.Health) :
            (MIN_STAT_VALUE * 4);

        int currentTotal = m_Strength + m_Agility + m_Intelligence + m_Health;
        int pointsLeft = m_AvailablePoints - (currentTotal - basePoints);

        bool canPlay = (m_IsRetrying || m_IsLevelUp) || !string.IsNullOrWhiteSpace(m_PlayerName);

        if (UICharacterCreationManager.Instance != null)
        {
            UICharacterCreationManager.Instance.UpdateStatsUI(m_Strength, m_Agility, m_Intelligence, m_Health, pointsLeft);
            UICharacterCreationManager.Instance.SetPlayButtonEnabled(canPlay);
            UICharacterCreationManager.Instance.UpdateExchangeButton(m_CurrentXP, XP_PER_POINT);

            PlayerData tempData = new PlayerData("Temp", m_Strength, m_Agility, m_Intelligence, m_Health);
            UICharacterCreationManager.Instance.UpdatePreviewLife(tempData.StartingLife);

            UICharacterCreationManager.Instance.UpdateButtonStates(m_IsLevelUp ? SessionManager.Instance.CurrentPlayerData : null,
                                                                   m_Strength, m_Agility, m_Intelligence, m_Health, pointsLeft);
        }
    }
}
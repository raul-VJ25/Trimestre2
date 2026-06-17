using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class UIGameManager : MonoBehaviour
{
    public static UIGameManager Instance { get; private set; }

    [Header("UIDocument")]
    public UIDocument UIDocument;

    [Header("HUD Central")]
    private Label m_LifeLabel;
    private Label m_AchievementLabel;

    [Header("HUD Izquierdo - Stats")]
    private Label m_StrengthLabel;
    private Label m_AgilityLabel;
    private Label m_IntelligenceLabel;
    private Label m_HealthLabel;
    private Label m_SkillPointsLabel;

    [Header("HUD Derecho - Progreso")]
    private Label m_NightLabel;
    private Label m_XPLabel;

    [Header("Paneles")]
    private VisualElement m_PauseOverlay;
    private VisualElement m_SettingsPanel;
    private VisualElement m_GameOverPanel;

    [Header("Game Over")]
    private Label m_GameOverDaysLabel;
    private Label m_GameOverXPLabel;
    private Label m_GameOverHeroLabel;
    private VisualElement m_TopPlayersContainer;

    [Header("Configuración Toggles")]
    private Toggle m_FullscreenToggle;
    private Toggle m_LimitFPSToggle;
    private Toggle m_ShowFPSToggle;
    private Toggle m_MuteMusicToggle;

    [Header("Botones")]
    private Button m_ContinueButton;
    private Button m_PauseSettingsButton;
    private Button m_PauseExitButton;
    private Button m_SettingsBackButton;
    private Button m_ExitToMenuButton;
    private Button m_RetryButton;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        if (UIDocument == null)
        {
            Debug.LogError("UIGameManager: No se ha asignado el UIDocument");
            return;
        }

        var root = UIDocument.rootVisualElement;

        m_LifeLabel = root.Q<Label>("FoodLabel");
        m_AchievementLabel = root.Q<Label>("AchievementLabel");

        m_StrengthLabel = root.Q<Label>("StrengthLabel");
        m_AgilityLabel = root.Q<Label>("AgilityLabel");
        m_IntelligenceLabel = root.Q<Label>("IntelligenceLabel");
        m_HealthLabel = root.Q<Label>("HealthLabel");
        m_SkillPointsLabel = root.Q<Label>("SkillPointsLabel");

        m_NightLabel = root.Q<Label>("NightLabel");
        m_XPLabel = root.Q<Label>("XPLabel");

        m_PauseOverlay = root.Q<VisualElement>("PauseOverlay");
        m_SettingsPanel = root.Q<VisualElement>("SettingsPanel");
        m_GameOverPanel = root.Q<VisualElement>("GameOverPanel");

        m_GameOverHeroLabel = root.Q<Label>("GameOverHeroLabel");
        m_GameOverDaysLabel = root.Q<Label>("GameOverDaysLabel");
        m_GameOverXPLabel = root.Q<Label>("GameOverXPLabel");
        m_TopPlayersContainer = root.Q<VisualElement>("TopPlayersContainer");

        m_FullscreenToggle = root.Q<Toggle>("FullscreenToggle");
        m_LimitFPSToggle = root.Q<Toggle>("LimitFPSToggle");
        m_ShowFPSToggle = root.Q<Toggle>("ShowFPSToggle");
        m_MuteMusicToggle = root.Q<Toggle>("MuteMusicToggle");

        m_ContinueButton = root.Q<Button>("ContinueButton");
        m_PauseSettingsButton = root.Q<Button>("SettingsButton");
        m_PauseExitButton = root.Q<Button>("ExitButton");
        m_SettingsBackButton = root.Q<Button>("SettingsBackButton");
        m_ExitToMenuButton = root.Q<Button>("ExitToMenuButton");
        m_RetryButton = root.Q<Button>("RetryButton");

        if (m_ContinueButton != null)
            m_ContinueButton.clicked += () => GameManager.Instance.ResumeGame();

        if (m_PauseSettingsButton != null)
            m_PauseSettingsButton.clicked += OpenSettingsFromPause;

        if (m_PauseExitButton != null)
            m_PauseExitButton.clicked += () => GameManager.Instance.SaveAndExit();

        if (m_SettingsBackButton != null)
            m_SettingsBackButton.clicked += CloseSettingsFromPause;

        if (m_ExitToMenuButton != null)
            m_ExitToMenuButton.clicked += () => GameManager.Instance.OnExitClicked();

        if (m_RetryButton != null)
            m_RetryButton.clicked += () => GameManager.Instance.OnRetryClicked();

        if (m_FullscreenToggle != null)
            m_FullscreenToggle.RegisterValueChangedCallback(evt => SettingsManager.Instance.SetFullscreen(evt.newValue));

        if (m_LimitFPSToggle != null)
            m_LimitFPSToggle.RegisterValueChangedCallback(evt => SettingsManager.Instance.SetLimitFPS(evt.newValue));

        if (m_ShowFPSToggle != null)
            m_ShowFPSToggle.RegisterValueChangedCallback(evt => SettingsManager.Instance.SetShowFPS(evt.newValue));

        if (m_MuteMusicToggle != null)
            m_MuteMusicToggle.RegisterValueChangedCallback(evt => SettingsManager.Instance.SetMuteMusic(evt.newValue));

        if (m_AchievementLabel != null)
            m_AchievementLabel.text = "";

        LoadSettingsValues();
    }

    public void UpdateSkillPointsUI()
    {
        if (m_SkillPointsLabel != null && SessionManager.Instance != null)
        {
            m_SkillPointsLabel.text = "Puntos: " + SessionManager.Instance.AvailableSkillPoints;
        }
    }

    public void UpdateHUD(string playerName, int life, int night, int xp)
    {
        if (SessionManager.Instance != null && SessionManager.Instance.CurrentPlayerData != null)
        {
            var data = SessionManager.Instance.CurrentPlayerData;
            if (m_StrengthLabel != null) m_StrengthLabel.text = "Fuerza: " + data.Strength;
            if (m_AgilityLabel != null) m_AgilityLabel.text = "Agilidad: " + data.Agility;
            if (m_IntelligenceLabel != null) m_IntelligenceLabel.text = "Intelig.: " + data.Intelligence;
            if (m_HealthLabel != null) m_HealthLabel.text = "Salud: " + data.Health;
        }

        if (m_LifeLabel != null) m_LifeLabel.text = "Vida " + playerName + ": " + life;
        if (m_NightLabel != null) m_NightLabel.text = "Noche: " + night;
        if (m_XPLabel != null) m_XPLabel.text = "XP: " + xp;

        UpdateSkillPointsUI();
    }

    public void ShowAchievementNotification(string message, bool isSkillPoint = false)
    {
        if (m_AchievementLabel != null)
        {
            if (isSkillPoint)
            {
                m_AchievementLabel.text = message;
                m_AchievementLabel.style.color = new StyleColor(new Color(0.5f, 0.7f, 1f));
            }
            else
            {
                m_AchievementLabel.text = "¡Logro!\n" + message;
                m_AchievementLabel.style.color = new StyleColor(new Color(1f, 0.84f, 0f));
            }
            StartCoroutine(HideAchievementLabel());
        }
    }

    public void ShowXPNotification(string message)
    {
        if (m_AchievementLabel != null)
        {
            m_AchievementLabel.text = message;
            m_AchievementLabel.style.color = new StyleColor(new Color(0.4f, 0.9f, 0.4f));
            StartCoroutine(HideAchievementLabel());
        }
    }

    public void ShowDodgeNotification(string message)
    {
        if (m_AchievementLabel != null)
        {
            m_AchievementLabel.text = message;
            m_AchievementLabel.style.color = new StyleColor(new Color(0.6f, 0.8f, 1f));
            StartCoroutine(HideAchievementLabel());
        }
    }

    private IEnumerator HideAchievementLabel()
    {
        yield return new WaitForSeconds(3f);
        if (m_AchievementLabel != null)
        {
            m_AchievementLabel.text = "";
        }
    }

    public void ShowGameOver(string playerName, int nights, int xp)
    {
        if (m_GameOverHeroLabel != null) m_GameOverHeroLabel.text = playerName;
        if (m_GameOverDaysLabel != null) m_GameOverDaysLabel.text = $"Noches sobrevividas: {nights}";
        if (m_GameOverXPLabel != null) m_GameOverXPLabel.text = $"XP conseguido: {xp}";

        LoadTop5Players();

        if (m_GameOverPanel != null) m_GameOverPanel.style.display = DisplayStyle.Flex;
    }

    void LoadTop5Players()
    {
        if (m_TopPlayersContainer == null) return;

        m_TopPlayersContainer.Clear();

        string[] files = SaveManager.GetAllSaveFiles();
        System.Collections.Generic.List<(string name, int level, int xp)> players =
            new System.Collections.Generic.List<(string, int, int)>();

        foreach (string filePath in files)
        {
            string fileName = System.IO.Path.GetFileName(filePath);
            PlayerData data = SaveManager.LoadPlayerData(fileName);
            if (data != null && data.BestLevel > 0)
            {
                players.Add((data.Name, data.BestLevel, data.BestXP));
            }
        }

        players.Sort((a, b) =>
        {
            int levelCompare = b.level.CompareTo(a.level);
            return levelCompare != 0 ? levelCompare : b.xp.CompareTo(a.xp);
        });

        int count = Mathf.Min(5, players.Count);

        if (count == 0)
        {
            var noDataLabel = new Label("No hay records todavía");
            noDataLabel.AddToClassList("top-player-empty");
            m_TopPlayersContainer.Add(noDataLabel);
            return;
        }

        for (int i = 0; i < count; i++)
        {
            var row = new VisualElement();
            row.AddToClassList("top-player-row");

            string medal = i switch
            {
                0 => "1.",
                1 => "2.",
                2 => "3.",
                _ => $"{i + 1}."
            };

            var posLabel = new Label(medal);
            posLabel.AddToClassList("top-player-pos");
            row.Add(posLabel);

            var nameLabel = new Label(players[i].name);
            nameLabel.AddToClassList("top-player-name");
            row.Add(nameLabel);

            var levelLabel = new Label($"{players[i].level} noches");
            levelLabel.AddToClassList("top-player-level");
            row.Add(levelLabel);

            var xpLabel = new Label($"{players[i].xp} XP");
            xpLabel.AddToClassList("top-player-xp");
            row.Add(xpLabel);

            m_TopPlayersContainer.Add(row);
        }
    }

    void OpenSettingsFromPause()
    {
        if (m_PauseOverlay != null) m_PauseOverlay.style.display = DisplayStyle.None;
        if (m_SettingsPanel != null) m_SettingsPanel.style.display = DisplayStyle.Flex;
        LoadSettingsValues();
    }

    void CloseSettingsFromPause()
    {
        if (m_SettingsPanel != null) m_SettingsPanel.style.display = DisplayStyle.None;
        if (m_PauseOverlay != null) m_PauseOverlay.style.display = DisplayStyle.Flex;
    }

    void LoadSettingsValues()
    {
        if (m_FullscreenToggle != null)
            m_FullscreenToggle.value = PlayerPrefs.GetInt("Fullscreen", Screen.fullScreen ? 1 : 0) == 1;

        if (m_LimitFPSToggle != null)
            m_LimitFPSToggle.value = PlayerPrefs.GetInt("LimitFPS", 0) == 1;

        if (m_ShowFPSToggle != null)
            m_ShowFPSToggle.value = PlayerPrefs.GetInt("ShowFPS", 0) == 1;

        if (m_MuteMusicToggle != null)
            m_MuteMusicToggle.value = PlayerPrefs.GetInt("MuteMusic", 0) == 1;
    }

    public void ShowPauseMenu()
    {
        if (m_PauseOverlay != null) m_PauseOverlay.style.display = DisplayStyle.Flex;
    }

    public void HidePauseMenu()
    {
        if (m_PauseOverlay != null) m_PauseOverlay.style.display = DisplayStyle.None;
        if (m_SettingsPanel != null) m_SettingsPanel.style.display = DisplayStyle.None;
    }

    public void HideGameOverPanel()
    {
        if (m_GameOverPanel != null) m_GameOverPanel.style.display = DisplayStyle.None;
    }
}
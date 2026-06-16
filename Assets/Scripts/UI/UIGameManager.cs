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
    private Label m_NameLabel;
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

    private const int MIN_WINDOW_WIDTH = 800;
    private const int MIN_WINDOW_HEIGHT = 600;

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

        // Referencias del GameUI.uxml
        m_LifeLabel = root.Q<Label>("FoodLabel");
        m_AchievementLabel = root.Q<Label>("AchievementLabel");
        if (m_AchievementLabel != null) m_AchievementLabel.text = "";

        // Crear HUD dinámico (stats laterales)
        SetupHUD(root);

        // Crear paneles (pausa, settings, game over)
        SetupPauseMenu(root);
        SetupSettingsPanel(root);
        SetupGameOverPanel(root);
    }

    void SetupHUD(VisualElement root)
    {
        // Panel izquierdo - Estadísticas
        var leftPanel = new VisualElement();
        leftPanel.AddToClassList("hud-panel-left");

        m_StrengthLabel = CreateHUDLabel("Fuerza: 1", "hud-strength");
        leftPanel.Add(m_StrengthLabel);

        m_AgilityLabel = CreateHUDLabel("Agilidad: 1", "hud-agility");
        leftPanel.Add(m_AgilityLabel);

        m_IntelligenceLabel = CreateHUDLabel("Intelig.: 1", "hud-intelligence");
        leftPanel.Add(m_IntelligenceLabel);

        m_HealthLabel = CreateHUDLabel("Salud: 1", "hud-health");
        leftPanel.Add(m_HealthLabel);

        m_SkillPointsLabel = CreateHUDLabel("Puntos: 0", "hud-skillpoints");
        leftPanel.Add(m_SkillPointsLabel);

        root.Add(leftPanel);

        // Panel derecho - Progreso
        var rightPanel = new VisualElement();
        rightPanel.AddToClassList("hud-panel-right");

        m_NightLabel = CreateHUDLabel("Noche: 1", "hud-night");
        rightPanel.Add(m_NightLabel);

        m_XPLabel = CreateHUDLabel("XP: 0", "hud-xp");
        rightPanel.Add(m_XPLabel);

        root.Add(rightPanel);

        // Label de nombre (se crea dinámicamente)
        m_NameLabel = new Label("Hero: Desconocido");
        m_NameLabel.AddToClassList("hud-label");
        m_NameLabel.style.position = Position.Absolute;
        m_NameLabel.style.top = 10;
        m_NameLabel.style.left = 0;
        m_NameLabel.style.right = 0;
        m_NameLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
        root.Add(m_NameLabel);
    }

    private Label CreateHUDLabel(string text, string className)
    {
        var label = new Label(text);
        label.AddToClassList("hud-label");
        label.AddToClassList(className);
        return label;
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
            m_StrengthLabel.text = "Fuerza: " + data.Strength;
            m_AgilityLabel.text = "Agilidad: " + data.Agility;
            m_IntelligenceLabel.text = "Intelig.: " + data.Intelligence;
            m_HealthLabel.text = "Salud: " + data.Health;
        }

        if (m_NameLabel != null) m_NameLabel.text = "Hero: " + playerName;
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

    void SetupPauseMenu(VisualElement root)
    {
        m_PauseOverlay = new VisualElement();
        m_PauseOverlay.AddToClassList("pause-overlay");

        var panel = new VisualElement();
        panel.AddToClassList("pause-panel");

        var title = new Label("PAUSA");
        title.AddToClassList("pause-title");
        panel.Add(title);

        var continueBtn = new Button { text = "Continuar" };
        continueBtn.AddToClassList("pause-btn");
        continueBtn.clicked += () => GameManager.Instance.ResumeGame();
        panel.Add(continueBtn);

        var settingsBtn = new Button { text = "Configuración" };
        settingsBtn.AddToClassList("pause-btn");
        settingsBtn.clicked += OpenSettingsFromPause;
        panel.Add(settingsBtn);

        var exitBtn = new Button { text = "Guardar y Salir" };
        exitBtn.AddToClassList("pause-btn");
        exitBtn.AddToClassList("pause-btn-exit");
        exitBtn.clicked += () => GameManager.Instance.SaveAndExit();
        panel.Add(exitBtn);

        m_PauseOverlay.Add(panel);
        root.Add(m_PauseOverlay);
    }

    void SetupSettingsPanel(VisualElement root)
    {
        m_SettingsPanel = new VisualElement();
        m_SettingsPanel.AddToClassList("pause-overlay");
        m_SettingsPanel.style.display = DisplayStyle.None;

        var panel = new VisualElement();
        panel.AddToClassList("pause-panel");
        panel.style.width = 450;

        var title = new Label("Configuración");
        title.AddToClassList("pause-title");
        panel.Add(title);

        var fullscreenRow = CreateSettingRow("Pantalla Completa");
        m_FullscreenToggle = new Toggle();
        m_FullscreenToggle.AddToClassList("settings-toggle");
        m_FullscreenToggle.RegisterValueChangedCallback(OnFullscreenChanged);
        fullscreenRow.Add(m_FullscreenToggle);
        panel.Add(fullscreenRow);

        var fpsRow = CreateSettingRow("Limitar a 60 FPS");
        m_LimitFPSToggle = new Toggle();
        m_LimitFPSToggle.AddToClassList("settings-toggle");
        m_LimitFPSToggle.RegisterValueChangedCallback(OnLimitFPSChanged);
        fpsRow.Add(m_LimitFPSToggle);
        panel.Add(fpsRow);

        var showFPSRow = CreateSettingRow("Mostrar FPS");
        m_ShowFPSToggle = new Toggle();
        m_ShowFPSToggle.AddToClassList("settings-toggle");
        m_ShowFPSToggle.RegisterValueChangedCallback(OnShowFPSChanged);
        showFPSRow.Add(m_ShowFPSToggle);
        panel.Add(showFPSRow);

        var muteRow = CreateSettingRow("Silenciar Música");
        m_MuteMusicToggle = new Toggle();
        m_MuteMusicToggle.AddToClassList("settings-toggle");
        m_MuteMusicToggle.RegisterValueChangedCallback(OnMuteMusicChanged);
        muteRow.Add(m_MuteMusicToggle);
        panel.Add(muteRow);

        var separator = new VisualElement();
        separator.style.height = 20;
        panel.Add(separator);

        var backBtn = new Button { text = "Volver" };
        backBtn.AddToClassList("pause-btn");
        backBtn.AddToClassList("pause-btn-exit");
        backBtn.clicked += CloseSettingsFromPause;
        panel.Add(backBtn);

        m_SettingsPanel.Add(panel);
        root.Add(m_SettingsPanel);

        LoadSettingsValues();
    }

    VisualElement CreateSettingRow(string labelText)
    {
        var row = new VisualElement();
        row.AddToClassList("settings-row");
        var label = new Label(labelText);
        label.AddToClassList("settings-label");
        row.Add(label);
        return row;
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

    void OnFullscreenChanged(ChangeEvent<bool> evt)
    {
        if (evt.newValue)
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
        PlayerPrefs.SetInt("Fullscreen", evt.newValue ? 1 : 0);
        PlayerPrefs.Save();
    }

    void OnLimitFPSChanged(ChangeEvent<bool> evt)
    {
        if (evt.newValue)
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;
        }
        else
        {
            QualitySettings.vSyncCount = 1;
            Application.targetFrameRate = -1;
        }
        PlayerPrefs.SetInt("LimitFPS", evt.newValue ? 1 : 0);
        PlayerPrefs.Save();
    }

    void OnShowFPSChanged(ChangeEvent<bool> evt)
    {
        if (FPSCounter.Instance != null)
        {
            FPSCounter.Instance.SetShowFPS(evt.newValue);
        }
    }

    void OnMuteMusicChanged(ChangeEvent<bool> evt)
    {
        AudioListener.volume = evt.newValue ? 0f : 1f;
        PlayerPrefs.SetInt("MuteMusic", evt.newValue ? 1 : 0);
        PlayerPrefs.Save();
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

    void SetupGameOverPanel(VisualElement root)
    {
        m_GameOverPanel = new VisualElement();
        m_GameOverPanel.AddToClassList("gameover-overlay");
        m_GameOverPanel.style.display = DisplayStyle.None;

        var panel = new VisualElement();
        panel.AddToClassList("gameover-panel");

        var title = new Label("HAS MUERTO");
        title.AddToClassList("gameover-title");
        panel.Add(title);

        m_GameOverHeroLabel = new Label("");
        m_GameOverHeroLabel.AddToClassList("gameover-hero");
        panel.Add(m_GameOverHeroLabel);

        var statsContainer = new VisualElement();
        statsContainer.AddToClassList("gameover-stats");

        m_GameOverDaysLabel = new Label("Noches sobrevividas: 0");
        m_GameOverDaysLabel.AddToClassList("gameover-stat");
        statsContainer.Add(m_GameOverDaysLabel);

        m_GameOverXPLabel = new Label("XP conseguido: 0");
        m_GameOverXPLabel.AddToClassList("gameover-stat");
        statsContainer.Add(m_GameOverXPLabel);

        panel.Add(statsContainer);

        var separator = new VisualElement();
        separator.AddToClassList("gameover-separator");
        panel.Add(separator);

        var topTitle = new Label("TOP 5 JUGADORES");
        topTitle.AddToClassList("gameover-top-title");
        panel.Add(topTitle);

        m_TopPlayersContainer = new VisualElement();
        m_TopPlayersContainer.AddToClassList("gameover-top-container");
        panel.Add(m_TopPlayersContainer);

        var btnContainer = new VisualElement();
        btnContainer.AddToClassList("gameover-buttons");

        var exitBtn = new Button { text = "Salir al Menú" };
        exitBtn.AddToClassList("gameover-btn");
        exitBtn.AddToClassList("gameover-btn-exit");
        exitBtn.clicked += () => GameManager.Instance.OnExitClicked();
        btnContainer.Add(exitBtn);

        var retryBtn = new Button { text = "Reintentar" };
        retryBtn.AddToClassList("gameover-btn");
        retryBtn.AddToClassList("gameover-btn-retry");
        retryBtn.clicked += () => GameManager.Instance.OnRetryClicked();
        btnContainer.Add(retryBtn);

        panel.Add(btnContainer);
        m_GameOverPanel.Add(panel);
        root.Add(m_GameOverPanel);
    }
}
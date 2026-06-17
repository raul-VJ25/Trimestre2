using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class UIMenuManager : MonoBehaviour
{
    public static UIMenuManager Instance { get; private set; }
    public UIDocument UIDocument;

    // Paneles
    private VisualElement m_LoadPanel;
    private VisualElement m_SettingsPanel;
    private VisualElement m_AchievementsPanel;
    private VisualElement m_AchievementDetailsPanel;

    // Scrolls
    private ScrollView m_LoadScroll;
    private ScrollView m_AchievementsScroll;
    private ScrollView m_AchievementDetailsScroll;

    // Toggles
    private Toggle m_FullscreenToggle;
    private Toggle m_LimitFPSToggle;
    private Toggle m_ShowFPSToggle;
    private Toggle m_MuteMusicToggle;

    // Botones
    private Button m_DeleteAllSavesButton;
    private bool m_IsInitializing = true;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        if (UIDocument == null) return;
        var root = UIDocument.rootVisualElement;

        // Botones principales
        var newGameButton = root.Q<Button>("NewGameButton");
        var loadGameButton = root.Q<Button>("LoadGameButton");
        var achievementsButton = root.Q<Button>("AchievementsButton");
        var settingsButton = root.Q<Button>("SettingsButton");
        var exitButton = root.Q<Button>("ExitButton");

        if (newGameButton != null) newGameButton.clicked += () => MenuManager.Instance.OnNewGameClicked();
        if (loadGameButton != null) loadGameButton.clicked += () => MenuManager.Instance.OnLoadGameClicked();
        if (achievementsButton != null) achievementsButton.clicked += () => MenuManager.Instance.OnAchievementsClicked();
        if (settingsButton != null) settingsButton.clicked += () => MenuManager.Instance.OnSettingsClicked();
        if (exitButton != null) exitButton.clicked += () => MenuManager.Instance.OnExitClicked();

        // Paneles
        m_LoadPanel = root.Q<VisualElement>("LoadPanel");
        m_SettingsPanel = root.Q<VisualElement>("SettingsPanel");
        m_AchievementsPanel = root.Q<VisualElement>("AchievementsPanel");
        m_AchievementDetailsPanel = root.Q<VisualElement>("AchievementDetailsPanel");

        // Scrolls
        m_LoadScroll = root.Q<ScrollView>("LoadScroll");
        m_AchievementsScroll = root.Q<ScrollView>("AchievementsScroll");
        m_AchievementDetailsScroll = root.Q<ScrollView>("AchievementDetailsScroll");

        // Botones de volver
        var loadBackBtn = root.Q<Button>("LoadBackButton");
        if (loadBackBtn != null) loadBackBtn.clicked += () => { if (m_LoadPanel != null) m_LoadPanel.style.display = DisplayStyle.None; };

        var achievementsBackBtn = root.Q<Button>("AchievementsBackButton");
        if (achievementsBackBtn != null) achievementsBackBtn.clicked += () => { if (m_AchievementsPanel != null) m_AchievementsPanel.style.display = DisplayStyle.None; };

        var detailsBackBtn = root.Q<Button>("AchievementDetailsBackButton");
        if (detailsBackBtn != null) detailsBackBtn.clicked += () => {
            if (m_AchievementDetailsPanel != null) m_AchievementDetailsPanel.style.display = DisplayStyle.None;
            if (m_AchievementsPanel != null) m_AchievementsPanel.style.display = DisplayStyle.Flex;
        };

        var settingsBackBtn = root.Q<Button>("SettingsBackButton");
        if (settingsBackBtn != null) settingsBackBtn.clicked += () => {
            if (m_SettingsPanel != null) m_SettingsPanel.style.display = DisplayStyle.None;
            MenuManager.Instance.ResetDeleteConfirmation();
        };

        // Toggles
        m_FullscreenToggle = root.Q<Toggle>("FullscreenToggle");
        m_LimitFPSToggle = root.Q<Toggle>("LimitFPSToggle");
        m_ShowFPSToggle = root.Q<Toggle>("ShowFPSToggle");
        m_MuteMusicToggle = root.Q<Toggle>("MuteMusicToggle");

        if (m_FullscreenToggle != null) m_FullscreenToggle.RegisterValueChangedCallback(evt => {
            if (!m_IsInitializing && SettingsManager.Instance != null) SettingsManager.Instance.SetFullscreen(evt.newValue);
        });

        if (m_LimitFPSToggle != null) m_LimitFPSToggle.RegisterValueChangedCallback(evt => {
            if (!m_IsInitializing && SettingsManager.Instance != null) SettingsManager.Instance.SetLimitFPS(evt.newValue);
        });

        if (m_ShowFPSToggle != null) m_ShowFPSToggle.RegisterValueChangedCallback(evt => {
            if (!m_IsInitializing && SettingsManager.Instance != null) SettingsManager.Instance.SetShowFPS(evt.newValue);
        });

        if (m_MuteMusicToggle != null) m_MuteMusicToggle.RegisterValueChangedCallback(evt => {
            if (!m_IsInitializing && SettingsManager.Instance != null) SettingsManager.Instance.SetMuteMusic(evt.newValue);
        });

        // Botón borrar partidas
        m_DeleteAllSavesButton = root.Q<Button>("DeleteAllSavesButton");
        if (m_DeleteAllSavesButton != null) m_DeleteAllSavesButton.clicked += () => MenuManager.Instance.OnDeleteAllSavesClicked();

        LoadSettingsUI();
        m_IsInitializing = false;
    }

    void LoadSettingsUI()
    {
        bool isFullscreen = PlayerPrefs.GetInt("Fullscreen", Screen.fullScreen ? 1 : 0) == 1;
        if (m_FullscreenToggle != null) m_FullscreenToggle.value = isFullscreen;

        bool limitFPS = PlayerPrefs.GetInt("LimitFPS", 0) == 1;
        if (m_LimitFPSToggle != null) m_LimitFPSToggle.value = limitFPS;

        bool showFPS = PlayerPrefs.GetInt("ShowFPS", 0) == 1;
        if (m_ShowFPSToggle != null) m_ShowFPSToggle.value = showFPS;

        bool muteMusic = PlayerPrefs.GetInt("MuteMusic", 0) == 1;
        if (m_MuteMusicToggle != null) m_MuteMusicToggle.value = muteMusic;
    }

    public void OnLoadGameClicked()
    {
        if (m_LoadPanel != null) m_LoadPanel.style.display = DisplayStyle.Flex;
        RefreshSaveFilesList();
    }

    public void OnSettingsClicked()
    {
        if (m_SettingsPanel != null) m_SettingsPanel.style.display = DisplayStyle.Flex;
    }

    // NUEVO MÉTODO PARA MOSTRAR EL PANEL DE LOGROS
    public void ShowAchievementsPanel()
    {
        if (m_AchievementsPanel != null)
            m_AchievementsPanel.style.display = DisplayStyle.Flex;
    }

    public void RefreshSaveFilesList()
    {
        if (m_LoadScroll == null) return;
        m_LoadScroll.Clear();
        string[] files = SaveManager.GetAllSaveFiles();
        if (files.Length == 0)
        {
            var noSavesLabel = new Label("No hay partidas guardadas");
            noSavesLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            noSavesLabel.style.color = Color.gray;
            m_LoadScroll.Add(noSavesLabel);
            return;
        }
        foreach (string filePath in files)
        {
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            DateTime timestamp = SaveManager.GetSaveTimestamp(filePath);
            string timestampText = timestamp != DateTime.MinValue ? $"({timestamp:dd/MM/yy - HH:mm})" : "";

            var btn = new Button();
            btn.AddToClassList("pause-btn");
            btn.style.height = 40;
            btn.style.paddingLeft = 10;
            btn.style.paddingRight = 10;

            // Contenedor con flexbox para separar nombre y fecha
            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Row;
            container.style.justifyContent = Justify.SpaceBetween;
            container.style.alignItems = Align.Center;
            container.style.width = Length.Percent(100);

            // Label del nombre (izquierda)
            var nameLabel = new Label(fileName);
            nameLabel.style.flexGrow = 1;
            nameLabel.style.marginRight = 10;
            nameLabel.style.unityTextAlign = TextAnchor.MiddleLeft;
            nameLabel.style.whiteSpace = WhiteSpace.Normal;
            nameLabel.style.overflow = Overflow.Hidden;

            // Label de la fecha (derecha)
            var dateLabel = new Label(timestampText);
            dateLabel.style.flexShrink = 0;
            dateLabel.style.unityTextAlign = TextAnchor.MiddleRight;
            dateLabel.style.color = new StyleColor(new Color(0.7f, 0.7f, 0.7f));
            dateLabel.style.fontSize = 9;

            container.Add(nameLabel);
            container.Add(dateLabel);
            btn.Add(container);

            string pathCopy = filePath;
            btn.clicked += () => MenuManager.Instance.OnSaveFileSelected(pathCopy);
            m_LoadScroll.Add(btn);
        }
    }

    public void RefreshAchievementsList()
    {
        if (m_AchievementsScroll == null) return;
        m_AchievementsScroll.Clear();

        string[] files = SaveManager.GetAllSaveFiles();
        if (files.Length == 0)
        {
            var noSavesLabel = new Label("No hay partidas guardadas");
            noSavesLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            noSavesLabel.style.color = Color.gray;
            m_AchievementsScroll.Add(noSavesLabel);
            return;
        }

        int totalCount = GetDefaultAchievements().Count;
        List<(PlayerData data, string filePath, float percentage)> entries = new List<(PlayerData, string, float)>();

        foreach (string filePath in files)
        {
            string fileName = Path.GetFileName(filePath);
            PlayerData data = SaveManager.LoadPlayerData(fileName);
            if (data != null)
            {
                int completedCount = 0;
                if (data.Achievements != null)
                {
                    foreach (var achievement in data.Achievements)
                    {
                        if (achievement.Completed) completedCount++;
                    }
                }
                float percentage = totalCount > 0 ? (float)completedCount / totalCount * 100f : 0f;
                entries.Add((data, filePath, percentage));
            }
        }

        entries.Sort((a, b) => b.percentage.CompareTo(a.percentage));
        foreach (var entry in entries)
            m_AchievementsScroll.Add(CreateAchievementRow(entry.data, entry.filePath));
    }

    VisualElement CreateAchievementRow(PlayerData data, string filePath)
    {
        var row = new VisualElement();
        row.AddToClassList("achievement-row");

        var nameLabel = new Label(data.Name);
        nameLabel.AddToClassList("achievement-name");
        row.Add(nameLabel);

        int completedCount = 0;
        int totalCount = GetDefaultAchievements().Count;
        if (data.Achievements != null && data.Achievements.Count > 0)
        {
            foreach (var achievement in data.Achievements)
            {
                if (achievement.Completed) completedCount++;
            }
        }
        float percentage = totalCount > 0 ? (float)completedCount / totalCount * 100f : 0f;

        var progressContainer = new VisualElement();
        progressContainer.AddToClassList("achievement-progress");

        var percentLabel = new Label($"{percentage:F0}%");
        percentLabel.AddToClassList("achievement-percent");
        progressContainer.Add(percentLabel);

        var barContainer = new VisualElement();
        barContainer.AddToClassList("progress-container");

        var bar = new VisualElement();
        bar.AddToClassList("progress-bar");
        bar.style.width = new Length(percentage, LengthUnit.Percent);
        if (percentage >= 100) bar.AddToClassList("progress-bar-complete");
        else if (percentage >= 50) bar.AddToClassList("progress-bar-half");
        barContainer.Add(bar);

        progressContainer.Add(barContainer);
        row.Add(progressContainer);

        var infoBtn = new Button { text = ">" };
        infoBtn.AddToClassList("achievement-info-btn");

        string filePathCopy = filePath;
        infoBtn.clicked += () => ShowAchievementDetails(filePathCopy);
        row.Add(infoBtn);

        return row;
    }

    void ShowAchievementDetails(string filePath)
    {
        PlayerData data = SaveManager.LoadPlayerData(Path.GetFileName(filePath));
        if (data == null) return;

        if (m_AchievementsPanel != null) m_AchievementsPanel.style.display = DisplayStyle.None;
        if (m_AchievementDetailsPanel != null) m_AchievementDetailsPanel.style.display = DisplayStyle.Flex;

        var title = m_AchievementDetailsPanel.Q<Label>("AchievementDetailsTitle");
        if (title != null) title.text = $"Logros de {data.Name}";

        if (m_AchievementDetailsScroll != null) m_AchievementDetailsScroll.Clear();

        List<Achievement> defaultAchievements = GetDefaultAchievements();
        List<(Achievement defaultAch, Achievement savedAch, float percentage)> achievementEntries = new List<(Achievement, Achievement, float)>();

        foreach (var defaultAch in defaultAchievements)
        {
            Achievement savedAch = null;
            if (data.Achievements != null)
                savedAch = data.Achievements.Find(a => a.ID == defaultAch.ID && a.Name == defaultAch.Name);

            int currentAmount = savedAch != null ? savedAch.CurrentAmount : 0;
            int targetAmount = defaultAch.AmountToAchieve;
            float percentage = targetAmount > 0 ? (float)currentAmount / targetAmount * 100f : 0f;
            if (percentage > 100f) percentage = 100f;

            achievementEntries.Add((defaultAch, savedAch, percentage));
        }

        achievementEntries.Sort((a, b) => b.percentage.CompareTo(a.percentage));
        foreach (var entry in achievementEntries)
            m_AchievementDetailsScroll.Add(CreateAchievementDetailRow(entry.defaultAch, entry.savedAch));
    }

    VisualElement CreateAchievementDetailRow(Achievement defaultAch, Achievement savedAch)
    {
        var row = new VisualElement();
        row.AddToClassList("achievement-detail-row");

        var infoContainer = new VisualElement();
        infoContainer.AddToClassList("achievement-info");

        var nameLabel = new Label(defaultAch.Name);
        nameLabel.AddToClassList("achievement-detail-name");
        infoContainer.Add(nameLabel);

        var descLabel = new Label(defaultAch.Description);
        descLabel.AddToClassList("achievement-detail-desc");
        infoContainer.Add(descLabel);

        row.Add(infoContainer);

        int currentAmount = savedAch != null ? savedAch.CurrentAmount : 0;
        int targetAmount = defaultAch.AmountToAchieve;
        bool isCompleted = savedAch != null && savedAch.Completed;
        float percentage = (float)currentAmount / targetAmount * 100f;
        if (percentage > 100f) percentage = 100f;

        var progressContainer = new VisualElement();
        progressContainer.AddToClassList("achievement-detail-progress");

        var progressBarContainer = new VisualElement();
        progressBarContainer.AddToClassList("progress-container");

        var progressBar = new VisualElement();
        progressBar.AddToClassList("progress-bar");
        progressBar.style.width = new Length(percentage, LengthUnit.Percent);
        if (isCompleted) progressBar.AddToClassList("progress-bar-complete");
        else if (percentage >= 50) progressBar.AddToClassList("progress-bar-half");
        progressBarContainer.Add(progressBar);

        progressContainer.Add(progressBarContainer);

        var progressText = new Label($"{currentAmount}/{targetAmount} ({percentage:F0}%)");
        progressText.AddToClassList("achievement-progress-text");
        if (isCompleted)
        {
            progressText.text = "Completado";
            progressText.AddToClassList("achievement-completed");
        }
        progressContainer.Add(progressText);

        row.Add(progressContainer);

        return row;
    }

    List<Achievement> GetDefaultAchievements()
    {
        List<Achievement> achievements = new List<Achievement>();
        achievements.Add(new Achievement("KILL", "Homicidio", "Primeras 5 bajas", 5));
        achievements.Add(new Achievement("KILL", "Exterminador", "La que has liao", 100));
        achievements.Add(new Achievement("KILL", "Loco", "Para ya", 1000));
        achievements.Add(new Achievement("BREAK", "Picapiedra", "No más muros", 100));
        achievements.Add(new Achievement("EAT", "Lluvia de albóndigas", "Pero si son hamburguesas", 100));
        achievements.Add(new Achievement("WALK", "Inicio", "Primeros 20 pasos", 20));
        achievements.Add(new Achievement("WALK", "Strava", "Pesao", 1000));
        return achievements;
    }
}
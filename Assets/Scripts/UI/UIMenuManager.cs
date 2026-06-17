using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class UIMenuManager : MonoBehaviour
{
    public static UIMenuManager Instance { get; private set; }

    [Header("UIDocument")]
    public UIDocument UIDocument;

    [Header("Botones del Menú Principal")]
    private Button m_NewGameButton;
    private Button m_LoadGameButton;
    private Button m_AchievementsButton;
    private Button m_SettingsButton;
    private Button m_ExitButton;

    [Header("Paneles")]
    private VisualElement m_LoadPanel;
    private VisualElement m_SettingsPanel;
    private VisualElement m_AchievementsPanel;
    private VisualElement m_AchievementDetailsPanel;

    [Header("Configuración")]
    private Toggle m_FullscreenToggle;
    private Toggle m_LimitFPSToggle;
    private Toggle m_MuteMusicToggle;
    private Toggle m_ShowFPSToggle;
    private Button m_DeleteAllSavesButton;

    [Header("Logros")]
    private ScrollView m_AchievementsScroll;
    private ScrollView m_AchievementDetailsScroll;

    // Bandera para evitar que los toggles disparen eventos durante la inicialización
    private bool m_IsInitializing = true;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        if (UIDocument == null)
        {
            Debug.LogError("UIMenuManager: No se ha asignado el UIDocument");
            return;
        }

        var root = UIDocument.rootVisualElement;

        // Referencias a botones del MenuUI.uxml
        m_NewGameButton = root.Q<Button>("NewGameButton");
        m_LoadGameButton = root.Q<Button>("LoadGameButton");
        m_AchievementsButton = root.Q<Button>("AchievementsButton");
        m_SettingsButton = root.Q<Button>("SettingsButton");
        m_ExitButton = root.Q<Button>("ExitButton");

        // Asignar callbacks
        if (m_NewGameButton != null) m_NewGameButton.clicked += () => MenuManager.Instance.OnNewGameClicked();
        if (m_LoadGameButton != null) m_LoadGameButton.clicked += () => MenuManager.Instance.OnLoadGameClicked();
        if (m_AchievementsButton != null) m_AchievementsButton.clicked += () => MenuManager.Instance.OnAchievementsClicked();
        if (m_SettingsButton != null) m_SettingsButton.clicked += () => MenuManager.Instance.OnSettingsClicked();
        if (m_ExitButton != null) m_ExitButton.clicked += () => MenuManager.Instance.OnExitClicked();

        // Crear paneles adicionales
        SetupLoadPanel(root);
        SetupSettingsPanel(root);
        SetupAchievementsPanel(root);
        SetupAchievementDetailsPanel(root);
        LoadSettingsUI();

        // La inicialización ha terminado, ahora los toggles sí pueden disparar eventos
        m_IsInitializing = false;
    }

    void SetupLoadPanel(VisualElement root)
    {
        m_LoadPanel = new VisualElement();
        m_LoadPanel.AddToClassList("pause-overlay");
        m_LoadPanel.style.display = DisplayStyle.None;
        var panel = new VisualElement();
        panel.AddToClassList("pause-panel");
        var title = new Label("Partidas Guardadas");
        title.AddToClassList("pause-title");
        panel.Add(title);
        var scroll = new ScrollView();
        scroll.style.maxHeight = 300;
        scroll.name = "SaveListScroll";
        panel.Add(scroll);
        var backBtn = new Button { text = "Volver" };
        backBtn.AddToClassList("pause-btn");
        backBtn.AddToClassList("pause-btn-exit");
        backBtn.clicked += () => { m_LoadPanel.style.display = DisplayStyle.None; };
        panel.Add(backBtn);
        m_LoadPanel.Add(panel);
        root.Add(m_LoadPanel);
    }

    void SetupSettingsPanel(VisualElement root)
    {
        m_SettingsPanel = new VisualElement();
        m_SettingsPanel.AddToClassList("pause-overlay");
        m_SettingsPanel.style.display = DisplayStyle.None;
        var panel = new VisualElement();
        panel.AddToClassList("pause-panel");
        panel.style.width = 350;
        var title = new Label("Configuración");
        title.AddToClassList("pause-title");
        panel.Add(title);

        var fullscreenRow = CreateSettingRow("Pantalla Completa");
        m_FullscreenToggle = new Toggle();
        m_FullscreenToggle.AddToClassList("settings-toggle");
        m_FullscreenToggle.RegisterValueChangedCallback(evt => {
            if (!m_IsInitializing && SettingsManager.Instance != null) SettingsManager.Instance.SetFullscreen(evt.newValue);
        });
        fullscreenRow.Add(m_FullscreenToggle);
        panel.Add(fullscreenRow);

        var fpsRow = CreateSettingRow("Limitar a 60 FPS");
        m_LimitFPSToggle = new Toggle();
        m_LimitFPSToggle.AddToClassList("settings-toggle");
        m_LimitFPSToggle.RegisterValueChangedCallback(evt => {
            if (!m_IsInitializing && SettingsManager.Instance != null) SettingsManager.Instance.SetLimitFPS(evt.newValue);
        });
        fpsRow.Add(m_LimitFPSToggle);
        panel.Add(fpsRow);

        var showFPSRow = CreateSettingRow("Mostrar FPS");
        m_ShowFPSToggle = new Toggle();
        m_ShowFPSToggle.AddToClassList("settings-toggle");
        m_ShowFPSToggle.RegisterValueChangedCallback(evt => {
            if (!m_IsInitializing && SettingsManager.Instance != null) SettingsManager.Instance.SetShowFPS(evt.newValue);
        });
        showFPSRow.Add(m_ShowFPSToggle);
        panel.Add(showFPSRow);

        var muteRow = CreateSettingRow("Silenciar Música");
        m_MuteMusicToggle = new Toggle();
        m_MuteMusicToggle.AddToClassList("settings-toggle");
        m_MuteMusicToggle.RegisterValueChangedCallback(evt => {
            if (!m_IsInitializing && SettingsManager.Instance != null) SettingsManager.Instance.SetMuteMusic(evt.newValue);
        });
        muteRow.Add(m_MuteMusicToggle);
        panel.Add(muteRow);

        var separator = new VisualElement(); separator.style.height = 20; panel.Add(separator);

        m_DeleteAllSavesButton = new Button { text = "Borrar Todas las Partidas" };
        m_DeleteAllSavesButton.AddToClassList("pause-btn");
        m_DeleteAllSavesButton.AddToClassList("settings-delete-btn");
        m_DeleteAllSavesButton.clicked += () => MenuManager.Instance.OnDeleteAllSavesClicked();
        panel.Add(m_DeleteAllSavesButton);

        var separator2 = new VisualElement(); separator2.style.height = 10; panel.Add(separator2);

        var backBtn = new Button { text = "Volver" };
        backBtn.AddToClassList("pause-btn"); backBtn.AddToClassList("pause-btn-exit");
        backBtn.clicked += () => { m_SettingsPanel.style.display = DisplayStyle.None; MenuManager.Instance.ResetDeleteConfirmation(); };
        panel.Add(backBtn);
        m_SettingsPanel.Add(panel);
        root.Add(m_SettingsPanel);
    }

    void SetupAchievementsPanel(VisualElement root)
    {
        m_AchievementsPanel = new VisualElement();
        m_AchievementsPanel.AddToClassList("pause-overlay");
        m_AchievementsPanel.style.display = DisplayStyle.None;
        var panel = new VisualElement();
        panel.AddToClassList("pause-panel");
        panel.style.width = 400;
        var title = new Label("Logros por Partida");
        title.AddToClassList("pause-title");
        panel.Add(title);
        m_AchievementsScroll = new ScrollView();
        m_AchievementsScroll.style.maxHeight = 350;
        panel.Add(m_AchievementsScroll);
        var backBtn = new Button { text = "Volver" };
        backBtn.AddToClassList("pause-btn"); backBtn.AddToClassList("pause-btn-exit");
        backBtn.clicked += () => { m_AchievementsPanel.style.display = DisplayStyle.None; };
        panel.Add(backBtn);
        m_AchievementsPanel.Add(panel);
        root.Add(m_AchievementsPanel);
    }

    void SetupAchievementDetailsPanel(VisualElement root)
    {
        m_AchievementDetailsPanel = new VisualElement();
        m_AchievementDetailsPanel.AddToClassList("pause-overlay");
        m_AchievementDetailsPanel.style.display = DisplayStyle.None;
        var panel = new VisualElement();
        panel.AddToClassList("pause-panel");
        panel.style.width = 450;
        var title = new Label("Detalles de Logros");
        title.name = "AchievementDetailsTitle";
        title.AddToClassList("pause-title");
        panel.Add(title);
        m_AchievementDetailsScroll = new ScrollView();
        m_AchievementDetailsScroll.style.maxHeight = 400;
        panel.Add(m_AchievementDetailsScroll);
        var backBtn = new Button { text = "Volver" };
        backBtn.AddToClassList("pause-btn"); backBtn.AddToClassList("pause-btn-exit");
        backBtn.clicked += () =>
        {
            m_AchievementDetailsPanel.style.display = DisplayStyle.None;
            m_AchievementsPanel.style.display = DisplayStyle.Flex;
        };
        panel.Add(backBtn);
        m_AchievementDetailsPanel.Add(panel);
        root.Add(m_AchievementDetailsPanel);
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

    public void RefreshSaveList()
    {
        var scroll = m_LoadPanel?.Q<ScrollView>("SaveListScroll");
        if (scroll == null)
        {
            Debug.LogError("UIMenuManager: No se encontró el ScrollView 'SaveListScroll'");
            return;
        }
        scroll.Clear();
        string[] files = SaveManager.GetAllSaveFiles();
        if (files.Length == 0)
        {
            var noSavesLabel = new Label("No hay partidas guardadas");
            noSavesLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            noSavesLabel.style.color = Color.gray;
            scroll.Add(noSavesLabel);
            return;
        }
        foreach (string filePath in files)
        {
            string fileName = Path.GetFileNameWithoutExtension(filePath);

            // Obtener timestamp del archivo
            DateTime timestamp = SaveManager.GetSaveTimestamp(filePath);
            string timestampText = timestamp != DateTime.MinValue ?
                $" ({timestamp:dd/MM/yy - HH:mm})" : "";

            string displayName = fileName + timestampText;

            var btn = new Button { text = displayName };
            btn.AddToClassList("pause-btn");
            btn.style.unityTextAlign = TextAnchor.MiddleLeft;

            string pathCopy = filePath;
            btn.clicked += () => MenuManager.Instance.OnSaveFileSelected(pathCopy);
            scroll.Add(btn);
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
            noSavesLabel.style.marginTop = 20; noSavesLabel.style.marginBottom = 20;
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
                if (data.Achievements != null) foreach (var achievement in data.Achievements) if (achievement.completed) completedCount++;
                float percentage = totalCount > 0 ? (float)completedCount / totalCount * 100f : 0f;
                entries.Add((data, filePath, percentage));
            }
        }
        entries.Sort((a, b) => b.percentage.CompareTo(a.percentage));
        foreach (var entry in entries) m_AchievementsScroll.Add(CreateAchievementRow(entry.data, entry.filePath));
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
        if (data.Achievements != null && data.Achievements.Count > 0) foreach (var achievement in data.Achievements) if (achievement.completed) completedCount++;
        float percentage = totalCount > 0 ? (float)completedCount / totalCount * 100f : 0f;
        var progressContainer = new VisualElement();
        progressContainer.AddToClassList("progress-container");
        var progressBar = new VisualElement();
        progressBar.AddToClassList("progress-bar");
        progressBar.style.width = new Length(percentage, LengthUnit.Percent);
        if (percentage >= 100) progressBar.AddToClassList("progress-bar-complete");
        else if (percentage >= 50) progressBar.AddToClassList("progress-bar-half");
        progressContainer.Add(progressBar);
        row.Add(progressContainer);
        var percentLabel = new Label($"{percentage:F0}%");
        percentLabel.AddToClassList("achievement-percent");
        row.Add(percentLabel);
        row.RegisterCallback<ClickEvent>(evt => ShowAchievementDetails(data));
        return row;
    }

    void ShowAchievementDetails(PlayerData data)
    {
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
            if (data.Achievements != null) savedAch = data.Achievements.Find(a => a.ID == defaultAch.ID && a.Name == defaultAch.Name);
            int currentAmount = savedAch != null ? savedAch.CurrentAmount : 0;
            int targetAmount = defaultAch.AmountToAchieve;
            float percentage = targetAmount > 0 ? (float)currentAmount / targetAmount * 100f : 0f;
            if (percentage > 100f) percentage = 100f;
            achievementEntries.Add((defaultAch, savedAch, percentage));
        }
        achievementEntries.Sort((a, b) => b.percentage.CompareTo(a.percentage));
        foreach (var entry in achievementEntries) m_AchievementDetailsScroll.Add(CreateAchievementDetailRow(entry.defaultAch, entry.savedAch));
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
        bool isCompleted = savedAch != null && savedAch.completed;
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
        if (isCompleted) { progressText.text = "Completado"; progressText.AddToClassList("achievement-completed"); }
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
        RefreshSaveList();
        if (m_LoadPanel != null) m_LoadPanel.style.display = DisplayStyle.Flex;
    }

    public void OnSettingsClicked()
    {
        if (m_SettingsPanel != null) m_SettingsPanel.style.display = DisplayStyle.Flex;
    }
}
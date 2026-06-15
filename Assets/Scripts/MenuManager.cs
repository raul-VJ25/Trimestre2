using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections.Generic;

// Gestor del menu principal
// Maneja partidas guardadas, logros y configuracion
public class MenuManager : MonoBehaviour
{
    public UIDocument UIDocument;
    public StyleSheet GameStyles;
    public string CharacterCreationSceneName = "CharacterCreationScene";

    private const int MIN_WINDOW_WIDTH = 800;
    private const int MIN_WINDOW_HEIGHT = 600;

    // Botones del menu principal
    private Button m_NewGameButton;
    private Button m_LoadGameButton;
    private Button m_AchievementsButton;
    private Button m_SettingsButton;
    private Button m_ExitButton;

    // Paneles
    private VisualElement m_LoadPanel;
    private VisualElement m_SettingsPanel;
    private VisualElement m_AchievementsPanel;
    private VisualElement m_AchievementDetailsPanel;

    // Configuracion
    private Toggle m_FullscreenToggle;
    private Toggle m_LimitFPSToggle;
    private Toggle m_MuteMusicToggle;
    private Toggle m_ShowFPSToggle;
    private Button m_DeleteAllSavesButton;
    private int m_DeleteConfirmationCount = 0;

    // Logros
    private ScrollView m_AchievementsScroll;
    private ScrollView m_AchievementDetailsScroll;

    // Inicializa el menu y conecta botones
    private void Start()
    {
        var root = UIDocument.rootVisualElement;

        if (GameStyles != null && !root.styleSheets.Contains(GameStyles))
        {
            root.styleSheets.Add(GameStyles);
        }

        m_NewGameButton = root.Q<Button>("NewGameButton");
        m_LoadGameButton = root.Q<Button>("LoadGameButton");
        m_AchievementsButton = root.Q<Button>("AchievementsButton");
        m_SettingsButton = root.Q<Button>("SettingsButton");
        m_ExitButton = root.Q<Button>("ExitButton");

        if (m_NewGameButton != null) m_NewGameButton.clicked += OnNewGameClicked;
        if (m_LoadGameButton != null) m_LoadGameButton.clicked += OnLoadGameClicked;
        if (m_AchievementsButton != null) m_AchievementsButton.clicked += OnAchievementsClicked;
        if (m_SettingsButton != null) m_SettingsButton.clicked += OnSettingsClicked;
        if (m_ExitButton != null) m_ExitButton.clicked += OnExitClicked;

        SetupLoadPanel(root);
        SetupSettingsPanel(root);
        SetupAchievementsPanel(root);
        SetupAchievementDetailsPanel(root);

        LoadSettings();
    }

    // Crea el panel de carga de partidas
    void SetupLoadPanel(VisualElement root)
    {
        m_LoadPanel = new VisualElement();
        m_LoadPanel.AddToClassList("pause-overlay");
        m_LoadPanel.style.display = DisplayStyle.None;

        var panel = new VisualElement();
        panel.AddToClassList("pause-panel");

        var title = new Label("Partidas");
        title.AddToClassList("pause-title");
        panel.Add(title);

        var scroll = new ScrollView();
        scroll.style.maxHeight = 300;
        panel.Add(scroll);

        var backBtn = new Button { text = "Volver" };
        backBtn.AddToClassList("pause-btn");
        backBtn.AddToClassList("pause-btn-exit");
        backBtn.clicked += () => { m_LoadPanel.style.display = DisplayStyle.None; };
        panel.Add(backBtn);

        m_LoadPanel.Add(panel);
        root.Add(m_LoadPanel);

        RefreshSaveList(scroll);
    }

    // Crea el panel de configuracion
    void SetupSettingsPanel(VisualElement root)
    {
        m_SettingsPanel = new VisualElement();
        m_SettingsPanel.AddToClassList("pause-overlay");
        m_SettingsPanel.style.display = DisplayStyle.None;

        var panel = new VisualElement();
        panel.AddToClassList("pause-panel");
        panel.style.width = 350;

        var title = new Label("Configuracion");
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

        var muteRow = CreateSettingRow("Silenciar Musica");
        m_MuteMusicToggle = new Toggle();
        m_MuteMusicToggle.AddToClassList("settings-toggle");
        m_MuteMusicToggle.RegisterValueChangedCallback(OnMuteMusicChanged);
        muteRow.Add(m_MuteMusicToggle);
        panel.Add(muteRow);

        var separator = new VisualElement();
        separator.style.height = 20;
        panel.Add(separator);

        m_DeleteAllSavesButton = new Button { text = "Borrar Todas las Partidas" };
        m_DeleteAllSavesButton.AddToClassList("pause-btn");
        m_DeleteAllSavesButton.AddToClassList("settings-delete-btn");
        m_DeleteAllSavesButton.clicked += OnDeleteAllSavesClicked;
        panel.Add(m_DeleteAllSavesButton);

        var separator2 = new VisualElement();
        separator2.style.height = 10;
        panel.Add(separator2);

        var backBtn = new Button { text = "Volver" };
        backBtn.AddToClassList("pause-btn");
        backBtn.AddToClassList("pause-btn-exit");
        backBtn.clicked += OnSettingsBackClicked;
        panel.Add(backBtn);

        m_SettingsPanel.Add(panel);
        root.Add(m_SettingsPanel);
    }

    // Crea el panel de lista de partidas con logros
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
        backBtn.AddToClassList("pause-btn");
        backBtn.AddToClassList("pause-btn-exit");
        backBtn.clicked += () => { m_AchievementsPanel.style.display = DisplayStyle.None; };
        panel.Add(backBtn);

        m_AchievementsPanel.Add(panel);
        root.Add(m_AchievementsPanel);
    }

    // Crea el panel de detalles de logros
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
        backBtn.AddToClassList("pause-btn");
        backBtn.AddToClassList("pause-btn-exit");
        backBtn.clicked += () =>
        {
            m_AchievementDetailsPanel.style.display = DisplayStyle.None;
            m_AchievementsPanel.style.display = DisplayStyle.Flex;
        };
        panel.Add(backBtn);

        m_AchievementDetailsPanel.Add(panel);
        root.Add(m_AchievementDetailsPanel);
    }

    // Crea una fila de configuracion
    VisualElement CreateSettingRow(string labelText)
    {
        var row = new VisualElement();
        row.AddToClassList("settings-row");

        var label = new Label(labelText);
        label.AddToClassList("settings-label");
        row.Add(label);

        return row;
    }

    // Refresca la lista de logros ordenada por porcentaje
    void RefreshAchievementsList()
    {
        m_AchievementsScroll.Clear();
        string[] files = SaveManager.GetAllSaveFiles();

        if (files.Length == 0)
        {
            var noSavesLabel = new Label("No hay partidas guardadas");
            noSavesLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            noSavesLabel.style.color = Color.gray;
            noSavesLabel.style.marginTop = 20;
            noSavesLabel.style.marginBottom = 20;
            m_AchievementsScroll.Add(noSavesLabel);
            return;
        }

        int totalCount = GetDefaultAchievements().Count;

        List<(PlayerData data, string filePath, float percentage)> entries
            = new List<(PlayerData, string, float)>();

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
                        if (achievement.completed) completedCount++;
                    }
                }
                float percentage = totalCount > 0 ? (float)completedCount / totalCount * 100f : 0f;
                entries.Add((data, filePath, percentage));
            }
        }

        entries.Sort((a, b) => b.percentage.CompareTo(a.percentage));

        foreach (var entry in entries)
        {
            var row = CreateAchievementRow(entry.data, entry.filePath);
            m_AchievementsScroll.Add(row);
        }
    }

    // Crea una fila de partida con progreso de logros
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
                if (achievement.completed)
                {
                    completedCount++;
                }
            }
        }

        float percentage = totalCount > 0 ? (float)completedCount / totalCount * 100f : 0f;

        var progressContainer = new VisualElement();
        progressContainer.AddToClassList("progress-container");

        var progressBar = new VisualElement();
        progressBar.AddToClassList("progress-bar");
        progressBar.style.width = new Length(percentage, LengthUnit.Percent);

        if (percentage >= 100)
            progressBar.AddToClassList("progress-bar-complete");
        else if (percentage >= 50)
            progressBar.AddToClassList("progress-bar-half");

        progressContainer.Add(progressBar);
        row.Add(progressContainer);

        var percentLabel = new Label($"{percentage:F0}%");
        percentLabel.AddToClassList("achievement-percent");
        row.Add(percentLabel);

        row.RegisterCallback<ClickEvent>(evt => ShowAchievementDetails(data));

        return row;
    }

    // Muestra los detalles de logros de una partida
    void ShowAchievementDetails(PlayerData data)
    {
        m_AchievementsPanel.style.display = DisplayStyle.None;
        m_AchievementDetailsPanel.style.display = DisplayStyle.Flex;

        var title = m_AchievementDetailsPanel.Q<Label>("AchievementDetailsTitle");
        if (title != null)
        {
            title.text = $"Logros de {data.Name}";
        }

        m_AchievementDetailsScroll.Clear();

        List<Achievement> defaultAchievements = GetDefaultAchievements();

        List<(Achievement defaultAch, Achievement savedAch, float percentage)> achievementEntries
            = new List<(Achievement, Achievement, float)>();

        foreach (var defaultAch in defaultAchievements)
        {
            Achievement savedAch = null;
            if (data.Achievements != null)
            {
                savedAch = data.Achievements.Find(a => a.ID == defaultAch.ID && a.Name == defaultAch.Name);
            }

            int currentAmount = savedAch != null ? savedAch.CurrentAmount : 0;
            int targetAmount = defaultAch.AmountToAchieve;
            float percentage = targetAmount > 0 ? (float)currentAmount / targetAmount * 100f : 0f;
            if (percentage > 100f) percentage = 100f;

            achievementEntries.Add((defaultAch, savedAch, percentage));
        }

        achievementEntries.Sort((a, b) => b.percentage.CompareTo(a.percentage));

        foreach (var entry in achievementEntries)
        {
            var row = CreateAchievementDetailRow(entry.defaultAch, entry.savedAch);
            m_AchievementDetailsScroll.Add(row);
        }
    }

    // Crea una fila de detalle de logro
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

        if (isCompleted)
            progressBar.AddToClassList("progress-bar-complete");
        else if (percentage >= 50)
            progressBar.AddToClassList("progress-bar-half");

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

    // Retorna la lista de logros por defecto
    List<Achievement> GetDefaultAchievements()
    {
        List<Achievement> achievements = new List<Achievement>();

        achievements.Add(new Achievement("KILL", "Homicidio", "Primeras 5 bajas", 5));
        achievements.Add(new Achievement("KILL", "Exterminador", "La que has liao", 100));
        achievements.Add(new Achievement("KILL", "Loco", "Para ya", 1000));

        achievements.Add(new Achievement("BREAK", "Picapiedra", "No mas muros", 100));

        achievements.Add(new Achievement("EAT", "Lluvia de albondigas", "Pero si son hamburguesas", 100));

        achievements.Add(new Achievement("WALK", "Inicio", "Primeros 20 pasos", 20));
        achievements.Add(new Achievement("WALK", "Strava", "Pesao", 1000));

        return achievements;
    }

    // Callbacks de configuracion
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

    // Maneja el borrado de todas las partidas con confirmacion
    void OnDeleteAllSavesClicked()
    {
        m_DeleteConfirmationCount++;

        if (m_DeleteConfirmationCount == 1)
        {
            m_DeleteAllSavesButton.text = "Estas seguro? (Click otra vez)";
            m_DeleteAllSavesButton.AddToClassList("settings-delete-btn-confirm");
        }
        else if (m_DeleteConfirmationCount >= 2)
        {
            DeleteAllSaves();
            m_DeleteAllSavesButton.text = "Partidas Borradas!";
            m_DeleteAllSavesButton.SetEnabled(false);

            m_DeleteAllSavesButton.schedule.Execute(() =>
            {
                ResetDeleteButton();
            }).ExecuteLater(2000);
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

    // Resetea el boton de borrado
    void ResetDeleteButton()
    {
        m_DeleteConfirmationCount = 0;
        m_DeleteAllSavesButton.text = "Borrar Todas las Partidas";
        m_DeleteAllSavesButton.RemoveFromClassList("settings-delete-btn-confirm");
        m_DeleteAllSavesButton.SetEnabled(true);
    }

    void OnSettingsBackClicked()
    {
        ResetDeleteButton();
        m_SettingsPanel.style.display = DisplayStyle.None;
    }

    // Carga la configuracion guardada
    void LoadSettings()
    {
        bool isFullscreen = PlayerPrefs.GetInt("Fullscreen", Screen.fullScreen ? 1 : 0) == 1;
        m_FullscreenToggle.value = isFullscreen;

        if (isFullscreen)
        {
            Resolution currentRes = Screen.currentResolution;
            Screen.SetResolution(currentRes.width, currentRes.height, FullScreenMode.FullScreenWindow);
        }

        bool limitFPS = PlayerPrefs.GetInt("LimitFPS", 0) == 1;
        m_LimitFPSToggle.value = limitFPS;

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
        m_ShowFPSToggle.value = showFPS;

        bool muteMusic = PlayerPrefs.GetInt("MuteMusic", 0) == 1;
        m_MuteMusicToggle.value = muteMusic;
        AudioListener.volume = muteMusic ? 0f : 1f;
    }

    // Refresca la lista de partidas guardadas
    void RefreshSaveList(ScrollView container)
    {
        container.Clear();
        string[] files = SaveManager.GetAllSaveFiles();

        if (files.Length == 0)
        {
            var noSavesLabel = new Label("No hay partidas guardadas");
            noSavesLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            noSavesLabel.style.color = Color.gray;
            container.Add(noSavesLabel);
            return;
        }

        foreach (string filePath in files)
        {
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            var btn = new Button { text = fileName };
            btn.AddToClassList("pause-btn");
            btn.clicked += () => OnSaveFileSelected(filePath);
            container.Add(btn);
        }
    }

    // Carga una partida guardada
    void OnSaveFileSelected(string filePath)
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
    private void OnNewGameClicked()
    {
        SceneManager.LoadScene(CharacterCreationSceneName);
    }

    private void OnLoadGameClicked()
    {
        var scroll = m_LoadPanel.Q<ScrollView>();
        if (scroll != null)
        {
            RefreshSaveList(scroll);
        }
        m_LoadPanel.style.display = DisplayStyle.Flex;
    }

    private void OnAchievementsClicked()
    {
        RefreshAchievementsList();
        m_AchievementsPanel.style.display = DisplayStyle.Flex;
    }

    private void OnSettingsClicked()
    {
        m_SettingsPanel.style.display = DisplayStyle.Flex;
    }

    private void OnExitClicked()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
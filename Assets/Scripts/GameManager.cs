using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

// Controlador principal del juego
// Gestiona el flujo del juego, UI, turnos y sistemas principales
public class GameManager : MonoBehaviour
{
    // Singleton y referencias principales
    public static GameManager Instance { get; private set; }
    public UIDocument UIDoc;
    public StyleSheet GameStyles;
    public BoardManager BoardManager;
    public PlayerController PlayerController;
    public Camera MainCamera;

    // Labels del HUD central
    private Label m_LifeLabel;
    private Label m_NameLabel;
    private Label m_AchievementLabel;
    private Label m_SkillPointsLabel;

    // HUD izquierdo - Estadisticas del personaje
    private Label m_StrengthLabel;
    private Label m_AgilityLabel;
    private Label m_IntelligenceLabel;
    private Label m_HealthLabel;

    // HUD derecho - Progreso del jugador
    private Label m_NightLabel;
    private Label m_XPLabel;

    // Variables de estado del juego
    private int m_LifeAmount = 100;
    private int m_CurrentLevel = 1;
    private int m_XPAmount = 0;
    private string m_PlayerName = "Desconocido";

    public TurnManager TurnManager { get; private set; }
    public bool IsPaused { get; private set; }

    // Paneles de UI
    private VisualElement m_PauseOverlay;
    private VisualElement m_SettingsPanel;

    // Panel de Game Over
    private VisualElement m_GameOverPanel;
    private Label m_GameOverDaysLabel;
    private Label m_GameOverXPLabel;
    private Label m_GameOverHeroLabel;
    private VisualElement m_TopPlayersContainer;

    // Toggles de configuracion
    private Toggle m_FullscreenToggle;
    private Toggle m_LimitFPSToggle;
    private Toggle m_ShowFPSToggle;
    private Toggle m_MuteMusicToggle;

    private const int MIN_WINDOW_WIDTH = 800;
    private const int MIN_WINDOW_HEIGHT = 600;

    // Inicializacion del gestor
    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        TurnManager = new TurnManager();
        TurnManager.OnTick += OnTurnHappen;

        var root = UIDoc.rootVisualElement;

        if (GameStyles != null)
        {
            if (!root.styleSheets.Contains(GameStyles))
            {
                root.styleSheets.Add(GameStyles);
            }
        }

        m_LifeLabel = root.Q<Label>("FoodLabel");
        m_NameLabel = root.Q<Label>("NameLabel");
        m_AchievementLabel = root.Q<Label>("AchievementLabel");
        if (m_AchievementLabel != null) m_AchievementLabel.text = "";

        SetupHUD(root);
        SetupPauseMenu(root);
        SetupSettingsPanel(root);
        SetupGameOverPanel(root);
        StartNewGame();
    }

    // Maneja la entrada de teclado para pausa
    private void Update()
    {
        if (m_GameOverPanel.style.display == DisplayStyle.Flex) return;

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (m_SettingsPanel.style.display == DisplayStyle.Flex)
            {
                m_SettingsPanel.style.display = DisplayStyle.None;
                m_PauseOverlay.style.display = DisplayStyle.Flex;
            }
            else if (IsPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    // Crea el HUD con estadisticas y progreso
    void SetupHUD(VisualElement root)
    {
        // Panel izquierdo - Estadisticas
        var leftPanel = new VisualElement();
        leftPanel.AddToClassList("hud-panel-left");

        m_StrengthLabel = new Label("Fuerza: 1");
        m_StrengthLabel.AddToClassList("hud-label");
        m_StrengthLabel.AddToClassList("hud-strength");
        leftPanel.Add(m_StrengthLabel);

        m_AgilityLabel = new Label("Agilidad: 1");
        m_AgilityLabel.AddToClassList("hud-label");
        m_AgilityLabel.AddToClassList("hud-agility");
        leftPanel.Add(m_AgilityLabel);

        m_IntelligenceLabel = new Label("Intelig.: 1");
        m_IntelligenceLabel.AddToClassList("hud-label");
        m_IntelligenceLabel.AddToClassList("hud-intelligence");
        leftPanel.Add(m_IntelligenceLabel);

        m_HealthLabel = new Label("Salud: 1");
        m_HealthLabel.AddToClassList("hud-label");
        m_HealthLabel.AddToClassList("hud-health");
        leftPanel.Add(m_HealthLabel);

        m_SkillPointsLabel = new Label("Puntos: 0");
        m_SkillPointsLabel.AddToClassList("hud-label");
        m_SkillPointsLabel.AddToClassList("hud-skillpoints");
        leftPanel.Add(m_SkillPointsLabel);

        root.Add(leftPanel);

        // Panel derecho - Progreso
        var rightPanel = new VisualElement();
        rightPanel.AddToClassList("hud-panel-right");

        m_NightLabel = new Label("Noche: 1");
        m_NightLabel.AddToClassList("hud-label");
        m_NightLabel.AddToClassList("hud-night");
        rightPanel.Add(m_NightLabel);

        m_XPLabel = new Label("XP: 0");
        m_XPLabel.AddToClassList("hud-label");
        m_XPLabel.AddToClassList("hud-xp");
        rightPanel.Add(m_XPLabel);

        root.Add(rightPanel);
    }

    // Actualiza el contador de puntos de habilidad
    public void UpdateSkillPointsUI()
    {
        if (m_SkillPointsLabel != null && SessionManager.Instance != null)
        {
            m_SkillPointsLabel.text = "Puntos: " + SessionManager.Instance.AvailableSkillPoints;
        }
    }

    // Actualiza todos los elementos del HUD
    void UpdateHUD()
    {
        if (SessionManager.Instance != null && SessionManager.Instance.CurrentPlayerData != null)
        {
            var data = SessionManager.Instance.CurrentPlayerData;

            m_StrengthLabel.text = "Fuerza: " + data.Strength;
            m_AgilityLabel.text = "Agilidad: " + data.Agility;
            m_IntelligenceLabel.text = "Intelig.: " + data.Intelligence;
            m_HealthLabel.text = "Salud: " + data.Health;
        }

        m_NightLabel.text = "Noche: " + m_CurrentLevel;
        m_XPLabel.text = "XP: " + m_XPAmount;
        UpdateSkillPointsUI();
    }

    // Inicia una nueva partida o carga partida guardada
    public void StartNewGame()
    {
        m_GameOverPanel.style.display = DisplayStyle.None;
        if (IsPaused) ResumeGame();

        bool isLevelUp = SessionManager.Instance != null && SessionManager.Instance.IsLevelUp;

        if (isLevelUp)
        {
            m_CurrentLevel = SessionManager.Instance.CurrentNight;
            m_XPAmount = SessionManager.Instance.CurrentXP;
            SessionManager.Instance.IsLevelUp = false;
        }
        else
        {
            m_CurrentLevel = 1;
            m_XPAmount = 0;

            if (SessionManager.Instance != null && !SessionManager.Instance.IsRetrying)
            {
                SessionManager.Instance.AvailableSkillPoints = 0;
                SessionManager.Instance.CurrentXP = 0;
                SessionManager.Instance.BoardWidth = 8;
                SessionManager.Instance.BoardHeight = 8;
                SessionManager.Instance.EnemyHealthBonus = 0;
            }
        }

        if (m_AchievementLabel != null) m_AchievementLabel.text = "";

        m_PlayerName = "Desconocido";

        if (SessionManager.Instance != null && SessionManager.Instance.CurrentPlayerData != null)
        {
            var data = SessionManager.Instance.CurrentPlayerData;
            m_PlayerName = data.Name;

            if (m_NameLabel != null) m_NameLabel.text = "Hero: " + data.Name;

            if (data.SavedLife != -1 && !isLevelUp)
            {
                m_LifeAmount = data.SavedLife;
                m_CurrentLevel = data.SavedNight;

                SessionManager.Instance.AvailableSkillPoints = data.SavedSkillPoints;
                SessionManager.Instance.CurrentNight = data.SavedNight;
                SessionManager.Instance.BoardWidth = data.SavedBoardWidth;
                SessionManager.Instance.BoardHeight = data.SavedBoardHeight;
                SessionManager.Instance.EnemyHealthBonus = data.SavedEnemyHealthBonus;

                if (data.Achievements != null && AchievementManager.Instance != null)
                {
                    AchievementManager.Instance.LoadAchievements(data.Achievements);
                }
            }
            else if (!isLevelUp)
            {
                m_LifeAmount = data.StartingLife;
            }
            else
            {
                m_LifeAmount = data.StartingLife;
            }
        }
        else
        {
            if (m_NameLabel != null) m_NameLabel.text = "Hero: Desconocido";
            m_LifeAmount = 10;
        }

        m_LifeLabel.text = "Vida " + m_PlayerName + ": " + m_LifeAmount;

        if (SessionManager.Instance != null)
        {
            SessionManager.Instance.CurrentNight = m_CurrentLevel;
        }

        // Aplica configuracion de tamano del tablero
        if (SessionManager.Instance != null)
        {
            BoardManager.Width = SessionManager.Instance.BoardWidth;
            BoardManager.Height = SessionManager.Instance.BoardHeight;
        }

        BoardManager.Clean();
        BoardManager.Init();

        PlayerController.Init();
        PlayerController.Spawn(BoardManager, new Vector2Int(1, 1));

        UpdateHUD();
        AdjustCamera();
    }

    // Avanza al siguiente nivel
    public void NewLevel()
    {
        m_CurrentLevel++;

        if (SessionManager.Instance != null)
        {
            SessionManager.Instance.CurrentNight = m_CurrentLevel;
        }

        // Cada 10 niveles va a pantalla de mejora
        if (m_CurrentLevel % 5 == 0 && m_CurrentLevel > 0)
        {
            GoToLevelUpScreen();
            return;
        }

        BoardManager.Clean();
        BoardManager.Init();
        PlayerController.Spawn(BoardManager, new Vector2Int(1, 1));
        UpdateHUD();
        AdjustCamera();
    }

    // Cambia a la pantalla de mejora de nivel
    void GoToLevelUpScreen()
    {
        if (SessionManager.Instance != null)
        {
            SessionManager.Instance.IsLevelUp = true;
            SessionManager.Instance.CurrentNight = m_CurrentLevel;
            SessionManager.Instance.CurrentXP = m_XPAmount;

            // Incrementa el tamano del mapa
            if (Random.value < 0.5f)
            {
                SessionManager.Instance.BoardWidth++;
            }
            else
            {
                SessionManager.Instance.BoardHeight++;
            }

            SessionManager.Instance.EnemyHealthBonus++;

            if (SessionManager.Instance.CurrentPlayerData != null)
            {
                SessionManager.Instance.CurrentPlayerData.SavedLife = m_LifeAmount;
            }
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene("CharacterCreation");
    }

    // Cada turno reduce la vida del jugador
    void OnTurnHappen() { ChangeLife(-1); }

    // Modifica la vida del jugador
    public void ChangeLife(int amount)
    {
        m_LifeAmount += amount;

        if (m_LifeAmount < 0)
        {
            m_LifeAmount = 0;
        }

        m_LifeLabel.text = "Vida " + m_PlayerName + ": " + m_LifeAmount;

        if (m_LifeAmount <= 0)
        {
            PlayerController.GameOver();
            SaveStatsOnDeath();
            ShowGameOver();
        }
    }

    public void ChangeFood(int amount)
    {
        ChangeLife(amount);
    }

    public void ChangeXP(int amount)
    {
        m_XPAmount += amount;
        UpdateHUD();
    }

    // Crea el panel de Game Over
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

        var exitBtn = new Button { text = "Salir al Menu" };
        exitBtn.AddToClassList("gameover-btn");
        exitBtn.AddToClassList("gameover-btn-exit");
        exitBtn.clicked += OnExitClicked;
        btnContainer.Add(exitBtn);

        var retryBtn = new Button { text = "Reintentar" };
        retryBtn.AddToClassList("gameover-btn");
        retryBtn.AddToClassList("gameover-btn-retry");
        retryBtn.clicked += OnRetryClicked;
        btnContainer.Add(retryBtn);

        panel.Add(btnContainer);
        m_GameOverPanel.Add(panel);
        root.Add(m_GameOverPanel);
    }

    // Guarda las estadisticas del jugador al morir
    void SaveStatsOnDeath()
    {
        if (SessionManager.Instance != null && SessionManager.Instance.CurrentPlayerData != null)
        {
            PlayerData data = SessionManager.Instance.CurrentPlayerData;

            if (m_CurrentLevel > data.BestLevel)
            {
                data.BestLevel = m_CurrentLevel;
            }
            if (m_XPAmount > data.BestXP)
            {
                data.BestXP = m_XPAmount;
            }

            if (AchievementManager.Instance != null)
            {
                data.Achievements = AchievementManager.Instance.Logros;
            }

            data.SavedLife = -1;
            data.SavedSkillPoints = 0;
            data.SavedNight = 1;
            data.SavedBoardWidth = 8;
            data.SavedBoardHeight = 8;
            data.SavedEnemyHealthBonus = 0;

            SaveManager.SavePlayerData(data);
        }
    }

    // Muestra el panel de Game Over con estadisticas
    void ShowGameOver()
    {
        m_GameOverHeroLabel.text = m_PlayerName;
        m_GameOverDaysLabel.text = $"Noches sobrevividas: {m_CurrentLevel}";
        m_GameOverXPLabel.text = $"XP conseguido: {m_XPAmount}";

        LoadTop5Players();

        m_GameOverPanel.style.display = DisplayStyle.Flex;
    }

    // Carga y muestra el top 5 de jugadores
    void LoadTop5Players()
    {
        m_TopPlayersContainer.Clear();

        string[] files = SaveManager.GetAllSaveFiles();
        List<(string name, int level, int xp)> players = new List<(string, int, int)>();

        foreach (string filePath in files)
        {
            string fileName = Path.GetFileName(filePath);
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
            var noDataLabel = new Label("No hay records todavia");
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

    // Reinicia con nueva asignacion de puntos
    void OnRetryClicked()
    {
        if (SessionManager.Instance != null)
        {
            SessionManager.Instance.IsRetrying = true;
            SessionManager.Instance.AvailableSkillPoints = 0;
            SessionManager.Instance.CurrentNight = 1;
            SessionManager.Instance.CurrentXP = 0;
            SessionManager.Instance.BoardWidth = 8;
            SessionManager.Instance.BoardHeight = 8;
            SessionManager.Instance.EnemyHealthBonus = 0;
        }
        Time.timeScale = 1f;
        SceneManager.LoadScene("CharacterCreation");
    }

    // Vuelve al menu principal
    void OnExitClicked()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }

    // Muestra notificacion de logro conseguido
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
                m_AchievementLabel.text = "Logro!\n\n" + message;
                m_AchievementLabel.style.color = new StyleColor(new Color(1f, 0.84f, 0f));
            }

            StopCoroutine("HideAchievementLabel");
            StartCoroutine("HideAchievementLabel");
        }
    }

    // Muestra notificacion de XP ganado
    public void ShowXPNotification(string message)
    {
        if (m_AchievementLabel != null)
        {
            m_AchievementLabel.text = message;
            m_AchievementLabel.style.color = new StyleColor(new Color(0.4f, 0.9f, 0.4f));

            StopCoroutine("HideAchievementLabel");
            StartCoroutine("HideAchievementLabel");
        }
    }

    // Muestra notificacion de esquive
    public void ShowDodgeNotification(string message)
    {
        if (m_AchievementLabel != null)
        {
            m_AchievementLabel.text = message;
            m_AchievementLabel.style.color = new StyleColor(new Color(0.6f, 0.8f, 1f));

            StopCoroutine("HideAchievementLabel");
            StartCoroutine("HideAchievementLabel");
        }
    }

    // Oculta las notificaciones despues de 3 segundos
    private IEnumerator HideAchievementLabel()
    {
        yield return new WaitForSeconds(3f);
        if (m_AchievementLabel != null)
        {
            m_AchievementLabel.text = "";
        }
    }

    // Ajusta la camara al tamano del tablero
    private void AdjustCamera()
    {
        if (MainCamera == null) return;
        float aspect = MainCamera.aspect;
        float bottomMargin = 1.0f;
        float sizeNeededForWidth = (BoardManager.Width / aspect) / 2f;
        float sizeNeededForHeight = (BoardManager.Height + bottomMargin) / 2f;
        MainCamera.orthographicSize = Mathf.Max(sizeNeededForWidth, sizeNeededForHeight);
        float centerX = BoardManager.Width / 2f;
        float centerY = MainCamera.orthographicSize - bottomMargin;
        MainCamera.transform.position = new Vector3(centerX, centerY, MainCamera.transform.position.z);
    }

    // Crea el menu de pausa
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
        continueBtn.clicked += ResumeGame;
        panel.Add(continueBtn);

        var settingsBtn = new Button { text = "Configuracion" };
        settingsBtn.AddToClassList("pause-btn");
        settingsBtn.clicked += OpenSettingsFromPause;
        panel.Add(settingsBtn);

        var exitBtn = new Button { text = "Guardar y Salir" };
        exitBtn.AddToClassList("pause-btn");
        exitBtn.AddToClassList("pause-btn-exit");
        exitBtn.clicked += SaveAndExit;
        panel.Add(exitBtn);

        m_PauseOverlay.Add(panel);
        root.Add(m_PauseOverlay);
    }

    // Crea el panel de configuracion
    void SetupSettingsPanel(VisualElement root)
    {
        m_SettingsPanel = new VisualElement();
        m_SettingsPanel.AddToClassList("pause-overlay");
        m_SettingsPanel.style.display = DisplayStyle.None;

        var panel = new VisualElement();
        panel.AddToClassList("pause-panel");
        panel.style.width = 450;

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

        var backBtn = new Button { text = "Volver" };
        backBtn.AddToClassList("pause-btn");
        backBtn.AddToClassList("pause-btn-exit");
        backBtn.clicked += CloseSettingsFromPause;
        panel.Add(backBtn);

        m_SettingsPanel.Add(panel);
        root.Add(m_SettingsPanel);

        LoadSettingsValues();
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

    // Carga los valores guardados de configuracion
    void LoadSettingsValues()
    {
        m_FullscreenToggle.value = PlayerPrefs.GetInt("Fullscreen", Screen.fullScreen ? 1 : 0) == 1;
        m_LimitFPSToggle.value = PlayerPrefs.GetInt("LimitFPS", 0) == 1;
        m_ShowFPSToggle.value = PlayerPrefs.GetInt("ShowFPS", 0) == 1;
        m_MuteMusicToggle.value = PlayerPrefs.GetInt("MuteMusic", 0) == 1;
    }

    void OpenSettingsFromPause()
    {
        m_PauseOverlay.style.display = DisplayStyle.None;
        m_SettingsPanel.style.display = DisplayStyle.Flex;
        LoadSettingsValues();
    }

    void CloseSettingsFromPause()
    {
        m_SettingsPanel.style.display = DisplayStyle.None;
        m_PauseOverlay.style.display = DisplayStyle.Flex;
    }

    // Cambia entre pantalla completa y ventana
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

    // Limita o no los FPS a 60
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

    // Muestra u oculta el contador de FPS
    void OnShowFPSChanged(ChangeEvent<bool> evt)
    {
        if (FPSCounter.Instance != null)
        {
            FPSCounter.Instance.SetShowFPS(evt.newValue);
        }
    }

    // Silencia o activa la musica
    void OnMuteMusicChanged(ChangeEvent<bool> evt)
    {
        AudioListener.volume = evt.newValue ? 0f : 1f;
        PlayerPrefs.SetInt("MuteMusic", evt.newValue ? 1 : 0);
        PlayerPrefs.Save();
    }

    // Pausa el juego
    public void PauseGame()
    {
        IsPaused = true;
        m_PauseOverlay.style.display = DisplayStyle.Flex;
        Time.timeScale = 0f;
    }

    // Reanuda el juego
    public void ResumeGame()
    {
        IsPaused = false;
        m_PauseOverlay.style.display = DisplayStyle.None;
        m_SettingsPanel.style.display = DisplayStyle.None;
        Time.timeScale = 1f;
    }

    // Guarda la partida y sale al menu
    private void SaveAndExit()
    {
        if (SessionManager.Instance != null && SessionManager.Instance.CurrentPlayerData != null)
        {
            PlayerData dataToSave = SessionManager.Instance.CurrentPlayerData;
            dataToSave.SavedLife = m_LifeAmount;
            dataToSave.SavedSkillPoints = SessionManager.Instance.AvailableSkillPoints;
            dataToSave.SavedNight = m_CurrentLevel;
            dataToSave.SavedBoardWidth = SessionManager.Instance.BoardWidth;
            dataToSave.SavedBoardHeight = SessionManager.Instance.BoardHeight;
            dataToSave.SavedEnemyHealthBonus = SessionManager.Instance.EnemyHealthBonus;

            if (AchievementManager.Instance != null)
            {
                dataToSave.Achievements = AchievementManager.Instance.Logros;
            }

            SaveManager.SavePlayerData(dataToSave);
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }
}
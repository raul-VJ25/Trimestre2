using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// Controlador principal del juego - ACTÚA SOLO COMO COORDINADOR
// Delega toda la lógica a los sistemas especializados
public class GameManager : MonoBehaviour
{
    // Singleton y referencias principales
    public static GameManager Instance { get; private set; }
    public BoardManager BoardManager;
    public PlayerController PlayerController;
    public Camera MainCamera;

    // Referencias a los sistemas
    private LifeSystem m_LifeSystem;
    private XPSystem m_XPSystem;
    private LevelSystem m_LevelSystem;
    private PauseSystem m_PauseSystem;

    // Propiedades que delegan a los sistemas
    public TurnManager TurnManager { get; private set; }
    public bool IsPaused => m_PauseSystem != null && m_PauseSystem.IsPaused;
    public int CurrentLife => m_LifeSystem != null ? m_LifeSystem.CurrentLife : 0;
    public int CurrentXP => m_XPSystem != null ? m_XPSystem.CurrentXP : 0;
    public int CurrentLevel => m_LevelSystem != null ? m_LevelSystem.CurrentLevel : 1;

    private string m_PlayerName = "Desconocido";

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        // Obtiene referencias a los sistemas
        m_LifeSystem = GetComponent<LifeSystem>();
        m_XPSystem = GetComponent<XPSystem>();
        m_LevelSystem = GetComponent<LevelSystem>();
        m_PauseSystem = GetComponent<PauseSystem>();

        // Inicializa TurnManager y suscribe eventos
        TurnManager = new TurnManager();
        TurnManager.OnTick += OnTurnHappen;
        m_LifeSystem.OnPlayerDeath += HandlePlayerDeath;

        // CORRECCIÓN HUD: Actualiza el HUD automáticamente cuando el LifeSystem detecta un cambio de vida
        m_LifeSystem.OnLifeChanged += (life) => UpdateHUD();

        // CORRECCIÓN MENÚ DE PAUSA: Suscribe la UI a los eventos del sistema de pausa
        m_PauseSystem.OnGamePaused += () => { if (UIGameManager.Instance != null) UIGameManager.Instance.ShowPauseMenu(); };
        m_PauseSystem.OnGameResumed += () => { if (UIGameManager.Instance != null) UIGameManager.Instance.HidePauseMenu(); };

        StartNewGame();
    }

    // Inicia una nueva partida o carga partida guardada
    public void StartNewGame()
    {
        if (UIGameManager.Instance != null) UIGameManager.Instance.HideGameOverPanel();

        bool isLevelUp = SessionManager.Instance != null && SessionManager.Instance.IsLevelUp;

        if (isLevelUp)
        {
            if (m_LevelSystem != null) m_LevelSystem.Init(SessionManager.Instance.CurrentNight);
            if (m_XPSystem != null) m_XPSystem.Init(SessionManager.Instance.CurrentXP);
            SessionManager.Instance.IsLevelUp = false;
        }
        else
        {
            if (SessionManager.Instance != null && !SessionManager.Instance.IsRetrying)
            {
                SessionManager.Instance.AvailableSkillPoints = 0;
                SessionManager.Instance.CurrentXP = 0;
                SessionManager.Instance.BoardWidth = 8;
                SessionManager.Instance.BoardHeight = 8;
                SessionManager.Instance.EnemyHealthBonus = 0;
            }
            if (m_LevelSystem != null) m_LevelSystem.Init(1);
            if (m_XPSystem != null) m_XPSystem.Init(0);
        }

        m_PlayerName = "Desconocido";
        if (SessionManager.Instance != null && SessionManager.Instance.CurrentPlayerData != null)
        {
            var data = SessionManager.Instance.CurrentPlayerData;
            m_PlayerName = data.Name;

            if (data.SavedLife != -1 && !isLevelUp)
            {
                if (m_LifeSystem != null) m_LifeSystem.Init(data.SavedLife);
                if (m_LevelSystem != null) m_LevelSystem.Init(data.SavedNight);
                SessionManager.Instance.AvailableSkillPoints = data.SavedSkillPoints;
                SessionManager.Instance.CurrentNight = data.SavedNight;
                SessionManager.Instance.BoardWidth = data.SavedBoardWidth;
                SessionManager.Instance.BoardHeight = data.SavedBoardHeight;
                SessionManager.Instance.EnemyHealthBonus = data.SavedEnemyHealthBonus;
                if (data.Achievements != null && AchievementManager.Instance != null)
                    AchievementManager.Instance.LoadAchievements(data.Achievements);
            }
            else
            {
                if (m_LifeSystem != null) m_LifeSystem.Init(data.StartingLife);
            }
        }
        else
        {
            if (m_LifeSystem != null) m_LifeSystem.Init(10);
        }

        if (SessionManager.Instance != null) SessionManager.Instance.CurrentNight = CurrentLevel;

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

    // Regenera el tablero al avanzar de nivel (llamado por LevelSystem)
    public void RefreshLevel()
    {
        BoardManager.Clean();
        BoardManager.Init();
        PlayerController.Spawn(BoardManager, new Vector2Int(1, 1));
        UpdateHUD();
        AdjustCamera();
    }

    public void NewLevel()
    {
        if (m_LevelSystem != null) m_LevelSystem.AdvanceLevel();
    }

    void OnTurnHappen()
    {
        if (m_LifeSystem != null) m_LifeSystem.OnTurnTick();
    }

    // Maneja la muerte del jugador (llamado por el evento de LifeSystem)
    private void HandlePlayerDeath()
    {
        SaveStatsOnDeath();
        ShowGameOver();
    }

    public void ChangeLife(int amount)
    {
        if (m_LifeSystem != null) m_LifeSystem.ChangeLife(amount);
    }

    public void ChangeFood(int amount) => ChangeLife(amount);

    public void ChangeXP(int amount)
    {
        if (m_XPSystem != null) m_XPSystem.ChangeXP(amount);
        UpdateHUD();
    }

    void UpdateHUD()
    {
        if (UIGameManager.Instance != null)
            UIGameManager.Instance.UpdateHUD(m_PlayerName, CurrentLife, CurrentLevel, CurrentXP);
    }

    public void UpdateSkillPointsUI()
    {
        if (UIGameManager.Instance != null) UIGameManager.Instance.UpdateSkillPointsUI();
    }

    void SaveStatsOnDeath()
    {
        if (SessionManager.Instance != null && SessionManager.Instance.CurrentPlayerData != null)
        {
            PlayerData data = SessionManager.Instance.CurrentPlayerData;
            if (CurrentLevel > data.BestLevel) data.BestLevel = CurrentLevel;
            if (CurrentXP > data.BestXP) data.BestXP = CurrentXP;
            if (AchievementManager.Instance != null) data.Achievements = AchievementManager.Instance.Logros;

            data.SavedLife = -1;
            data.SavedSkillPoints = 0;
            data.SavedNight = 1;
            data.SavedBoardWidth = 8;
            data.SavedBoardHeight = 8;
            data.SavedEnemyHealthBonus = 0;
            SaveManager.SavePlayerData(data);
        }
    }

    void ShowGameOver()
    {
        if (UIGameManager.Instance != null)
            UIGameManager.Instance.ShowGameOver(m_PlayerName, CurrentLevel, CurrentXP);
    }

    public void OnRetryClicked()
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

    public void OnExitClicked()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }

    public void ShowAchievementNotification(string message, bool isSkillPoint = false)
    {
        if (UIGameManager.Instance != null) UIGameManager.Instance.ShowAchievementNotification(message, isSkillPoint);
    }

    public void ShowXPNotification(string message)
    {
        if (UIGameManager.Instance != null) UIGameManager.Instance.ShowXPNotification(message);
    }

    public void ShowDodgeNotification(string message)
    {
        if (UIGameManager.Instance != null) UIGameManager.Instance.ShowDodgeNotification(message);
    }

    // Ajusta la cámara al tamaño del tablero
    public void AdjustCamera()
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

    // Delega la pausa al sistema
    public void PauseGame()
    {
        if (m_PauseSystem != null) m_PauseSystem.PauseGame();
    }

    // Delega la reanudación al sistema
    public void ResumeGame()
    {
        if (m_PauseSystem != null) m_PauseSystem.ResumeGame();
    }

    public void SaveAndExit()
    {
        if (SessionManager.Instance != null && SessionManager.Instance.CurrentPlayerData != null)
        {
            PlayerData dataToSave = SessionManager.Instance.CurrentPlayerData;
            dataToSave.SavedLife = CurrentLife;
            dataToSave.SavedSkillPoints = SessionManager.Instance.AvailableSkillPoints;
            dataToSave.SavedNight = CurrentLevel;
            dataToSave.SavedBoardWidth = SessionManager.Instance.BoardWidth;
            dataToSave.SavedBoardHeight = SessionManager.Instance.BoardHeight;
            dataToSave.SavedEnemyHealthBonus = SessionManager.Instance.EnemyHealthBonus;
            if (AchievementManager.Instance != null) dataToSave.Achievements = AchievementManager.Instance.Logros;
            SaveManager.SavePlayerData(dataToSave);
        }
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }
}
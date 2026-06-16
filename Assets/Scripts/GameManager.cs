using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// Controlador principal del juego
// Gestiona el flujo del juego, turnos y sistemas principales
// NOTA: Toda la UI ha sido delegada a UIGameManager
public class GameManager : MonoBehaviour
{
    // Singleton y referencias principales
    public static GameManager Instance { get; private set; }
    public BoardManager BoardManager;
    public PlayerController PlayerController;
    public Camera MainCamera;

    // Variables de estado del juego
    private int m_LifeAmount = 100;
    private int m_CurrentLevel = 1;
    private int m_XPAmount = 0;
    private string m_PlayerName = "Desconocido";

    public TurnManager TurnManager { get; private set; }
    public bool IsPaused { get; private set; }

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

        StartNewGame();
    }

    // Maneja la entrada de teclado para pausa
    private void Update()
    {
        // Verificar si el panel de Game Over está visible a través de UIGameManager
        if (UIGameManager.Instance != null)
        {
            // Si el panel de game over está activo, no procesar pausa
            // (esto se maneja en UIGameManager)
        }

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (IsPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    // Actualiza el contador de puntos de habilidad
    public void UpdateSkillPointsUI()
    {
        if (UIGameManager.Instance != null)
        {
            UIGameManager.Instance.UpdateSkillPointsUI();
        }
    }

    // Inicia una nueva partida o carga partida guardada
    public void StartNewGame()
    {
        // Ocultar panel de game over
        if (UIGameManager.Instance != null)
        {
            UIGameManager.Instance.HideGameOverPanel();
        }

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

        m_PlayerName = "Desconocido";

        if (SessionManager.Instance != null && SessionManager.Instance.CurrentPlayerData != null)
        {
            var data = SessionManager.Instance.CurrentPlayerData;
            m_PlayerName = data.Name;

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
            m_LifeAmount = 10;
        }

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

        // Cada 5 niveles va a pantalla de mejora
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

        if (m_LifeAmount <= 0)
        {
            PlayerController.GameOver();
            SaveStatsOnDeath();
            ShowGameOver();
        }

        // Actualizar UI a través de UIGameManager
        if (UIGameManager.Instance != null)
        {
            UIGameManager.Instance.UpdateHUD(m_PlayerName, m_LifeAmount, m_CurrentLevel, m_XPAmount);
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

    // Actualiza todos los elementos del HUD
    void UpdateHUD()
    {
        if (UIGameManager.Instance != null)
        {
            UIGameManager.Instance.UpdateHUD(m_PlayerName, m_LifeAmount, m_CurrentLevel, m_XPAmount);
        }
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
        if (UIGameManager.Instance != null)
        {
            UIGameManager.Instance.ShowGameOver(m_PlayerName, m_CurrentLevel, m_XPAmount);
        }
    }

    // Reinicia con nueva asignacion de puntos
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

    // Vuelve al menu principal
    public void OnExitClicked()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }

    // Muestra notificacion de logro conseguido
    public void ShowAchievementNotification(string message, bool isSkillPoint = false)
    {
        if (UIGameManager.Instance != null)
        {
            UIGameManager.Instance.ShowAchievementNotification(message, isSkillPoint);
        }
    }

    // Muestra notificacion de XP ganado
    public void ShowXPNotification(string message)
    {
        if (UIGameManager.Instance != null)
        {
            UIGameManager.Instance.ShowXPNotification(message);
        }
    }

    // Muestra notificacion de esquive
    public void ShowDodgeNotification(string message)
    {
        if (UIGameManager.Instance != null)
        {
            UIGameManager.Instance.ShowDodgeNotification(message);
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

    // Pausa el juego
    public void PauseGame()
    {
        IsPaused = true;

        if (UIGameManager.Instance != null)
        {
            UIGameManager.Instance.ShowPauseMenu();
        }

        Time.timeScale = 0f;
    }

    // Reanuda el juego
    public void ResumeGame()
    {
        IsPaused = false;

        if (UIGameManager.Instance != null)
        {
            UIGameManager.Instance.HidePauseMenu();
        }

        Time.timeScale = 1f;
    }

    // Guarda la partida y sale al menu
    public void SaveAndExit()
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
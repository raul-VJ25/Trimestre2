using UnityEngine;

// Gestor de sesion persistente entre escenas
// Mantiene datos del jugador y configuracion de partida
public class SessionManager : MonoBehaviour
{
    public static SessionManager Instance { get; private set; }

    // Datos de la sesion actual
    public PlayerData CurrentPlayerData { get; set; }
    public bool IsRetrying { get; set; } = false;
    public bool IsLevelUp { get; set; } = false;
    public int AvailableSkillPoints { get; set; } = 0;
    public int CurrentNight { get; set; } = 1;
    public int CurrentXP { get; set; } = 0;
    public int BoardWidth { get; set; } = 8;
    public int BoardHeight { get; set; } = 8;
    public int EnemyHealthBonus { get; set; } = 0;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Resetea todos los datos para nueva partida
    public void ResetForNewGame()
    {
        AvailableSkillPoints = 0;
        CurrentNight = 1;
        CurrentXP = 0;
        IsRetrying = false;
        IsLevelUp = false;
        BoardWidth = 8;
        BoardHeight = 8;
        EnemyHealthBonus = 0;
    }
}
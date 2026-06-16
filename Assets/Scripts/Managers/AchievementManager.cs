using System.Collections.Generic;
using UnityEngine;

// Sistema de logros del juego
// Gestiona la creacion, progreso y completado de achievements
public class AchievementManager : MonoBehaviour
{
    // Singleton para acceso global
    public static AchievementManager Instance { get; private set; }

    // Lista de todos los logros del juego
    public List<Achievement> Logros;

    // Inicializacion del singleton
    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        if (Logros == null || Logros.Count == 0)
        {
            InitializeDefaultAchievements();
        }
    }

    // Crea los logros predeterminados del juego
    private void InitializeDefaultAchievements()
    {
        Logros = new List<Achievement>();

        Achievement Logro1 = new Achievement("KILL", "Exterminador", "La que has liao", 100);
        Logros.Add(Logro1);
        Logro1 = new Achievement("KILL", "loco", "para ya", 1000);
        Logros.Add(Logro1);
        Logro1 = new Achievement("KILL", "Homicidio", "Primeras 5 bajas", 5);
        Logros.Add(Logro1);

        Achievement Logro2 = new Achievement("BREAK", "Picapiedra", "No mas muros", 100);
        Logros.Add(Logro2);

        Achievement Logro3 = new Achievement("EAT", "LLuvia de albondigas", "Pero si son hamburguesas", 100);
        Logros.Add(Logro3);

        Achievement Logro4 = new Achievement("WALK", "Strava", "Pesao", 1000);
        Logros.Add(Logro4);
        Logro4 = new Achievement("WALK", "Inicio", "Primeros 20 pasos", 20);
        Logros.Add(Logro4);
    }

    // Carga logros guardados desde un archivo
    public void LoadAchievements(List<Achievement> savedAchievements)
    {
        if (savedAchievements == null) return;

        InitializeDefaultAchievements();

        for (int i = 0; i < Logros.Count; i++)
        {
            var saved = savedAchievements.Find(x => x.ID == Logros[i].ID && x.Name == Logros[i].Name);
            if (saved != null)
            {
                Logros[i].CurrentAmount = saved.CurrentAmount;
                Logros[i].completed = saved.completed;
            }
        }
    }

    // Suscripcion a eventos del juego
    private void OnEnable()
    {
        GameEvents.OnEnemyKilled += HandleEnemyKill;
        GameEvents.OnWallDestroyed += HandleWallDestroy;
        GameEvents.OnFoodPicked += HandleFoodPick;
        GameEvents.OnPlayerStep += HandlePlayerStep;
    }

    // Desuscripcion de eventos
    private void OnDisable()
    {
        GameEvents.OnEnemyKilled -= HandleEnemyKill;
        GameEvents.OnWallDestroyed -= HandleWallDestroy;
        GameEvents.OnFoodPicked -= HandleFoodPick;
        GameEvents.OnPlayerStep -= HandlePlayerStep;
    }

    // Actualiza y verifica el progreso de logros
    public void CheckAchievement(string IDAchievement, int amount = 1)
    {
        foreach (Achievement l in Logros)
        {
            if (l.ID == IDAchievement && l.completed == false)
            {
                l.CurrentAmount += amount;

                if (l.CurrentAmount >= l.AmountToAchieve)
                {
                    Debug.Log(l.Name + ": " + l.Description);
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.ShowAchievementNotification(l.Name + ": " + l.Description);
                    }
                    l.completed = true;
                }
            }
        }
    }

    // Handlers para eventos especificos
    private void HandleEnemyKill() { CheckAchievement("KILL"); }
    private void HandleWallDestroy() { CheckAchievement("BREAK"); }
    private void HandleFoodPick(int amount) { CheckAchievement("EAT", amount); }
    private void HandlePlayerStep() { CheckAchievement("WALK"); }
}
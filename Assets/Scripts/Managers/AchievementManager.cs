using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

// Sistema de logros del juego
public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance { get; private set; }

    [FormerlySerializedAs("Logros")][SerializeField] private List<Achievement> m_Logros;
    public List<Achievement> Logros => m_Logros;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        if (m_Logros == null || m_Logros.Count == 0)
        {
            InitializeDefaultAchievements();
        }
    }

    private void InitializeDefaultAchievements()
    {
        m_Logros = new List<Achievement>();
        m_Logros.Add(new Achievement(AchievementType.KILL.ToString(), "Exterminador", "La que has liao", 100));
        m_Logros.Add(new Achievement(AchievementType.KILL.ToString(), "Loco", "Para ya", 1000));
        m_Logros.Add(new Achievement(AchievementType.KILL.ToString(), "Homicidio", "Primeras 5 bajas", 5));
        m_Logros.Add(new Achievement(AchievementType.BREAK.ToString(), "Picapiedra", "No mas muros", 100));
        m_Logros.Add(new Achievement(AchievementType.EAT.ToString(), "Lluvia de albondigas", "Pero si son hamburguesas", 100));
        m_Logros.Add(new Achievement(AchievementType.WALK.ToString(), "Strava", "Pesao", 1000));
        m_Logros.Add(new Achievement(AchievementType.WALK.ToString(), "Inicio", "Primeros 20 pasos", 20));
    }

    public void LoadAchievements(List<Achievement> savedAchievements)
    {
        if (savedAchievements == null) return;
        InitializeDefaultAchievements();
        for (int i = 0; i < m_Logros.Count; i++)
        {
            var saved = savedAchievements.Find(x => x.ID == m_Logros[i].ID && x.Name == m_Logros[i].Name);
            if (saved != null)
            {
                m_Logros[i].CurrentAmount = saved.CurrentAmount;
                m_Logros[i].Completed = saved.Completed;
            }
        }
    }

    private void OnEnable()
    {
        GameEvents.OnEnemyKilled += HandleEnemyKill;
        GameEvents.OnWallDestroyed += HandleWallDestroy;
        GameEvents.OnFoodPicked += HandleFoodPick;
        GameEvents.OnPlayerStep += HandlePlayerStep;
    }

    private void OnDisable()
    {
        GameEvents.OnEnemyKilled -= HandleEnemyKill;
        GameEvents.OnWallDestroyed -= HandleWallDestroy;
        GameEvents.OnFoodPicked -= HandleFoodPick;
        GameEvents.OnPlayerStep -= HandlePlayerStep;
    }

    public void CheckAchievement(AchievementType type, int amount = 1)
    {
        string typeId = type.ToString();
        foreach (Achievement l in m_Logros)
        {
            if (l.ID == typeId && !l.Completed)
            {
                l.CurrentAmount += amount;
                if (l.CurrentAmount >= l.AmountToAchieve)
                {
                    Debug.Log(l.Name + ": " + l.Description);
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.ShowAchievementNotification(l.Name + ": " + l.Description);
                    }
                    l.Completed = true;
                }
            }
        }
    }

    private void HandleEnemyKill() { CheckAchievement(AchievementType.KILL); }
    private void HandleWallDestroy() { CheckAchievement(AchievementType.BREAK); }
    private void HandleFoodPick(int amount) { CheckAchievement(AchievementType.EAT, amount); }
    private void HandlePlayerStep() { CheckAchievement(AchievementType.WALK); }
}
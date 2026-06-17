using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class PlayerData
{
    [Header("Estadisticas Base")]
    [FormerlySerializedAs("Name")][SerializeField] private string m_Name;
    [FormerlySerializedAs("Strength")][SerializeField] private int m_Strength;
    [FormerlySerializedAs("Agility")][SerializeField] private int m_Agility;
    [FormerlySerializedAs("Intelligence")][SerializeField] private int m_Intelligence;
    [FormerlySerializedAs("Health")][SerializeField] private int m_Health;

    [Header("Datos de Partida Guardada")]
    [FormerlySerializedAs("SavedLife")][SerializeField] private int m_SavedLife;
    [FormerlySerializedAs("SavedSkillPoints")][SerializeField] private int m_SavedSkillPoints;
    [FormerlySerializedAs("SavedNight")][SerializeField] private int m_SavedNight;
    [FormerlySerializedAs("SavedBoardWidth")][SerializeField] private int m_SavedBoardWidth;
    [FormerlySerializedAs("SavedBoardHeight")][SerializeField] private int m_SavedBoardHeight;
    [FormerlySerializedAs("SavedEnemyHealthBonus")][SerializeField] private int m_SavedEnemyHealthBonus;
    [FormerlySerializedAs("Achievements")][SerializeField] private List<Achievement> m_Achievements;

    [Header("Records")]
    [FormerlySerializedAs("BestLevel")][SerializeField] private int m_BestLevel = 0;
    [FormerlySerializedAs("BestXP")][SerializeField] private int m_BestXP = 0;

    public string Name { get => m_Name; set => m_Name = value; }
    public int Strength { get => m_Strength; set => m_Strength = value; }
    public int Agility { get => m_Agility; set => m_Agility = value; }
    public int Intelligence { get => m_Intelligence; set => m_Intelligence = value; }
    public int Health { get => m_Health; set => m_Health = value; }
    public int SavedLife { get => m_SavedLife; set => m_SavedLife = value; }
    public int SavedSkillPoints { get => m_SavedSkillPoints; set => m_SavedSkillPoints = value; }
    public int SavedNight { get => m_SavedNight; set => m_SavedNight = value; }
    public int SavedBoardWidth { get => m_SavedBoardWidth; set => m_SavedBoardWidth = value; }
    public int SavedBoardHeight { get => m_SavedBoardHeight; set => m_SavedBoardHeight = value; }
    public int SavedEnemyHealthBonus { get => m_SavedEnemyHealthBonus; set => m_SavedEnemyHealthBonus = value; }
    public List<Achievement> Achievements { get => m_Achievements; set => m_Achievements = value; }
    public int BestLevel { get => m_BestLevel; set => m_BestLevel = value; }
    public int BestXP { get => m_BestXP; set => m_BestXP = value; }

    public int StartingLife => 35 + (m_Health * 5);
    public int BonusDamageToEnemies => m_Strength / 5;
    public int BonusDamageToWalls => m_Agility / 5;

    public float DodgeChance
    {
        get
        {
            float chance = (m_Intelligence / 5) * 0.05f;
            return Mathf.Min(chance, 0.9f);
        }
    }

    public PlayerData(string name, int strength, int agility, int intelligence, int health)
    {
        m_Name = name;
        m_Strength = strength;
        m_Agility = agility;
        m_Intelligence = intelligence;
        m_Health = health;
        m_SavedLife = -1;
        m_SavedSkillPoints = 0;
        m_SavedNight = 1;
        m_SavedBoardWidth = 8;
        m_SavedBoardHeight = 8;
        m_SavedEnemyHealthBonus = 0;
        m_Achievements = null;
        m_BestLevel = 0;
        m_BestXP = 0;
    }

    public PlayerData() { }
}
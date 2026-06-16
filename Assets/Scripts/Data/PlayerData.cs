using System;
using System.Collections.Generic;
using UnityEngine;

// Datos del personaje del jugador
// Almacena estadisticas, progreso y logros
[Serializable]
public class PlayerData
{
    // Estadisticas base del personaje
    public string Name;
    public int Strength;
    public int Agility;
    public int Intelligence;
    public int Health;

    // Datos de partida guardada
    public int SavedLife;
    public int SavedSkillPoints;
    public int SavedNight;
    public int SavedBoardWidth;
    public int SavedBoardHeight;
    public int SavedEnemyHealthBonus;
    public List<Achievement> Achievements;

    // Records del jugador
    public int BestLevel = 0;
    public int BestXP = 0;

    // Vida inicial calculada segun salud
    public int StartingLife
    {
        get { return 35 + (Health * 5); }
    }

    // Dano extra a enemigos por fuerza
    public int BonusDamageToEnemies
    {
        get { return Strength / 5; }
    }

    // Dano extra a muros por agilidad
    public int BonusDamageToWalls
    {
        get { return Agility / 5; }
    }

    // Probabilidad de esquivar ataques por inteligencia
    public float DodgeChance
    {
        get
        {
            float chance = (Intelligence / 5) * 0.05f;
            return Mathf.Min(chance, 0.9f);
        }
    }

    // Constructor completo
    public PlayerData(string name, int strength, int agility, int intelligence, int health)
    {
        Name = name;
        Strength = strength;
        Agility = agility;
        Intelligence = intelligence;
        Health = health;
        SavedLife = -1;
        SavedSkillPoints = 0;
        SavedNight = 1;
        SavedBoardWidth = 8;
        SavedBoardHeight = 8;
        SavedEnemyHealthBonus = 0;
        Achievements = null;
        BestLevel = 0;
        BestXP = 0;
    }

    // Constructor vacio para deserializacion
    public PlayerData() { }
}
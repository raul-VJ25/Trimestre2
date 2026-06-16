using System;
using UnityEngine;

[Serializable]
public class Achievement
{
    public string ID;
    public string Name;
    public string Description;
    public int AmountToAchieve;
    public int CurrentAmount;
    public bool completed;

    public Achievement(string iD, string name, string description, int amountToAchieve)
    {
        ID = iD;
        Name = name;
        Description = description;
        AmountToAchieve = amountToAchieve;
        CurrentAmount = 0;
        this.completed = false;
    }
}
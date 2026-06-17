using System;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class Achievement
{
    [FormerlySerializedAs("ID")][SerializeField] private string m_ID;
    [FormerlySerializedAs("Name")][SerializeField] private string m_Name;
    [FormerlySerializedAs("Description")][SerializeField] private string m_Description;
    [FormerlySerializedAs("AmountToAchieve")][SerializeField] private int m_AmountToAchieve;
    [FormerlySerializedAs("CurrentAmount")][SerializeField] private int m_CurrentAmount;
    [FormerlySerializedAs("completed")][SerializeField] private bool m_Completed;

    // Propiedades públicas (manteniendo los nombres originales para que el resto del código no se rompa)
    public string ID => m_ID;
    public string Name => m_Name;
    public string Description => m_Description;
    public int AmountToAchieve => m_AmountToAchieve;
    public int CurrentAmount { get => m_CurrentAmount; set => m_CurrentAmount = value; }
    public bool Completed { get => m_Completed; set => m_Completed = value; }

    public Achievement(string iD, string name, string description, int amountToAchieve)
    {
        m_ID = iD;
        m_Name = name;
        m_Description = description;
        m_AmountToAchieve = amountToAchieve;
        m_CurrentAmount = 0;
        m_Completed = false;
    }

    // Constructor vacío necesario para la deserialización de JsonUtility
    public Achievement() { }
}
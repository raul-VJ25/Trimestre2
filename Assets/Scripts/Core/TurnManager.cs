using UnityEngine;

// Gestor del sistema de turnos
// Controla cuando ocurren eventos por turno
public class TurnManager
{
    public event System.Action OnTick;
    private int m_TurnCount;

    public TurnManager()
    {
        m_TurnCount = 1;
    }

    // Avanza un turno y notifica a todos los suscriptores
    public void Tick()
    {
        OnTick?.Invoke();
        m_TurnCount += 1;
        Debug.Log("Turno : " + m_TurnCount);
    }
}
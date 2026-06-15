using UnityEngine;

// Clase base para todos los objetos que pueden estar en una celda del tablero
public class CellObject : MonoBehaviour
{
    protected Vector2Int m_Cell;

    // Inicializa el objeto con su posicion en el grid
    public virtual void Init(Vector2Int cell)
    {
        m_Cell = cell;
    }

    // Llamado cuando el jugador entra en esta celda
    public virtual void PlayerEntered()
    {
    }

    // Retorna true si el jugador puede entrar en esta celda
    public virtual bool PlayerWantsToEnter()
    {
        return true;
    }
}
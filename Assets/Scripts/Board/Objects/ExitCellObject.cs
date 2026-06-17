using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.Serialization;

// Celda de salida que avanza al siguiente nivel
public class ExitCellObject : CellObject, IInteractable
{
    [FormerlySerializedAs("EndTile")][SerializeField] private Tile m_EndTile;

    public override void Init(Vector2Int coord)
    {
        base.Init(coord);
        GameManager.Instance.BoardManager.SetCellTile(coord, m_EndTile);
    }

    // IMPLEMENTACIÓN DE LA INTERFAZ IInteractable
    public void Interact()
    {
        GameManager.Instance.NewLevel();
    }

    public override void PlayerEntered()
    {
        Interact();
    }
}
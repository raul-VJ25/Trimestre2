using UnityEngine;
using UnityEngine.Tilemaps;

// Celda de salida que avanza al siguiente nivel
public class ExitCellObject : CellObject
{
    public Tile EndTile;

    // Inicializa y coloca el tile de salida
    public override void Init(Vector2Int coord)
    {
        base.Init(coord);
        GameManager.Instance.BoardManager.SetCellTile(coord, EndTile);
    }

    // Avanza al siguiente nivel cuando el jugador entra
    public override void PlayerEntered()
    {
        GameManager.Instance.NewLevel();
    }
}
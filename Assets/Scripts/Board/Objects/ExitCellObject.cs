using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.Serialization;

public class ExitCellObject : CellObject
{
    [FormerlySerializedAs("EndTile")][SerializeField] private Tile m_EndTile;

    public override void Init(Vector2Int coord)
    {
        base.Init(coord);
        GameManager.Instance.BoardManager.SetCellTile(coord, m_EndTile);
    }

    public override void PlayerEntered()
    {
        GameManager.Instance.NewLevel();
    }
}
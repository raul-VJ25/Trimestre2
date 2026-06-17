using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.Serialization;

// Muro destructible que bloquea el paso
// Tiene diferentes niveles de vida segun el tipo
public class WallObject : CellObject
{
    [FormerlySerializedAs("ObstacleTiles")][SerializeField] private Tile[] m_ObstacleTiles;
    [FormerlySerializedAs("LowHealthTile")][SerializeField] private Tile m_LowHealthTile;

    private int m_HealthPoint;
    private Tile m_OriginalTile;
    private bool m_ShowedLowHealth = false;
    private readonly int[] HealthByIndex = { 3, 5, 7 };

    public override void Init(Vector2Int cell)
    {
        base.Init(cell);
        int index = Random.Range(0, m_ObstacleTiles.Length);
        Tile randomTile = m_ObstacleTiles[index];
        m_HealthPoint = index < HealthByIndex.Length ? HealthByIndex[index] : 3;
        m_OriginalTile = GameManager.Instance.BoardManager.GetCellTile(cell);
        GameManager.Instance.BoardManager.SetCellTile(cell, randomTile);
    }

    public override bool PlayerWantsToEnter()
    {
        int playerDamage = 1;
        if (SessionManager.Instance != null && SessionManager.Instance.CurrentPlayerData != null)
            playerDamage += SessionManager.Instance.CurrentPlayerData.BonusDamageToWalls;

        m_HealthPoint -= playerDamage;

        if (m_ShowedLowHealth)
        {
            GameManager.Instance.BoardManager.SetCellTile(m_Cell, m_OriginalTile);
            GameEvents.RaiseWallDestroyed();
            Destroy(gameObject);
            return true;
        }

        if (m_HealthPoint <= 1)
        {
            m_HealthPoint = 1;
            m_ShowedLowHealth = true;
            GameManager.Instance.BoardManager.SetCellTile(m_Cell, m_LowHealthTile);
            return false;
        }
        return false;
    }
}
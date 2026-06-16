using UnityEngine;
using UnityEngine.Tilemaps;

// Muro destructible que bloquea el paso
// Tiene diferentes niveles de vida segun el tipo
public class WallObject : CellObject
{
    public Tile[] ObstacleTiles;
    public Tile LowHealthTile;
    public int MaxHealth = 3;

    private int m_HealthPoint;
    private Tile m_OriginalTile;
    private bool m_ShowedLowHealth = false;

    // Vida asignada segun el sprite
    private readonly int[] HealthByIndex = { 3, 5, 7 };

    // Inicializa el muro con tile y vida aleatorios
    public override void Init(Vector2Int cell)
    {
        base.Init(cell);

        int index = Random.Range(0, ObstacleTiles.Length);
        Tile randomTile = ObstacleTiles[index];

        m_HealthPoint = index < HealthByIndex.Length ? HealthByIndex[index] : 3;

        m_OriginalTile = GameManager.Instance.BoardManager.GetCellTile(cell);
        GameManager.Instance.BoardManager.SetCellTile(cell, randomTile);
    }

    // Maneja el ataque del jugador al muro
    public override bool PlayerWantsToEnter()
    {
        int playerDamage = 1;
        if (SessionManager.Instance != null && SessionManager.Instance.CurrentPlayerData != null)
        {
            playerDamage += SessionManager.Instance.CurrentPlayerData.BonusDamageToWalls;
        }

        m_HealthPoint -= playerDamage;

        // Si ya mostro sprite danado, ahora se destruye
        if (m_ShowedLowHealth)
        {
            GameManager.Instance.BoardManager.SetCellTile(m_Cell, m_OriginalTile);
            GameEvents.RaiseWallDestroyed();
            Destroy(gameObject);
            return true;
        }

        // Muestra sprite de baja vida
        if (m_HealthPoint <= 1)
        {
            m_HealthPoint = 1;
            m_ShowedLowHealth = true;
            GameManager.Instance.BoardManager.SetCellTile(m_Cell, LowHealthTile);
            return false;
        }

        return false;
    }
}
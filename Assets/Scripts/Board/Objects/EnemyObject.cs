using UnityEngine;
using UnityEngine.Serialization;

public class EnemyObject : CellObject, IDamageable
{
    [FormerlySerializedAs("Health")][SerializeField] private int m_Health = 3;
    [FormerlySerializedAs("Damage")][SerializeField] private int m_Damage = 1;

    private int m_CurrentHealth;

    private void Awake()
    {
        if (GameManager.Instance != null && GameManager.Instance.TurnManager != null)
            GameManager.Instance.TurnManager.OnTick += TurnHappened;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null && GameManager.Instance.TurnManager != null)
            GameManager.Instance.TurnManager.OnTick -= TurnHappened;
    }

    public override void Init(Vector2Int coord)
    {
        base.Init(coord);
        int bonus = SessionManager.Instance != null ? SessionManager.Instance.EnemyHealthBonus : 0;
        m_CurrentHealth = m_Health + bonus;
    }

    public void TakeDamage(int amount)
    {
        m_CurrentHealth -= amount;
        if (m_CurrentHealth <= 0)
        {
            GameEvents.RaiseEnemyKilled();
            Destroy(gameObject);
        }
    }

    public bool IsDead => m_CurrentHealth <= 0;

    public override bool PlayerWantsToEnter()
    {
        int playerDamage = 1;
        if (SessionManager.Instance != null && SessionManager.Instance.CurrentPlayerData != null)
            playerDamage += SessionManager.Instance.CurrentPlayerData.BonusDamageToEnemies;

        TakeDamage(playerDamage);
        return IsDead;
    }

    bool MoveTo(Vector2Int coord)
    {
        var board = GameManager.Instance.BoardManager;
        var targetCell = board.GetCellData(coord);
        if (targetCell == null || !targetCell.Passable || targetCell.ContainedObject != null) return false;

        var currentCell = board.GetCellData(m_Cell);
        currentCell.ContainedObject = null;
        targetCell.ContainedObject = this;
        m_Cell = coord;
        transform.position = board.CellToWorld(coord);
        return true;
    }

    void TurnHappened()
    {
        var playerCell = GameManager.Instance.PlayerController.Cell;
        int xDist = playerCell.x - m_Cell.x;
        int yDist = playerCell.y - m_Cell.y;
        int absXDist = Mathf.Abs(xDist);
        int absYDist = Mathf.Abs(yDist);

        if ((xDist == 0 && absYDist == 1) || (yDist == 0 && absXDist == 1))
            TryAttackPlayer();
        else
        {
            if (absXDist > absYDist)
            {
                if (!TryMoveInX(xDist)) TryMoveInY(yDist);
            }
            else
            {
                if (!TryMoveInY(yDist)) TryMoveInX(xDist);
            }
        }
    }

    void TryAttackPlayer()
    {
        if (SessionManager.Instance != null && SessionManager.Instance.CurrentPlayerData != null)
        {
            float dodgeChance = SessionManager.Instance.CurrentPlayerData.DodgeChance;
            if (Random.value < dodgeChance)
            {
                GameManager.Instance.ShowDodgeNotification("¡Esquivado!");
                return;
            }
        }
        GameManager.Instance.ChangeLife(-m_Damage);
    }

    bool TryMoveInX(int xDist) => xDist > 0 ? MoveTo(m_Cell + Vector2Int.right) : MoveTo(m_Cell + Vector2Int.left);
    bool TryMoveInY(int yDist) => yDist > 0 ? MoveTo(m_Cell + Vector2Int.up) : MoveTo(m_Cell + Vector2Int.down);
}
using UnityEngine;

// Enemigo que persigue al jugador y ataca
// Se mueve cada turno hacia el jugador
public class EnemyObject : CellObject
{
    // Configuracion del enemigo
    public int Health = 3;
    public int Damage = 1;
    private int m_CurrentHealth;

    // Suscripcion al sistema de turnos
    private void Awake()
    {
        if (GameManager.Instance != null && GameManager.Instance.TurnManager != null)
        {
            GameManager.Instance.TurnManager.OnTick += TurnHappened;
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null && GameManager.Instance.TurnManager != null)
        {
            GameManager.Instance.TurnManager.OnTick -= TurnHappened;
        }
    }

    // Inicializa la vida con bonus por noche
    public override void Init(Vector2Int coord)
    {
        base.Init(coord);

        int bonus = 0;
        if (SessionManager.Instance != null)
        {
            bonus = SessionManager.Instance.EnemyHealthBonus;
        }

        m_CurrentHealth = Health + bonus;
    }

    // Maneja el ataque del jugador al enemigo
    public override bool PlayerWantsToEnter()
    {
        int playerDamage = 1;
        if (SessionManager.Instance != null && SessionManager.Instance.CurrentPlayerData != null)
        {
            playerDamage += SessionManager.Instance.CurrentPlayerData.BonusDamageToEnemies;
        }

        m_CurrentHealth -= playerDamage;

        if (m_CurrentHealth <= 0)
        {
            GameEvents.RaiseEnemyKilled();
            Destroy(gameObject);
        }

        return m_CurrentHealth <= 0;
    }

    // Intenta mover el enemigo a una nueva posicion
    bool MoveTo(Vector2Int coord)
    {
        var board = GameManager.Instance.BoardManager;
        var targetCell = board.GetCellData(coord);

        if (targetCell == null
            || !targetCell.Passable
            || targetCell.ContainedObject != null)
        {
            return false;
        }

        var currentCell = board.GetCellData(m_Cell);
        currentCell.ContainedObject = null;

        targetCell.ContainedObject = this;
        m_Cell = coord;
        transform.position = board.CellToWorld(coord);

        return true;
    }

    // IA simple que persigue al jugador
    void TurnHappened()
    {
        var playerCell = GameManager.Instance.PlayerController.Cell;

        int xDist = playerCell.x - m_Cell.x;
        int yDist = playerCell.y - m_Cell.y;

        int absXDist = Mathf.Abs(xDist);
        int absYDist = Mathf.Abs(yDist);

        // Si esta adyacente, ataca
        if ((xDist == 0 && absYDist == 1)
            || (yDist == 0 && absXDist == 1))
        {
            TryAttackPlayer();
        }
        else
        {
            // Se mueve priorizando la mayor distancia
            if (absXDist > absYDist)
            {
                if (!TryMoveInX(xDist))
                {
                    TryMoveInY(yDist);
                }
            }
            else
            {
                if (!TryMoveInY(yDist))
                {
                    TryMoveInX(xDist);
                }
            }
        }
    }

    // Intenta atacar al jugador con posibilidad de esquive
    void TryAttackPlayer()
    {
        if (SessionManager.Instance != null && SessionManager.Instance.CurrentPlayerData != null)
        {
            float dodgeChance = SessionManager.Instance.CurrentPlayerData.DodgeChance;

            if (Random.value < dodgeChance)
            {
                GameManager.Instance.ShowDodgeNotification("Esquivado!");
                return;
            }
        }

        GameManager.Instance.ChangeLife(-Damage);
    }

    // Intenta moverse en direccion horizontal
    bool TryMoveInX(int xDist)
    {
        if (xDist > 0)
        {
            return MoveTo(m_Cell + Vector2Int.right);
        }
        else
        {
            return MoveTo(m_Cell + Vector2Int.left);
        }
    }

    // Intenta moverse en direccion vertical
    bool TryMoveInY(int yDist)
    {
        if (yDist > 0)
        {
            return MoveTo(m_Cell + Vector2Int.up);
        }
        else
        {
            return MoveTo(m_Cell + Vector2Int.down);
        }
    }
}
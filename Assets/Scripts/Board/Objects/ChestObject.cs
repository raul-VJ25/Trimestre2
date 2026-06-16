using UnityEngine;

// Cofre que otorga recompensas al ser abierto
// Puede estar congelado y dar puntos de habilidad o XP
public class ChestObject : CellObject
{
    // Sprites para diferentes estados y tipos
    public Sprite[] CofresCerrados;
    public Sprite[] CofresHelados;
    public Sprite[] SpritesAbiertos;

    private int m_Health;
    private bool m_IsFrozen;
    private int m_TypeIndex;
    private SpriteRenderer m_SpriteRenderer;
    private bool m_IsOpened = false;

    // Probabilidades de otorgar punto de habilidad segun tipo
    private readonly float[] SkillPointChances = { 0.15f, 0.30f, 0.50f, 0.75f };

    // Rangos de XP otorgado segun tipo (min, max)
    private readonly int[,] XPRanges = {
        { 5, 10 },
        { 10, 15 },
        { 15, 20 },
        { 20, 30 }
    };

    private void Awake()
    {
        m_SpriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Inicializa el cofre con tipo y estado aleatorio
    public override void Init(Vector2Int cell)
    {
        base.Init(cell);

        float randType = Random.value;

        if (randType < 0.50f) m_TypeIndex = 0;
        else if (randType < 0.80f) m_TypeIndex = 1;
        else if (randType < 0.95f) m_TypeIndex = 2;
        else m_TypeIndex = 3;

        m_IsFrozen = Random.value < 0.30f;

        if (m_IsFrozen)
        {
            m_Health = 4;
            m_SpriteRenderer.sprite = CofresHelados[m_TypeIndex];
        }
        else
        {
            m_Health = 2;
            m_SpriteRenderer.sprite = CofresCerrados[m_TypeIndex];
        }
    }

    // Maneja intentos del jugador de abrir el cofre
    public override bool PlayerWantsToEnter()
    {
        m_Health--;

        if (m_IsFrozen)
        {
            if (m_Health == 2)
            {
                m_SpriteRenderer.sprite = CofresCerrados[m_TypeIndex];
            }
            else if (m_Health == 1)
            {
                OpenChest();
            }
        }
        else
        {
            if (m_Health == 1)
            {
                OpenChest();
            }
        }

        return false;
    }

    // Abre el cofre y otorga recompensa
    private void OpenChest()
    {
        if (m_IsOpened) return;
        m_IsOpened = true;

        m_SpriteRenderer.sprite = SpritesAbiertos[m_TypeIndex];
        SetCellPassable(false);

        GiveReward();
    }

    // Determina y otorga la recompensa del cofre
    private void GiveReward()
    {
        float chance = m_TypeIndex < SkillPointChances.Length ? SkillPointChances[m_TypeIndex] : 0f;

        if (Random.value < chance)
        {
            if (SessionManager.Instance != null)
            {
                SessionManager.Instance.AvailableSkillPoints++;

                if (GameManager.Instance != null)
                {
                    GameManager.Instance.UpdateSkillPointsUI();
                    GameManager.Instance.ShowAchievementNotification("\n+1 Punto de Habilidad!", true);
                }
            }
        }
        else
        {
            int minXP = XPRanges[m_TypeIndex, 0];
            int maxXP = XPRanges[m_TypeIndex, 1];
            int xpAmount = Random.Range(minXP, maxXP + 1);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.ChangeXP(xpAmount);
                GameManager.Instance.ShowXPNotification($"+{xpAmount} XP");
            }
        }
    }

    // Cambia si la celda es transitable
    private void SetCellPassable(bool passable)
    {
        var board = GameManager.Instance.BoardManager;
        var cellData = board.GetCellData(m_Cell);
        if (cellData != null)
        {
            cellData.Passable = passable;
        }
    }
}
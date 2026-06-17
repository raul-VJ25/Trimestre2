using UnityEngine;
using UnityEngine.Serialization;

public class ChestObject : CellObject
{
    [FormerlySerializedAs("CofresCerrados")][SerializeField] private Sprite[] m_CofresCerrados;
    [FormerlySerializedAs("CofresHelados")][SerializeField] private Sprite[] m_CofresHelados;
    [FormerlySerializedAs("SpritesAbiertos")][SerializeField] private Sprite[] m_SpritesAbiertos;

    private int m_Health;
    private bool m_IsFrozen;
    private int m_TypeIndex;
    private SpriteRenderer m_SpriteRenderer;
    private bool m_IsOpened = false;

    private readonly float[] SkillPointChances = { 0.15f, 0.30f, 0.50f, 0.75f };
    private readonly int[,] XPRanges = { { 5, 10 }, { 10, 15 }, { 15, 20 }, { 20, 30 } };

    private void Awake() { m_SpriteRenderer = GetComponent<SpriteRenderer>(); }

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
            m_SpriteRenderer.sprite = m_CofresHelados[m_TypeIndex];
        }
        else
        {
            m_Health = 2;
            m_SpriteRenderer.sprite = m_CofresCerrados[m_TypeIndex];
        }
    }

    public override bool PlayerWantsToEnter()
    {
        m_Health--;
        if (m_IsFrozen)
        {
            if (m_Health == 2) m_SpriteRenderer.sprite = m_CofresCerrados[m_TypeIndex];
            else if (m_Health == 1) OpenChest();
        }
        else
        {
            if (m_Health == 1) OpenChest();
        }
        return false;
    }

    private void OpenChest()
    {
        if (m_IsOpened) return;
        m_IsOpened = true;
        m_SpriteRenderer.sprite = m_SpritesAbiertos[m_TypeIndex];
        SetCellPassable(false);
        GiveReward();
    }

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
                    GameManager.Instance.ShowAchievementNotification("¡+1 Punto de Habilidad!", true);
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
                GameManager.Instance.ShowXPNotification($"¡+{xpAmount} XP!");
            }
        }
    }

    private void SetCellPassable(bool passable)
    {
        var board = GameManager.Instance.BoardManager;
        var cellData = board.GetCellData(m_Cell);
        if (cellData != null) cellData.Passable = passable;
    }
}
using UnityEngine;

// Sistema de gestión de experiencia del jugador
public class XPSystem : MonoBehaviour
{
    [SerializeField] private int m_CurrentXP = 0;

    public event System.Action<int> OnXPChanged;
    public int CurrentXP => m_CurrentXP;

    public void Init(int startingXP)
    {
        m_CurrentXP = startingXP;
        RaiseXPChanged();
    }

    public void ChangeXP(int amount)
    {
        m_CurrentXP += amount;
        if (m_CurrentXP < 0) m_CurrentXP = 0;
        RaiseXPChanged();
    }

    public void AddXP(int amount) => ChangeXP(amount);
    public int GetXP() => m_CurrentXP;

    private void RaiseXPChanged() => OnXPChanged?.Invoke(m_CurrentXP);
}
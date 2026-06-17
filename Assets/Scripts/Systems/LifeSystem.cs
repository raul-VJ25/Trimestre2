using UnityEngine;

// Sistema de gestión de vida del jugador
// Encapsula toda la lógica relacionada con HP, daño y muerte
public class LifeSystem : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private PlayerController m_PlayerController;

    [Header("Vida Actual")]
    [SerializeField] private int m_CurrentLife = 100;

    // Eventos
    public event System.Action<int> OnLifeChanged;
    public event System.Action OnPlayerDeath;

    // Propiedad de solo lectura
    public int CurrentLife => m_CurrentLife;

    public void Init(int startingLife)
    {
        m_CurrentLife = startingLife;
        RaiseLifeChanged();
    }

    public void ChangeLife(int amount)
    {
        m_CurrentLife += amount;
        if (m_CurrentLife < 0) m_CurrentLife = 0;

        RaiseLifeChanged();
        if (m_CurrentLife <= 0) HandleDeath();
    }

    public void TakeDamage(int damage) => ChangeLife(-damage);
    public void Heal(int amount) => ChangeLife(amount);

    private void HandleDeath()
    {
        if (m_PlayerController != null) m_PlayerController.GameOver();
        OnPlayerDeath?.Invoke();
    }

    private void RaiseLifeChanged() => OnLifeChanged?.Invoke(m_CurrentLife);

    // Reducción de vida por turno
    public void OnTurnTick() => ChangeLife(-1);
}
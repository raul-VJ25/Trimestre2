// Interfaz para cualquier objeto que pueda recibir daño y morir
public interface IDamageable
{
    void TakeDamage(int amount);
    bool IsDead { get; }
}
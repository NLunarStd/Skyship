using static GameEnum;
public interface IDamageable
{
    int CurrentHealth { get; }
    Faction Side { get; }
    void TakeDamage(int damageAmount, Faction source);
    void TakeDamage(int amount);
    void Die();
}

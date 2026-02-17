using UnityEngine;
using static GameEnum;
public class EnemyHealth : MonoBehaviour, IDamageable, ITargetable
{
    [Header("Health Settings")]
    public int maxHealth = 10;
    private int currentHealth;
    [SerializeField] private Faction _faction;
    public Faction Side => _faction;
    public Transform damageEffectPrefab;
    void Awake()
    {
        currentHealth = maxHealth;
    }
    public int CurrentHealth => currentHealth;

    public void TakeDamage(int amount, Faction sourceFaction)
    {
        if (sourceFaction == _faction) return;
        ApplyDamage(amount);
    }
    public void TakeDamage(int amount)
    {
        ApplyDamage(amount);
    }
    private void ApplyDamage(int amount)
    {
        currentHealth -= amount;
        // Trigger VFX/SFX
        if (currentHealth <= 0) Die();
    }
    public void Die()
    {
        Debug.Log($"{gameObject.name} has died.");
        // Implement death behavior (e.g., play animation, disable character, etc.)
    }
    private void OnCollisionEnter(Collision collision)
    {
        IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();
        Faction sourceFaction = damageable != null ? damageable.Side : Faction.Neutral;
        if (damageable != null)
        {
            damageable.TakeDamage(1, sourceFaction);
            if (damageEffectPrefab != null)
            {
                Instantiate(damageEffectPrefab, collision.contacts[0].point, Quaternion.identity);
            }

            Debug.Log($"{gameObject.name} took damage from {collision.gameObject.name} (Faction: {sourceFaction}). Current Health: {currentHealth}");
        }
    }

    public Transform GetTransform()
    {
        return transform;
    }
    public bool IsActive()
    {
        return isActiveAndEnabled;
    }
    public Faction GetFaction()
    {
        return _faction;
    }
}
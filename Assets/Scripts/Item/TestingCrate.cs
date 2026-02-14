using UnityEngine;
using static GameEnum;
public class TestingCrate : BaseItem, IThrowable    
{
    [Header("Combat Stats")]
    [SerializeField] private int damage = 1;
    [SerializeField] private float stunDuration = 5f;
    [SerializeField] private float knockbackStrength = 5f;

    private bool isFlying = false;
    private Faction throwerFaction;
    public void PickUp()
    {

    }

    public void Drop()
    {

    }

    public void Throw(Vector3 direction, float throwForce)
    {
        isFlying = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"Crate hit: {collision.gameObject.name} with velocity: {collision.relativeVelocity.magnitude}");

        if (!isFlying)
        {
            Debug.Log("Hit cancelled because isFlying is false.");
            return;
        }

        if (collision.relativeVelocity.magnitude > 3f)
        {
            

        if (collision.gameObject.TryGetComponent<IDamageable>(out var damageable))
        {
            //damageable.TakeDamage(damage); // Universal damage
            damageable.TakeDamage(damage, throwerFaction); // Faction-specific damage
        }

        if (collision.gameObject.TryGetComponent<IStunnable>(out var stunnable))
        {
            stunnable.Stun(stunDuration);
        }

        if (collision.gameObject.TryGetComponent<Rigidbody>(out var targetRb))
        {
            Vector3 knockbackDir = (collision.transform.position - transform.position).normalized;
            knockbackDir.y = 0.5f; 
            targetRb.AddForce(knockbackDir * knockbackStrength, ForceMode.Impulse);
        }

        isFlying = false;

        PlayImpactEffects();

        }
    }

    private void PlayImpactEffects() 
    { 
    
    }

    public override void Sync()
    {

    }
}

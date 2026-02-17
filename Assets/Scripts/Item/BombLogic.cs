using UnityEngine;

public class BombLogic : MonoBehaviour
{
    public float explosionRadius = 5f;
    public float explosionForce = 700f;
    public int bombDamage = 5;
    public int detonationTime = 3;
    public int currentTime = 0;

    private void Start()
    {
        Use();
    }
    public void Use()
    {
        InvokeRepeating(nameof(CountTime), 1f, 3f);
    }
    
    public void Detonate()
    {
        Explode();
        Destroy(gameObject);
    }

    public void CountTime()
    {
        currentTime++;
        if (currentTime >= detonationTime)
        {
            Detonate();
        }
    }

    public void Explode()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (var hit in hitColliders)
        {
            IDamageable target = hit.GetComponent<IDamageable>();
            if (target != null)
            {
                target.TakeDamage(bombDamage);

                Rigidbody rb = hit.GetComponent<Rigidbody>();
                if (rb != null) rb.AddExplosionForce(explosionForce, transform.position, explosionRadius);

                Debug.Log($"Bomb exploded and hit {hit.name} for {bombDamage} damage.");
            }
        }
    }
}

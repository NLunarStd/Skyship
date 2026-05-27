using System.Collections;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(PooledItem))]
public class BombItem : NetworkBehaviour, IPickable, IThrowable
{
    [Header("Bomb Settings")]
    public float explosionDelay = 3f;
    public float explosionRadius = 5f;
    public GameObject explosionEffectPrefab; 
    
    private bool isArmed = false;
    private Coroutine explosionCoroutine;
    private PooledItem pooledItem;

    private void Awake()
    {
        pooledItem = GetComponent<PooledItem>();
    }

    public void PickUp()
    {
        if (!IsServer) return;

        if (explosionCoroutine != null)
        {
            StopCoroutine(explosionCoroutine);
            explosionCoroutine = null;
        }
        isArmed = false;
        DisarmBombClientRpc();
    }

    public void Drop()
    {
        if (IsServer) ArmBomb();
    }

    public void Throw(Vector3 direction, float throwForce)
    {
        if (IsServer) ArmBomb();
    }

    private void ArmBomb()
    {
        if (isArmed) return;
        isArmed = true;
        
        Debug.Log("Bomb is armed! Starting 3 second timer...");
        explosionCoroutine = StartCoroutine(ExplosionTimer());
        
        ArmBombClientRpc();
    }

    [ClientRpc]
    private void ArmBombClientRpc()
    {
        isArmed = true;
        StartCoroutine(BlinkRed());
    }

    [ClientRpc]
    private void DisarmBombClientRpc()
    {
        isArmed = false;
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (var r in renderers)
        {
            if (r != null) r.material.color = Color.white;
        }
    }

    private IEnumerator BlinkRed()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        while (isArmed)
        {
            foreach (var r in renderers)
            {
                if (r != null) r.material.color = Color.red;
            }
            yield return new WaitForSeconds(0.2f);
            foreach (var r in renderers)
            {
                if (r != null) r.material.color = Color.white;
            }
            yield return new WaitForSeconds(0.2f);
        }
    }

    private IEnumerator ExplosionTimer()
    {
        yield return new WaitForSeconds(explosionDelay);
        Explode();
    }

    private void Explode()
    {
        Debug.Log("Bomb EXPLODED!");
        isArmed = false;
        DisarmBombClientRpc();
        
        // Find players within explosion radius
        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                var netObj = hit.GetComponentInParent<NetworkObject>();
                if (netObj != null)
                {
                    // Calculate center position based on DeadPlaneController (if exists)
                    Vector3 centerPos = new Vector3(0, 10f, 0); 
                    var deadPlane = FindFirstObjectByType<DeadPlaneController>();
                    if (deadPlane != null)
                    {
                        centerPos = Vector3.zero + deadPlane.respawnOffset;
                    }
                    
                    // Use GameManager to smoothly teleport the player
                    if (GameManager.instance != null)
                    {
                        GameManager.instance.TeleportClientRpc(centerPos, netObj.OwnerClientId);
                    }
                }
            }
        }
        
        ExplodeVisualsClientRpc();
        
        if (pooledItem != null)
        {
            pooledItem.ReturnToPool();
        }
        
        // Fallback: if it's still active (e.g. wasn't spawned by spawner so it didn't return properly), hide it
        if (gameObject.activeInHierarchy)
        {
            if (IsServer)
            {
                var netObj = GetComponent<NetworkObject>();
                if (netObj != null && netObj.IsSpawned)
                {
                    netObj.Despawn(true);
                }
                else
                {
                    Destroy(gameObject);
                }
            }
        }
    }

    [ClientRpc]
    private void ExplodeVisualsClientRpc()
    {
        if (explosionEffectPrefab != null)
        {
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        }
    }
}

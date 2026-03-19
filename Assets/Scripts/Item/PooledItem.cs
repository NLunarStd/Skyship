using Unity.Netcode;
using UnityEngine;

public class PooledItem : MonoBehaviour
{
    private ItemSpawner mySpawner;
    private GameObject myPrefab;

    public void SetSource(ItemSpawner spawner, GameObject prefab)
    {
        mySpawner = spawner;
        myPrefab = prefab;
    }

    public void ReturnToPool()
    {
        if (mySpawner != null)
        {
            ItemPoolManager.Instance.ReturnToPool(myPrefab, GetComponent<NetworkObject>());
            mySpawner.OnItemReturned();
        }
    }
}

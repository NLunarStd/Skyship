using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ItemPoolManager : NetworkBehaviour
{
    public static ItemPoolManager Instance;

    [System.Serializable]
    public class PoolData
    {
        public GameObject prefab;
        public int initialSize;
        [HideInInspector] public Stack<NetworkObject> poolStack = new Stack<NetworkObject>();
    }

    public List<PoolData> pools;
    private Dictionary<GameObject, PoolData> prefabToPoolMap = new Dictionary<GameObject, PoolData>();

    private void Awake()
    {
        Instance = this;
        foreach (var pool in pools)
        {
            prefabToPoolMap[pool.prefab] = pool;
        }
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        foreach (var pool in pools)
        {
            for (int i = 0; i < pool.initialSize; i++)
            {
                CreateNewObject(pool);
            }
        }
    }

    private NetworkObject CreateNewObject(PoolData pool)
    {
        GameObject obj = Instantiate(pool.prefab);
        NetworkObject netObj = obj.GetComponent<NetworkObject>();
        netObj.Spawn();
        ReturnToPool(pool.prefab, netObj);
        return netObj;
    }

    public NetworkObject GetFromPool(GameObject prefab)
    {
        if (prefabToPoolMap.TryGetValue(prefab, out PoolData pool))
        {
            if (pool.poolStack.Count == 0)
            {
                GameObject obj = Instantiate(pool.prefab);
                NetworkObject netObj = obj.GetComponent<NetworkObject>();
                netObj.Spawn();
                return netObj;
            }

            NetworkObject netObjFromStack = pool.poolStack.Pop();
            ActivateObjectClientRpc(netObjFromStack.NetworkObjectId);
            return netObjFromStack;
        }
        return null;
    }

    public void ReturnToPool(GameObject prefab, NetworkObject netObj)
    {
        if (prefabToPoolMap.TryGetValue(prefab, out PoolData pool))
        {
            pool.poolStack.Push(netObj);
            DeactivateObjectClientRpc(netObj.NetworkObjectId);
        }
    }

    [ClientRpc]
    private void ActivateObjectClientRpc(ulong id)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(id, out var netObj))
        {
            netObj.gameObject.SetActive(true);
        }
    }

    [ClientRpc]
    private void DeactivateObjectClientRpc(ulong id)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(id, out var netObj))
        {
            netObj.gameObject.SetActive(false);
        }
    }
}
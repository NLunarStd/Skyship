using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class ItemSpawner : NetworkBehaviour
{
    [Header("Spawn Settings")]
    public System.Collections.Generic.List<GameObject> itemPrefabs; 
    public int totalItems = 10;  
    public float spawnRadius = 5f;
    public float spawnInterval = 0.5f; 

    private int currentActiveItems = 0;

    //public override void OnNetworkSpawn()
    //{
    //    if (!IsServer) return;


    //    StartCoroutine(InitialSpawnRoutine());
    //}

    private IEnumerator InitialSpawnRoutine()
    {
        if (!NetworkManager.Singleton.IsListening) yield break;

        for (int i = 0; i < totalItems; i++)
        {
            SpawnOneItem();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    public void SpawnOneItem()
    {
        if (!IsServer) return;
        if (itemPrefabs == null || itemPrefabs.Count == 0) return;

        Vector2 randomPoint = UnityEngine.Random.insideUnitCircle * spawnRadius;
        Vector3 spawnPos = transform.position + new Vector3(randomPoint.x, 2f, randomPoint.y); // สูงจากพื้นเล็กน้อย

        GameObject selectedPrefab = itemPrefabs[UnityEngine.Random.Range(0, itemPrefabs.Count)];
        if (selectedPrefab == null) return;

        NetworkObject item = ItemPoolManager.Instance.GetFromPool(selectedPrefab);
        if (item != null)
        {
            item.transform.position = spawnPos;
            item.transform.rotation = Quaternion.Euler(0, UnityEngine.Random.Range(0, 360f), 0);
            currentActiveItems++;

            if (item.TryGetComponent<PooledItem>(out var pooledScript))
            {
                pooledScript.SetSource(this, selectedPrefab);
            }
        }
    }

    private bool isSpawning = false;

    public void OnItemReturned()
    {
        currentActiveItems--;
        if (isSpawning)
        {
            StartCoroutine(DelayedRespawn());
        }
    }

    private IEnumerator DelayedRespawn()
    {
        yield return new WaitForSeconds(spawnInterval);
        SpawnOneItem();
    }

    public void StartSpawning()
    {
        if (!IsServer) return;
        isSpawning = true;
        StartCoroutine(InitialSpawnRoutine());
    }

    public void StopSpawning()
    {
        isSpawning = false;
        StopAllCoroutines();
        currentActiveItems = 0;
    }
}

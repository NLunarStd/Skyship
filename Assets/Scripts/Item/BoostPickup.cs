using System;
using Unity.Netcode;
using UnityEngine;


public class BoostPickup : NetworkBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        DestroyServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void DestroyServerRpc()
    {
        Debug.Log("Destroying");

        NetworkObject.Despawn(true);
    }
}

using System;
using Unity.Netcode;
using UnityEngine;


public class BoostPickup : NetworkBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        
        DestroyServerRpc();
    }

    [ServerRpc]
    private void DestroyServerRpc()
    {
        Debug.Log("Destroying");
         
        this.gameObject.SetActive(false);
    }
}

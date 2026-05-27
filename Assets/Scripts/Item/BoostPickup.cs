using System;
using Unity.Netcode;
using UnityEngine;


public class BoostPickup : NetworkBehaviour
{
    public float rotationSpeed = 90f;
    public NetworkVariable<bool> isCollected = new NetworkVariable<bool>(false);

    public override void OnNetworkSpawn()
    {
        isCollected.OnValueChanged += (oldVal, newVal) => ToggleVisibility(!newVal);
        ToggleVisibility(!isCollected.Value);
    }

    private void Update()
    {
        if (!isCollected.Value)
        {
            transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f, Space.Self);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer || isCollected.Value) return;

        if (other.CompareTag("Player"))
        {
            CollectServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void CollectServerRpc()
    {
        isCollected.Value = true;
    }

    public void ResetBoost()
    {
        if (IsServer)
        {
            isCollected.Value = false;
        }
    }

    private void ToggleVisibility(bool visible)
    {
        var cols = GetComponentsInChildren<Collider>();
        foreach (var c in cols) c.enabled = visible;

        var renderers = GetComponentsInChildren<Renderer>();
        foreach (var r in renderers) r.enabled = visible;
    }
}

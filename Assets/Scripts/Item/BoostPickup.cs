using System;
using UnityEngine;

public class BoostPickup : MonoBehaviour
{
    private Collider collider;

    private void Awake()
    {
        collider  = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Destroy(this);
        }
    }
}

using UnityEngine;
using System.Collections.Generic;

public class DeadPlaneController : MonoBehaviour
{

    public Transform playerCrystal;
    public Vector3 respawnOffset;
    private void Start()
    {
        //if (playerCrystal == null)
        //{
        //    Debug.LogError("Player Crystal reference is not set in the inspector.");
        //}
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<PooledItem>(out var item))
        {
            item.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
            item.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            item.ReturnToPool();
        }

        if (other.CompareTag("Player"))
        {

            other.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
            other.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            other.transform.position = Vector3.zero + respawnOffset;
            if (playerCrystal != null)
            {
                
                //other.transform.position = playerCrystal.position + respawnOffset;
            }
        }

    }
}

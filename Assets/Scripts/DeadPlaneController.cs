using UnityEngine;
using System.Collections.Generic;

public class DeadPlaneController : MonoBehaviour
{

    public Transform playerCrystal;
    public Vector3 respawnOffset;
    private void Start()
    {
        if (playerCrystal == null)
        {
            Debug.LogError("Player Crystal reference is not set in the inspector.");
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (playerCrystal != null)
            {
                other.GetComponent<Rigidbody>().linearVelocity = Vector3.zero; 
                other.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;

                other.transform.position = playerCrystal.position + respawnOffset;
            }
        }

    }
}

using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class PlayerBoost : NetworkBehaviour
{
    [Header("Speed boost")] 
    [SerializeField] private float speedBoostStr = 1.2f;
    [SerializeField] private float speedBoostDur = 5f;
    private bool speedBoostActive = false;
    private float speedBoostTimer = 0;
    private bool speedBoostPicked = false;
    
    [Header("Jump boost")]
    [SerializeField] private float jumpBoostStr = 1.2f;
    [SerializeField] private float jumpBoostDur = 5f;
    private bool jumpBoostActive = false;
    private float jumpBoostTimer = 0;
    private bool jumpBoostPicked = false;
    
    [Header("Input & UI")]
    [SerializeField] private InputActionReference speedBoostKey;
    [SerializeField] private InputActionReference jumpBoostKey;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private AudioClip useBoostSound;

    //new code for vfx from betty
    [Header("VFX")] 
    [SerializeField] private ParticleSystem speedBoostVFX;
    [SerializeField] private ParticleSystem jumpBoostVFX;
    //new code for vfx from betty

    //events
    [SerializeField] EventManager eventManager;

    private float originalSpeed;
    private float originalSprint;
    private float originalJump;
    
    private NetworkPlayerMovement networkPlayerMovement;
    
    private Collider collider;
    
    private void Awake()
    {
        networkPlayerMovement = GetComponent<NetworkPlayerMovement>();
        collider = GetComponent<Collider>();

        originalJump = networkPlayerMovement.jumpForce;
        originalSpeed = networkPlayerMovement.walkSpeed;
        originalSprint = networkPlayerMovement.sprintSpeed;
    }
    
    private void Update()
    {
        if (!IsOwner) return;

        if (speedBoostKey.action.WasPressedThisFrame())
        {
            if (!speedBoostActive && speedBoostPicked)
            {
                Debug.Log("BoostSpeed");
                speedBoostActive = true;
                speedBoostTimer = speedBoostDur;
                EventManager.Instance.TriggerOnSpeedBoostActive();
                speedBoostPicked = false;

                // Boost both walk and sprint speed
                networkPlayerMovement.walkSpeed = originalSpeed * speedBoostStr;
                networkPlayerMovement.sprintSpeed = originalSprint * speedBoostStr;

                //new code for active vfx speedboost from betty
                if (speedBoostVFX != null) speedBoostVFX.Play();
                
                if (SFXManager.Instance != null && useBoostSound != null)
                {
                    SFXManager.Instance.PlaySFX2D(useBoostSound);
                }
            }
        }

        if (jumpBoostKey.action.WasPressedThisFrame())
        {
            if (!jumpBoostActive && jumpBoostPicked)
            {
                Debug.Log("BoostJump");
                jumpBoostActive = true;
                jumpBoostTimer = jumpBoostDur;
                EventManager.Instance.TriggerOnJumpBoostActive();
                jumpBoostPicked = false;
                networkPlayerMovement.jumpForce = originalJump * jumpBoostStr;

                //new code for active vfx jumpboost from betty
                if (jumpBoostVFX != null) jumpBoostVFX.Play();

                if (SFXManager.Instance != null && useBoostSound != null)
                {
                    SFXManager.Instance.PlaySFX2D(useBoostSound);
                }
            }   
        }
        
        if (speedBoostActive)
        {
            speedBoostTimer -= Time.deltaTime;
            
            if (speedBoostTimer <= 0)
            {
                speedBoostActive = false;
                networkPlayerMovement.walkSpeed = originalSpeed;
                networkPlayerMovement.sprintSpeed = originalSprint;

                //new code for close vfx speedboost from betty
                if (speedBoostVFX != null) speedBoostVFX.Stop();
            }
        }
        
        if (jumpBoostActive)
        {
            jumpBoostTimer -= Time.deltaTime;
            
            if (jumpBoostTimer <= 0)
            {
                jumpBoostActive = false;
                networkPlayerMovement.jumpForce = originalJump;

                //new code for close vfx jumpboost from betty
                if (jumpBoostVFX != null) jumpBoostVFX.Stop();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsOwner) return;

        if (other.CompareTag(TagHandle.GetExistingTag("SpeedBoost"))) // && !speedBoostPicked
        {
            OnSpeedPickup();
        }

        if (other.CompareTag(TagHandle.GetExistingTag("JumpBoost")) ) //&& !jumpBoostPicked
        {
            OnJumpPikcup();
        }
    }

    private void OnSpeedPickup()
    {
        // speed boost picked up
        speedBoostPicked = true;
        // event call
        EventManager.Instance.TriggerOnSpeedBoostPickup();
        Debug.Log("Collided with speed boost");

        if (SFXManager.Instance != null && pickupSound != null)
        {
            SFXManager.Instance.PlaySFX2D(pickupSound);
        }
    }

    private void OnJumpPikcup()
    {
        // jump boost picked up
        jumpBoostPicked = true;
        // event call
        EventManager.Instance.TriggerOnJumpBoostPickup();
        Debug.Log("Collided with jump boost");

        if (SFXManager.Instance != null && pickupSound != null)
        {
            SFXManager.Instance.PlaySFX2D(pickupSound);
        }
    }
    
}

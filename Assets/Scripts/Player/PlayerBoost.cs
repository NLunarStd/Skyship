using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Image = UnityEngine.UIElements.Image;

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
    
    private NetworkPlayerMovement networkPlayerMovement;
    private GameObject speedBoostImage;
    private GameObject jumpBoostImage;
    
    
    
    private Collider collider;
    private void Awake()
    {
        networkPlayerMovement = GetComponent<NetworkPlayerMovement>();
        speedBoostImage = GameObject.Find("SpeedBoostImg");
        jumpBoostImage = GameObject.Find("JumpBoostImg");
        collider = GetComponent<Collider>();
    }
    
    private void Update()
    {
        if (speedBoostActive)
        {
            speedBoostTimer -= Time.deltaTime;
            networkPlayerMovement.walkSpeed *= speedBoostStr;
            
            if (speedBoostTimer <= 0)
            {
                speedBoostActive = false;
            }
        }


        if (jumpBoostActive)
        {
            jumpBoostTimer -= Time.deltaTime;
            networkPlayerMovement.jumpForce *= jumpBoostStr;
            
            if (jumpBoostTimer <= 0)
            {
                jumpBoostActive = false;
            }
        }
        
    }

    public void BoostSpeed()
    {
        if (!speedBoostActive &&  speedBoostKey.action.WasPressedThisFrame())
        {
            speedBoostActive = true;
            speedBoostTimer = speedBoostDur;
            speedBoostImage.SetActive(false);
            speedBoostPicked = false;
        }

    }

    public void BoostJump()
    {
        if (!jumpBoostActive && jumpBoostKey.action.WasPressedThisFrame())
        {
            jumpBoostActive = true;
            jumpBoostTimer = jumpBoostDur;
            jumpBoostImage.SetActive(false);
            jumpBoostPicked = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(TagHandle.GetExistingTag("SpeedBoost")) && !speedBoostPicked)
        {
            // speed boost picked up
            speedBoostPicked = true;
            speedBoostImage.SetActive(true);
            
        }

        if (other.CompareTag(TagHandle.GetExistingTag("JumpBoost")) && !jumpBoostPicked)
        {
            // jump boost picked up
            jumpBoostPicked = true;
            jumpBoostImage.SetActive(true);

        }

    }
    
}

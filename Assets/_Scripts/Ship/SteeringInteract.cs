using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class SteeringInteract : MonoBehaviour
{
    public Transform rayPoint;
    public float interactDistance = 2f;
    public Transform rudderUsingPos;

    [SerializeField] Outline outline;

    private GameObject hoveredPlayer;   
    private GameObject activeController; 
    
    public NetworkShipHandler ship;
    private void Start()
    {
        outline = GetComponent<Outline>();
        outline.enabled = false;
    }

    [Header("Input Action Reference")]
    public InputActionReference controlRudder;
    public InputActionReference exitRudder;

    private GameObject DrivingPlayer;

    void Update()
    {

        CheckForPlayer();


        if (activeController == null)
        {
            if (hoveredPlayer != null && controlRudder.action.WasPressedThisFrame())
            {
                EnterShipControl(hoveredPlayer);
            }
        }
        else
        {
            if (controlRudder.action.WasPressedThisFrame())
            {
                ExitShipControl();
            }
        }

    }

    void LateUpdate()
    {
        if (activeController != null && rudderUsingPos != null)
        {
            activeController.transform.position = rudderUsingPos.position;
            activeController.transform.rotation = rudderUsingPos.rotation;
        }
    }

    private void CheckForPlayer()
    {
        if (activeController != null)
        {
            SetHighlight(false);
            hoveredPlayer = null;
            return;
        }

        Ray ray = new Ray(rayPoint.position, rayPoint.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance))
        {
            if (hit.collider.CompareTag("Player"))
            {
                if (hoveredPlayer != hit.collider.gameObject)
                {
                    hoveredPlayer = hit.collider.gameObject;
                    SetHighlight(true);
                }
                return;
            }
        }

        if (hoveredPlayer != null)
        {
            SetHighlight(false);
            hoveredPlayer = null;
        }
    }

    private void SetHighlight(bool value)
    {
        if (outline != null) outline.enabled = value;
    }

    void EnterShipControlAnimation()
    {

    }
    void ExitShipControlAnimation()
    {

    }
    public void EnterShipControl(GameObject player)
    {
        activeController = player;

        TogglePlayerScripts(activeController, false);

        EventManager.Publish(new CharacterControlRudderEvent(true, ship));

        Animator animator = player.GetComponentInChildren<Animator>();

        if (animator != null)
        {
            animator.SetTrigger("ControlShip");
            DrivingPlayer = player;
        }
        else
        {
            Debug.Log("ShipSteer Can't find the Animator :(");
        }

    }

    public void ExitShipControl()
    {
        if (activeController == null) return;
        print("Exiting ship control for " + activeController.name);
        TogglePlayerScripts(activeController, true);

        EventManager.Publish(new CharacterControlRudderEvent(false, ship));

        if (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsHost)
        {
            StopShipImmediately();
        }
        else
        {
            var steering = GetComponent<NetworkShipSteeringControl>();
            if (steering != null)
            {
                steering.StopShipServerRpc();
            }
        }

        activeController = null;

        if (DrivingPlayer != null)
        {
            Animator animator = DrivingPlayer.GetComponentInChildren<Animator>();

            if (animator != null)
            {
                animator.SetTrigger("ExitControlShip");
                DrivingPlayer = null;
            }
            else
            {
                Debug.Log("ShipSteer Can't find the Animator :(");
            }
        }
        else
        {
            Debug.Log("DrivingPlayer is null");
        }
        
    }
    private void StopShipImmediately()
    {
        if (ship != null)
        {
            ship.EngineOn.Value = false;

            ship.IsBraking.Value = false;
            ship.ForceStop();
        }
    }
    private void TogglePlayerScripts(GameObject player, bool isEnabled)
    {
        if (player == null) return;

        var playerMove = player.GetComponentInParent<NetworkPlayerMovement>();
        var playerInter = player.GetComponentInParent<PlayerItemInteractHandler>();
        var rb = player.GetComponentInParent<Rigidbody>();
        var cols = player.GetComponentsInChildren<Collider>();

        if (playerMove != null) playerMove.enabled = isEnabled;
        if (playerInter != null) playerInter.enabled = isEnabled;
        
        foreach (var c in cols)
        {
            c.enabled = isEnabled; // Disable player collider so they don't block the ship!
        }

        if (rb != null)
        {
            // If disabling scripts (entering ship control), make kinematic to avoid physics jitter
            // If enabling scripts (exiting), restore kinematic state (local player should be non-kinematic)
            rb.isKinematic = !isEnabled;
        }

        var netTransform = player.GetComponentInParent<Unity.Netcode.Components.NetworkTransform>();
        if (netTransform != null)
        {
            netTransform.Interpolate = isEnabled; // Turn off interpolation while driving to prevent double-interpolation jitter!
        }

        // Synchronize the physics disable to the Server and all other clients!
        var netObj = player.GetComponentInParent<NetworkObject>();
        if (netObj != null && ship != null)
        {
            var steering = GetComponent<NetworkShipSteeringControl>();
            if (steering != null)
            {
                // isEnabled = false means entering ship (isDriving = true)
                steering.SetPlayerDrivingServerRpc(netObj.NetworkObjectId, !isEnabled);
            }
        }
    }

    public bool IsCurrentController(GameObject player)
    {
        return activeController == player;
    }
}

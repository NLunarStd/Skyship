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


    void Update()
    {

        CheckForPlayer();


        if (hoveredPlayer != null && activeController == null)
        {
            if (controlRudder.action.WasPressedThisFrame())
            {
                EnterShipControl(hoveredPlayer);
            }
        }

        if (activeController != null)
        {
            if (exitRudder.action.WasPressedThisFrame())
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

    public void EnterShipControl(GameObject player)
    {
        activeController = player;

        TogglePlayerScripts(activeController, false);

        EventManager.Publish(new CharacterControlRudderEvent(true, ship));

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
            ship.EngineOn.Value = false;
        }

        activeController = null;
    }
    private void StopShipImmediately()
    {
        if (ship != null)
        {
            ship.EngineOn.Value = false;

            ship.IsBraking.Value = false;
        }
    }
    private void TogglePlayerScripts(GameObject player, bool isEnabled)
    {
        if (player == null) return;

        var playerMove = player.GetComponent<NetworkPlayerMovement>();
        var playerInter = player.GetComponent<PlayerItemInteractHandler>();

        if (playerMove != null) playerMove.enabled = isEnabled;
        if (playerInter != null) playerInter.enabled = isEnabled;

    }

    public bool IsCurrentController(GameObject player)
    {
        return activeController == player;
    }
}

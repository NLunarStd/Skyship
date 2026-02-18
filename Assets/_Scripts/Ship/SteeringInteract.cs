using UnityEngine;
using UnityEngine.InputSystem;

public class SteeringInteract : MonoBehaviour
{
    public Transform rayPoint;
    public float interactDistance = 2f;

    private GameObject currentPlayer;

    [SerializeField] Outline outline;

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
        CheckPlayer();
        Debug.DrawRay(rayPoint.position, rayPoint.forward * interactDistance, Color.red);

        if (currentPlayer != null && controlRudder.action.WasPerformedThisFrame())
        {
            EnterShipControl();
        }

        if (exitRudder.action.WasPerformedThisFrame())
        {
            ExitShipControl();
        }


    }

    void CheckPlayer()
    {
        Ray ray = new Ray(rayPoint.position, rayPoint.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance))
        {
            if (hit.collider.CompareTag("Player"))
            {
                if (currentPlayer != hit.collider.gameObject)
                {
                    currentPlayer = hit.collider.gameObject;

                    // เปิด highlight หรือ UI
                    outline.enabled = true;
                }

                return;
            }
        }

        // ถ้าไม่โดนอะไรหรือหลุดจาก player
        if (currentPlayer != null)
        {
            outline.enabled = false;
            currentPlayer = null;
        }
    }

    void EnterShipControl()
    {
        if (currentPlayer == null)
        {
            Debug.Log("Rudder's current player is Null");
            return;
        }
       
        PlayerMovement playerMove = currentPlayer.GetComponent<PlayerMovement>();
        playerMove.enabled = false;

        PlayerInteract playerInter = currentPlayer.GetComponent<PlayerInteract>();
        playerInter.enabled = false;

        //EventManager2.RaiseEnterShipControl();

        EventManager.Publish(new CharacterControlRudderEvent(true));

    }

    void ExitShipControl()
    {
        PlayerMovement playerMove = currentPlayer.GetComponent<PlayerMovement>();
        PlayerInteract playerInter = currentPlayer.GetComponent<PlayerInteract>();

        if (playerMove != null)
        {
            playerMove.enabled = true;
        }
        if( playerInter != null)
        {
            playerInter.enabled = true;
        }

        //EventManager2.RaiseExitShipContorl();

        EventManager.Publish(new CharacterControlRudderEvent(false));
    }
}

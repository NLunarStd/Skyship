using UnityEngine;

public class CharacterPickable : MonoBehaviour, IPickable
{
    private bool isBeingHeld;
    private Rigidbody rb;
    private StunHandler stunHandler;
    private PlayerMovement playerMovement; // Only if this is a player

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        stunHandler = GetComponent<StunHandler>();
        playerMovement = GetComponent<PlayerMovement>();
    }

    public void PickUp()
    {
        isBeingHeld = true;

        if (playerMovement != null) playerMovement.SetUsingMode(true);
        if (TryGetComponent<UnityEngine.AI.NavMeshAgent>(out var agent))
        {
            agent.enabled = false;
        }

        if (TryGetComponent<Collider>(out var col))
        {
            col.isTrigger = true;
        }

        rb.isKinematic = true;
    }

    public void Drop()
    {
        isBeingHeld = false;
        rb.isKinematic = false;
        if (TryGetComponent<Collider>(out var col))
        {
            col.isTrigger = false;
        }

        rb.isKinematic = false;

        if (TryGetComponent<UnityEngine.AI.NavMeshAgent>(out var agent))
        {
            agent.enabled = true;
        }

        if (playerMovement != null) playerMovement.SetUsingMode(false);
    }

    void Update()
    {
        if (!isBeingHeld) return;

        if (playerMovement != null && playerMovement.jumpAction.action.triggered)
        {
            Flee();
        }

        if (stunHandler != null && !stunHandler.IsStunned && playerMovement == null)
        {
            Flee();
        }
    }

    private void Flee()
    {
        transform.parent.GetComponentInParent<PlayerItemHandler>()?.HandlePickInput();

        rb.AddForce(Vector3.up * 5f, ForceMode.Impulse);
    }
}

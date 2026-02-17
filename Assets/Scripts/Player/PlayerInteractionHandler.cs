using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractionHandler : MonoBehaviour
{
    [Header("Detection Settings")]
    [SerializeField] private float interactDistance = 2.5f;
    [SerializeField] private LayerMask interactableLayers;

    [Header("References")]
    [SerializeField] private PlayerItemHandler itemHandler; 
    public InputActionReference interactAction;

    private void Update()
    {
        if (interactAction.action.WasPressedThisFrame() && !itemHandler.IsHoldingItem)
        {
            HandleStationInteraction();
        }
    }

    private void HandleStationInteraction()
    {
        Ray ray = new Ray(transform.position + new Vector3(0, 1f, 0), transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactableLayers))
        {

            IInteractable station = hit.collider.GetComponent<IInteractable>();
            station?.Interact(this.gameObject);
        }
    }
}
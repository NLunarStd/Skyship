using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerItemInteractHandler : NetworkBehaviour
{
    [Header("Detection Settings")]
    [SerializeField] private float liftDistance = 2.5f;
    [SerializeField] private Vector3 liftRayOffset;
    [SerializeField] private LayerMask interactableLayer;

    [Header("Hold Settings")]
    [SerializeField] private Transform holdAnchor;
    [SerializeField] private string itemLayerName = "Item";
    [SerializeField] private string heldItemLayerName = "HeldItem";

    [Header("Throw Settings")]
    [SerializeField] private float minThrowForce = 5f;
    [SerializeField] private float maxThrowForce = 25f;
    [SerializeField] private float chargeTimeForMaxPower = 2f;
    [SerializeField] private float upwardArcForce = 0.25f;
    [SerializeField] private float tumbleIntensity = 5f;

    [Header("Input & UI")]
    public InputActionReference pickAction;
    public Image chargeFill;
    public GameObject chargeUI;

    [Header("NGO Sync")]
    private NetworkVariable<NetworkObjectReference> heldItemRef = new NetworkVariable<NetworkObjectReference>(
        new NetworkObjectReference(),
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private GameObject currentlyHeldObject;
    private GameObject hoveredObject;
    private PickupHighlight hoveredHighlight;
    private float pickPressedTime;
    private bool isChargingThrow;

    public bool IsHoldingItem => heldItemRef.Value.TryGet(out _);

    private void Start()
    {
        if (chargeUI != null) chargeUI.SetActive(false);
    }

    private void Update()
    {
        if (!IsOwner) return;

        if (!IsHoldingItem) PerformDetection();
        else ClearHoveredState();

        HandlePickInput();
        UpdateChargeUI();
    }

    private void LateUpdate()
    {
        UpdateHeldItemVisuals();
    }

    private void UpdateHeldItemVisuals()
    {
        if (heldItemRef.Value.TryGet(out NetworkObject netObj))
        {
            GameObject item = netObj.gameObject;

            item.transform.position = holdAnchor.position;
            item.transform.rotation = holdAnchor.rotation;

            if (item.TryGetComponent<Rigidbody>(out Rigidbody rb))
            {
                if (!rb.isKinematic) rb.isKinematic = true;
            }
        }
    }

    private void PerformDetection()
    {
        Ray ray = new Ray(transform.position + liftRayOffset, transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, liftDistance, interactableLayer))
        {
            GameObject hitObj = hit.collider.gameObject;
            if (hitObj != hoveredObject)
            {
                ClearHoveredState();
                hoveredObject = hitObj;
                hoveredHighlight = hitObj.GetComponent<PickupHighlight>();
                if (hoveredHighlight != null) hoveredHighlight.SetHighlight(true);
            }
        }
        else ClearHoveredState();
    }

    private void ClearHoveredState()
    {
        if (hoveredHighlight != null) hoveredHighlight.SetHighlight(false);
        hoveredObject = null;
        hoveredHighlight = null;
    }

    private void HandlePickInput()
    {
        if (pickAction.action.WasPressedThisFrame())
        {
            if (!IsHoldingItem && hoveredObject != null)
            {
                var networkObj = hoveredObject.GetComponent<NetworkObject>();
                if (networkObj != null)
                {
                    RequestPickUpServerRpc(networkObj.NetworkObjectId);
                }
            }
            else if (IsHoldingItem)
            {
                pickPressedTime = Time.time;
                isChargingThrow = true;
            }
        }

        if (pickAction.action.WasReleasedThisFrame() && isChargingThrow)
        {
            float holdDuration = Time.time - pickPressedTime;
            isChargingThrow = false;

            if (holdDuration < 0.5f) RequestDropServerRpc();
            else
            {
                float chargeRatio = Mathf.InverseLerp(0.5f, chargeTimeForMaxPower, holdDuration);
                float finalForce = Mathf.Lerp(minThrowForce, maxThrowForce, chargeRatio);
                RequestThrowServerRpc(finalForce);
            }
        }
    }


    [ServerRpc]
    private void RequestPickUpServerRpc(ulong networkObjectId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out var networkObj))
        {
            ToggleNetworkTransformClientRpc(networkObjectId, false);
            SetItemLayerClientRpc(networkObjectId, heldItemLayerName);

            heldItemRef.Value = networkObj;

            networkObj.GetComponent<IPickable>()?.PickUp();
        }
    }

    [ServerRpc]
    private void RequestDropServerRpc()
    {
        if (!heldItemRef.Value.TryGet(out NetworkObject netObj)) return;

        ToggleNetworkTransformClientRpc(netObj.NetworkObjectId, true);
        SetItemLayerClientRpc(netObj.NetworkObjectId, itemLayerName);

        if (netObj.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.isKinematic = false;
        }

        netObj.GetComponent<IPickable>()?.Drop();
        heldItemRef.Value = new NetworkObjectReference();
    }

    [ServerRpc]
    private void RequestThrowServerRpc(float force)
    {
        if (!heldItemRef.Value.TryGet(out NetworkObject netObj)) return;

        GameObject item = netObj.gameObject;
        ToggleNetworkTransformClientRpc(netObj.NetworkObjectId, true);
        SetItemLayerClientRpc(netObj.NetworkObjectId, itemLayerName);

        if (item.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.isKinematic = false;
            Vector3 throwDir = (transform.forward + (transform.up * upwardArcForce)).normalized;
            rb.AddForce(throwDir * force, ForceMode.Impulse);
            rb.AddTorque(transform.right * tumbleIntensity, ForceMode.Impulse);
        }

        item.GetComponent<IThrowable>()?.Throw(transform.forward, force);
        heldItemRef.Value = new NetworkObjectReference();
    }

    [ClientRpc]
    private void ToggleNetworkTransformClientRpc(ulong id, bool isEnabled)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(id, out var obj))
        {
            var netTransform = obj.GetComponent<NetworkTransform>();
            if (netTransform != null) netTransform.enabled = isEnabled;
        }
    }

    private void UpdateChargeUI()
    {
        if (chargeUI == null || !IsOwner) return;
        chargeUI.SetActive(isChargingThrow);
        if (isChargingThrow)
        {
            float duration = Time.time - pickPressedTime;
            chargeFill.fillAmount = Mathf.InverseLerp(0.5f, chargeTimeForMaxPower, duration);
        }
    }
    [ClientRpc]
    private void SetItemLayerClientRpc(ulong id, string layerName)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(id, out var obj))
        {
            obj.gameObject.layer = LayerMask.NameToLayer(layerName);
        }
    }
}
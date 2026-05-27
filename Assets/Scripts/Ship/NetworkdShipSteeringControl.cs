using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

public class NetworkShipSteeringControl : NetworkBehaviour
{
    public bool ShipControlMode = false;
    public NetworkShipHandler ship;

    [Header("Settings")]
    public float rudderChangeSpeed = 60f;
    public float returnSpeed = 30f;

    [Header("Input References")]
    public InputActionReference turnLeft;
    public InputActionReference turnRight;
    public InputActionReference startShipAction;
    public InputActionReference brakeAction;

    private void OnEnable() => EventManager.Subscribe<CharacterControlRudderEvent>(OnCharacterControlRudder);
    private void OnDisable() => EventManager.UnSubscribe<CharacterControlRudderEvent>(OnCharacterControlRudder);

    void Update()
    {
        if (!ShipControlMode) return;

        if (startShipAction.action.WasPressedThisFrame()) ToggleEngineServerRpc();

        bool isBrakePressed = brakeAction.action.IsPressed();

        if (NetworkManager.Singleton.IsListening)
        {
            UpdateBrakeServerRpc(isBrakePressed);
            HandleRudderInput();
        }
    }

    private void HandleRudderInput()
    {
        float input = 0f;
        if (turnLeft.action.IsPressed()) input = -1f;
        else if (turnRight.action.IsPressed()) input = 1f;

        UpdateRudderServerRpc(input, true);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ToggleEngineServerRpc() => ship.EngineOn.Value = !ship.EngineOn.Value;

    [ServerRpc(RequireOwnership = false)]
    private void UpdateBrakeServerRpc(bool braking)
    {
        ship.IsBraking.Value = braking;
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateRudderServerRpc(float input, bool isControlling)
    {
        float currentAngle = ship.rudderAngle.Value;

        if (isControlling && input != 0)
        {
            currentAngle += input * rudderChangeSpeed * Time.deltaTime;
        }
        else
        {
            currentAngle = Mathf.MoveTowards(currentAngle, 0f, returnSpeed * Time.deltaTime);
        }

        ship.rudderAngle.Value = Mathf.Clamp(currentAngle, -ship.maxRudder, ship.maxRudder);
    }

    [ServerRpc(RequireOwnership = false)]
    public void StopShipServerRpc()
    {
        if (ship != null)
        {
            ship.EngineOn.Value = false;
            ship.IsBraking.Value = false;
            ship.ForceStop();
        }
    }

    void OnCharacterControlRudder(CharacterControlRudderEvent e)
    {
        if (e.ship != ship) return;

        ShipControlMode = e.value;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetPlayerDrivingServerRpc(ulong networkObjectId, bool isDriving)
    {
        // Execute on the Server immediately so the server's physics engine doesn't get blocked
        ApplyDrivingPhysicsState(networkObjectId, isDriving);
        
        // Sync to all clients
        SetPlayerDrivingClientRpc(networkObjectId, isDriving);
    }

    [ClientRpc]
    private void SetPlayerDrivingClientRpc(ulong networkObjectId, bool isDriving)
    {
        if (IsServer) return; // Host already ran it in ServerRpc
        ApplyDrivingPhysicsState(networkObjectId, isDriving);
    }

    private void ApplyDrivingPhysicsState(ulong networkObjectId, bool isDriving)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out var playerObj))
        {
            // Disable ALL colliders on the player so they absolutely cannot block the ship
            var cols = playerObj.GetComponentsInChildren<Collider>();
            foreach (var c in cols)
            {
                c.enabled = !isDriving;
            }
            
            var rb = playerObj.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = isDriving;
        }
    }
}
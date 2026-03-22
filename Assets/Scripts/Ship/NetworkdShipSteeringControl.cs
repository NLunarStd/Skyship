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
        if (!IsOwner) return;

        if (ShipControlMode)
        {
            if (startShipAction.action.WasPressedThisFrame()) ToggleEngineServerRpc();

            bool isBrakePressed = brakeAction.action.IsPressed();

            if (!NetworkManager.Singleton.IsListening) return;
            UpdateBrakeServerRpc(isBrakePressed);
        }
        else
        {
            if (!NetworkManager.Singleton.IsListening) return;
            UpdateBrakeServerRpc(false);
        }

        HandleRudderInput();
    }

    private void HandleRudderInput()
    {
        if (ShipControlMode)
        {
            float input = 0f;
            if (turnLeft.action.IsPressed()) input = -1f;
            else if (turnRight.action.IsPressed()) input = 1f;

            UpdateRudderServerRpc(input, true);
        }
        else
        {
            UpdateRudderServerRpc(0f, false);
        }
    }

    [ServerRpc]
    private void ToggleEngineServerRpc() => ship.EngineOn.Value = !ship.EngineOn.Value;

    [ServerRpc]
    private void UpdateBrakeServerRpc(bool braking)
    {
        ship.IsBraking.Value = braking;
    }
    [ServerRpc]
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

    void OnCharacterControlRudder(CharacterControlRudderEvent e) => ShipControlMode = e.value;
}
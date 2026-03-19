using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class NetworkShipHandler : NetworkBehaviour
{
    [Header("Engine & Speed")]
    public NetworkVariable<bool> EngineOn = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public float moveSpeed = 10f;
    public float timeToMaxSpeed = 5f;
    private float accelerationTimer = 0f;
    private float stopSpeedMultiplier = 4f;

    [Header("Rudder & Turning")]
    public NetworkVariable<float> rudderAngle = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public float maxRudder = 45f;
    public float turnSpeed = 20f;

    public NetworkVariable<bool> IsBraking = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public float brakePower = 2f; 
    [Range(0, 1)] public float minTurnAbilityAtZeroSpeed = 0.3f; 

    [Header("Height Stabilization")]
    [SerializeField] private float buoyancyStrength = 15f;
    [SerializeField] private float buoyancyDamping = 7f;
    private float initialY;

    [Header("Fake Parenting")]
    private HashSet<Rigidbody> objectsOnBoard = new HashSet<Rigidbody>();
    private Vector3 lastPosition;
    private Quaternion lastRotation;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.mass = 5000f;
        rb.useGravity = true;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            initialY = transform.position.y;
            lastPosition = rb.position;
            lastRotation = rb.rotation;
        }
    }

    void FixedUpdate()
    {
        if (!IsServer) return;

        ApplyHeightStabilization();

        Vector3 moveDelta = Vector3.zero;
        Quaternion rotationDelta = Quaternion.identity;

        if (accelerationTimer > 0 || EngineOn.Value)
        {
            if (IsBraking.Value)
                accelerationTimer = Mathf.Max(0, accelerationTimer - (Time.fixedDeltaTime * brakePower));
            else if (EngineOn.Value)
                accelerationTimer = Mathf.Min(accelerationTimer + Time.fixedDeltaTime, timeToMaxSpeed);
            else
                accelerationTimer = Mathf.Max(0, accelerationTimer - (Time.fixedDeltaTime * stopSpeedMultiplier));

            float speedPercent = Mathf.Clamp01(accelerationTimer / timeToMaxSpeed);

            moveDelta = transform.forward * (moveSpeed * speedPercent) * Time.fixedDeltaTime;

            float turnRatio = rudderAngle.Value / maxRudder;
            float turnSpeedFactor = Mathf.Max(speedPercent, minTurnAbilityAtZeroSpeed);
            float rotationAmount = turnRatio * turnSpeed * turnSpeedFactor * Time.fixedDeltaTime;
            rotationDelta = Quaternion.Euler(0f, rotationAmount, 0f);
        }


        ApplyFakeParenting(moveDelta, rotationDelta);

        rb.MovePosition(rb.position + moveDelta);
        rb.MoveRotation(rb.rotation * rotationDelta);

    }
    private void SailShip()
    {
        float speedPercent = Mathf.Clamp01(accelerationTimer / timeToMaxSpeed);

        Vector3 move = transform.forward * (moveSpeed * speedPercent) * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + move);

        float turnRatio = rudderAngle.Value / maxRudder;

        float turnSpeedFactor = Mathf.Max(speedPercent, minTurnAbilityAtZeroSpeed);

        float rotationAmount = turnRatio * turnSpeed * turnSpeedFactor * Time.fixedDeltaTime;
        Quaternion turnRotation = Quaternion.Euler(0f, rotationAmount, 0f);
        rb.MoveRotation(rb.rotation * turnRotation);
    }

    private void ApplyHeightStabilization()
    {
        float displacement = initialY - rb.position.y;
        float springForce = displacement * buoyancyStrength;
        float dampeningForce = rb.linearVelocity.y * buoyancyDamping;
        rb.AddForce(Vector3.up * (springForce - dampeningForce), ForceMode.Acceleration);
    }

    private void ApplyFakeParenting(Vector3 moveDelta, Quaternion rotationDelta)
    {
        foreach (Rigidbody target in objectsOnBoard)
        {
            if (target == null) continue;

            Vector3 relativePos = target.position - rb.position;
            Vector3 rotatedPos = rotationDelta * relativePos;

            Vector3 finalPos = rb.position + rotatedPos + moveDelta;

            target.MovePosition(finalPos);
            target.MoveRotation(rotationDelta * target.rotation);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        if (other.TryGetComponent<Rigidbody>(out Rigidbody targetRb))
        {
            objectsOnBoard.Add(targetRb);

            other.gameObject.layer = LayerMask.NameToLayer("HeldItem");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsServer) return;

        if (other.TryGetComponent<Rigidbody>(out Rigidbody targetRb))
        {
            if (objectsOnBoard.Contains(targetRb))
            {
                objectsOnBoard.Remove(targetRb);
                other.gameObject.layer = LayerMask.NameToLayer("Default");

                SteeringInteract steering = GetComponentInChildren<SteeringInteract>();

                if (steering != null && steering.IsCurrentController(other.gameObject))
                {
                    steering.ExitShipControl();
                    EngineOn.Value = false;
                    IsBraking.Value = false;

                    EventManager.Publish(new CharacterControlRudderEvent
                    {
                        value = false
                    });
                }
            }
        }
    }
}
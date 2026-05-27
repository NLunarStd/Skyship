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
    public float reverseSpeedMultiplier = 0.5f;

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
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    private Vector3 initialPosition;
    private Quaternion initialRotation;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            initialY = transform.position.y;
            initialPosition = transform.position;
            initialRotation = transform.rotation;
            lastPosition = rb.position;
            lastRotation = rb.rotation;
        }
    }

    public void ResetShip()
    {
        if (!IsServer) return;
        ForceStop();
        EngineOn.Value = false;
        IsBraking.Value = false;
        rudderAngle.Value = 0f;
        
        rb.position = initialPosition;
        rb.rotation = initialRotation;
        lastPosition = initialPosition;
        lastRotation = initialRotation;
        
        objectsOnBoard.Clear();
    }

    public void ForceStop()
    {
        accelerationTimer = 0f;
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    void FixedUpdate()
    {
        if (!IsServer) return;

        // Force strictly upright to fix the tilt bug
        Vector3 currentEuler = rb.rotation.eulerAngles;
        if (Mathf.Abs(currentEuler.x) > 0.01f || Mathf.Abs(currentEuler.z) > 0.01f)
        {
            rb.rotation = Quaternion.Euler(0f, currentEuler.y, 0f);
        }

        // Calculate actual deltas from the LAST physics step for accurate fake parenting
        Vector3 actualMoveDelta = rb.position - lastPosition;
        Quaternion actualRotationDelta = rb.rotation * Quaternion.Inverse(lastRotation);

        ApplyFakeParenting(actualMoveDelta, actualRotationDelta, lastPosition);

        lastPosition = rb.position;
        lastRotation = rb.rotation;

        if (EngineOn.Value || Mathf.Abs(accelerationTimer) > 0.01f)
        {
            if (rb.IsSleeping()) rb.WakeUp();

            if (IsBraking.Value && EngineOn.Value)
            {
                // Brake and then go into reverse (timer goes negative)
                accelerationTimer = Mathf.Max(-timeToMaxSpeed * reverseSpeedMultiplier, accelerationTimer - (Time.fixedDeltaTime * brakePower));
            }
            else if (EngineOn.Value)
            {
                // Accelerate forward
                accelerationTimer = Mathf.Min(accelerationTimer + Time.fixedDeltaTime, timeToMaxSpeed);
            }
            else
            {
                // Engine off, naturally slow down to 0 from either direction
                if (accelerationTimer > 0)
                    accelerationTimer = Mathf.Max(0, accelerationTimer - (Time.fixedDeltaTime * stopSpeedMultiplier));
                else if (accelerationTimer < 0)
                    accelerationTimer = Mathf.Min(0, accelerationTimer + (Time.fixedDeltaTime * stopSpeedMultiplier));
            }
        }

        // Allow speedPercent to be negative for reverse
        float speedPercent = Mathf.Clamp(accelerationTimer / timeToMaxSpeed, -reverseSpeedMultiplier, 1f);

        // 1. Use Velocity instead of MovePosition to stop getting "sucked" into walls
        Vector3 targetVelocity = transform.forward * (moveSpeed * speedPercent);
        rb.linearVelocity = new Vector3(targetVelocity.x, 0f, targetVelocity.z);

        // 2. Use Angular Velocity for smooth turning without physics glitches
        float turnRatio = rudderAngle.Value / maxRudder;
        // Use Abs(speedPercent) so you can still turn while reversing
        float turnSpeedFactor = Mathf.Max(Mathf.Abs(speedPercent), minTurnAbilityAtZeroSpeed);
        float rotationAmountDeg = turnRatio * turnSpeed * turnSpeedFactor; 
        
        rb.angularVelocity = new Vector3(0f, rotationAmountDeg * Mathf.Deg2Rad, 0f);
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
        // Physics height stabilization disabled since Rigidbody Y is frozen
    }

    private void ApplyFakeParenting(Vector3 moveDelta, Quaternion rotationDelta, Vector3 prevShipPos)
    {
        // Don't interfere if the ship is completely still.
        if (moveDelta.sqrMagnitude < 0.000001f && Quaternion.Angle(rotationDelta, Quaternion.identity) < 0.001f)
        {
            return;
        }

        foreach (Rigidbody target in objectsOnBoard)
        {
            if (target == null) continue;

            // DO NOT fake parent kinematic objects! Their position is managed by NetworkTransform/Scripts.
            // Setting linearVelocity on kinematic rigidbodies can crash or lock up the Unity Physics solver!
            if (target.isKinematic) continue;

            // Use Velocity for fake parenting! This allows normal gravity to work perfectly.
            Vector3 relativePos = target.position - rb.position;
            Vector3 tangentialVel = Vector3.Cross(rb.angularVelocity, relativePos);

            Vector3 targetVel = rb.linearVelocity + tangentialVel;

            // Match ship horizontal speed, but keep the item's own vertical speed (Gravity)
            target.linearVelocity = new Vector3(targetVel.x, target.linearVelocity.y, targetVel.z);
            target.angularVelocity = rb.angularVelocity;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!IsServer) return;

        Rigidbody targetRb = other.attachedRigidbody;
        if (targetRb != null && targetRb != rb)
        {
            if (!objectsOnBoard.Contains(targetRb))
            {
                objectsOnBoard.Add(targetRb);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsServer) return;

        Rigidbody targetRb = other.attachedRigidbody;
        if (targetRb != null && targetRb != rb)
        {
            if (objectsOnBoard.Contains(targetRb))
            {
                SteeringInteract steering = GetComponentInChildren<SteeringInteract>();

                // Prevent false-positive exit triggers when the player toggles isKinematic to drive
                if (steering != null && steering.IsCurrentController(targetRb.gameObject))
                {
                    return; // Ignore the exit, they are driving and can't actually fall off
                }

                // Check ALL colliders on the ship to find the actual trigger and verify intersection
                Collider[] shipColliders = GetComponentsInChildren<Collider>();
                foreach (Collider col in shipColliders)
                {
                    if (col.isTrigger && col.bounds.Intersects(other.bounds))
                    {
                        return; // Still intersecting with the ship's trigger! False alarm, do not remove!
                    }
                }

                objectsOnBoard.Remove(targetRb);
            }
        }
    }
}
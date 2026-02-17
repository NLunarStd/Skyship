using UnityEngine;
using static GameEnum;

public class TurretAI : MonoBehaviour
{
    [Header("Detection Settings")]
    public float detectionRange = 10f;
    public float viewAngle = 45f; // Total cone width
    public LayerMask targetMask;
    public Faction targetFaction;
    [Header("Visual Reference")]
    public TurretFOVVisual fovVisual;
    private bool _lastSpottedState = false;


    private ITargetable currentTarget;

    void Update()
    {
        if (currentTarget != null)
        {
            if (!currentTarget.IsActive() || !IsTargetInFOV(currentTarget))
            {
                currentTarget = null;
            }
        }

        // 2. Search if empty
        if (currentTarget == null)
        {
            FindTarget();
        }

        // 3. ONLY update the visual if the state CHANGED
        bool currentlyHasTarget = currentTarget != null;
        if (currentlyHasTarget != _lastSpottedState)
        {
            _lastSpottedState = currentlyHasTarget;
            if (fovVisual) fovVisual.SetSpottedState(currentlyHasTarget);
            Debug.Log($"<color=red>Turret State Changed:</color> Target Spotted = {currentlyHasTarget}");
        }
    }

    private void FindTarget()
    {
        Collider[] potentialTargets = Physics.OverlapSphere(transform.position, detectionRange, targetMask);

        foreach (var col in potentialTargets)
        {
            if (col.TryGetComponent<ITargetable>(out var target))
            {
                // 1. Check Faction
                if (target.GetFaction() != targetFaction)
                {
                    Debug.Log($"{col.name} ignored: Faction mismatch ({target.GetFaction()} vs {targetFaction})");
                    continue;
                }

                // 2. Check FOV
                if (!IsTargetInFOV(target))
                {
                    Debug.Log($"{col.name} ignored: Not in FOV angle or Line of Sight blocked");
                    continue;
                }

                // 3. Success
                currentTarget = target;
                if (fovVisual) fovVisual.SetSpottedState(true);
                Debug.Log($"<color=green>SUCCESS:</color> {gameObject.name} spotted {target.GetTransform().name}!");
                return;
            }
            else
            {
                Debug.Log($"{col.name} ignored: No ITargetable component found");
            }
        }
    }

    private bool IsTargetInFOV(ITargetable target)
    {
        // 1. Calculate Positions with vertical and forward offsets
        // Start slightly in front of the turret to avoid hitting itself
        Vector3 startPos = transform.position + (transform.forward * 0.5f) + (Vector3.up * 1.2f);
        Vector3 endPos = target.GetTransform().position + (Vector3.up * 1.0f);

        // 2. Direction check
        Vector3 dirToTarget = (target.GetTransform().position - transform.position).normalized;
        float dot = Vector3.Dot(transform.forward, dirToTarget);
        float angleThreshold = Mathf.Cos(viewAngle * 0.5f * Mathf.Deg2Rad);

        if (dot >= angleThreshold)
        {
            // 3. Linecast with LayerMask (IMPORTANT)
            // We use ~targetMask to hit everything EXCEPT the players/enemies
            // This way, the character's own body won't "block" the turret
            if (!Physics.Linecast(startPos, endPos, out RaycastHit hit, ~targetMask))
            {
                return true; // Clear line of sight to the target
            }
            else
            {
                // Optional: Debug line to see what is blocking the view
                Debug.DrawLine(startPos, hit.point, Color.yellow);
                // Debug.Log($"View blocked by: {hit.collider.name}");
            }
        }
        return false;
    }
}
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class TurretFOVVisual : MonoBehaviour
{
    [Header("Connections")]
    [Tooltip("Drag the main Turret AI script here so this visual can read its settings automatically.")]
    [SerializeField] private TurretAI turretData;

    [Header("Visual Settings")]
    [Tooltip("How smooth the curved arc should be. Higher is smoother but costs more performance.")]
    [SerializeField] private int resolution = 30;
    [Tooltip("Lift the lines slightly off the ground to prevent z-fighting flickering.")]
    [SerializeField] private float groundOffset = 0.1f;
    [SerializeField] private Color safeColor = Color.green;
    [SerializeField] private Color spotColor = Color.red;

    private LineRenderer lr;

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        // Important settings for the LineRenderer
        lr.useWorldSpace = false; // We draw relative to the turret
        lr.loop = true; // Automatically connects the last point back to the start point
        lr.positionCount = resolution + 2; // +2 for the center point and the end edge
    }

    void Start()
    {
        // If the reference is missing, try to find it on the same object or parent
        if (turretData == null)
        {
            turretData = GetComponentInParent<TurretAI>();
        }

        if (turretData == null)
        {
            Debug.LogError("TurretFOVVisual: No TurretAI script found! Cannot draw FOV.");
            enabled = false;
            return;
        }

        // Set initial color
        SetColor(safeColor);
    }

    // Use LateUpdate so the visual follows the turret *after* it has finished rotating for the frame
    void LateUpdate()
    {
        if (turretData == null) return;

        DrawFOV();
    }

    private void DrawFOV()
    {
        // Get latest data from the AI script
        float angle = turretData.viewAngle;
        float range = turretData.detectionRange;

        Vector3[] points = new Vector3[resolution + 2];

        // Point 0 is always the center of the turret (slightly raised)
        points[0] = Vector3.up * groundOffset;

        // Calculate the starting angle (half the total angle to the left)
        float currentAngle = -angle / 2f;
        // Calculate how much to increase angle per step
        float angleStep = angle / resolution;

        for (int i = 1; i <= resolution + 1; i++)
        {
            // Math to convert an angle into a direction vector (Trigonometry)
            // We are working in local space, where forward is Z+
            float rad = Mathf.Deg2Rad * currentAngle;
            // x = sin(angle), z = cos(angle) gives us rotation around Y axis starting from forward Z
            Vector3 direction = new Vector3(Mathf.Sin(rad), 0, Mathf.Cos(rad));

            // Scale by range and add offset
            points[i] = (direction * range) + (Vector3.up * groundOffset);

            currentAngle += angleStep;
        }

        // Apply points to the LineRenderer
        lr.SetPositions(points);
    }

    // Public method the AI can call to change color when it spots someone
    public void SetSpottedState(bool isSpotted)
    {
        SetColor(isSpotted ? spotColor : safeColor);
    }

    private void SetColor(Color c)
    {
        lr.startColor = c;
        lr.endColor = c;
    }
}
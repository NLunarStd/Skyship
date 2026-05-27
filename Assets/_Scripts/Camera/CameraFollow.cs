using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(100)]
public class CameraFollow : MonoBehaviour
{
    public Transform target;

    [Header("Distance & Zoom")]
    public float distance = 6f;
    public float height = 2f;
    public float minDistance = 2f;
    public float maxDistance = 15f;
    public float zoomSpeed = 0.01f;

    [Header("Mouse Look")]
    public float mouseSensitivity = 0.2f;

    [Header("Smooth")]
    public float smoothSpeed = 10f;

    [Header("Vertical Rotation")]
    public float minPitch = -20f;
    public float maxPitch = 70f;

    private float yaw;
    private float pitch = 20f;

    void Start()
    {

    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector2 mouseDelta = Mouse.current.delta.ReadValue();
        yaw += mouseDelta.x * mouseSensitivity;
        pitch -= mouseDelta.y * mouseSensitivity;

        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        float scroll = Mouse.current.scroll.ReadValue().y;
        if (Mathf.Abs(scroll) > 0.01f)
        {
            distance -= scroll * zoomSpeed;
            distance = Mathf.Clamp(distance, minDistance, maxDistance);
        }

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);

        Vector3 desiredPosition =
            target.position
            - rotation * Vector3.forward * distance
            + Vector3.up * height;

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            smoothSpeed * Time.deltaTime
        );

        Vector3 lookTarget = target.position + Vector3.up * 1.5f;

        transform.rotation = Quaternion.LookRotation(
            lookTarget - transform.position
        );
    }
}
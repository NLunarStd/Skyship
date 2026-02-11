using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;

    public Vector3 offset = new Vector3(0, 10, -8);
    public float moveSmooth = 5f;

    public float rotateSpeed = 360f;   // องศาต่อวินาที
    public float topDownAngle = 50f;

    private float currentYAngle = 0f;
    private float targetYAngle = 0f;

    [Header("Zoom Offsets")]
    public Vector3 farOffset = new Vector3(0, 14, -14);
    public Vector3 nearOffset = new Vector3(0, 6, -5);

    [Header("Zoom Angles")]
    public float farAngle = 65f;
    public float nearAngle = 30f;

    [Header("Zoom")]
    public float zoom = 0f;
    public float zoomSpeed = 5f;
    public float minZoom = 0f;
    public float maxZoom = 1f;


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
            targetYAngle -= 90f;

        if (Input.GetKeyDown(KeyCode.E))
            targetYAngle += 90f;

        float scroll = Input.mouseScrollDelta.y;
        zoom += scroll * zoomSpeed * Time.deltaTime;
        zoom = Mathf.Clamp(zoom, minZoom, maxZoom);
    }

    void LateUpdate()
    {
        if (target == null) return;

        // หมุนซ้ายขวา
        currentYAngle = Mathf.MoveTowardsAngle(
            currentYAngle,
            targetYAngle,
            rotateSpeed * Time.deltaTime
        );

        // ? lerp offset ตาม zoom
        Vector3 offset = Vector3.Lerp(farOffset, nearOffset, zoom);

        // หมุนตามแกน Y
        Quaternion rot = Quaternion.Euler(0, currentYAngle, 0);
        Vector3 rotatedOffset = rot * offset;

        // ตำแหน่งกล้อง
        Vector3 desiredPosition = target.position + rotatedOffset;

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            moveSmooth * Time.deltaTime
        );

        // ? lerp มุมก้ม
        float angle = Mathf.Lerp(farAngle, nearAngle, zoom);
        transform.rotation = Quaternion.Euler(angle, currentYAngle, 0);
    }



}

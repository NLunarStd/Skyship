using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement_old : MonoBehaviour
{
    public float moveSpeed = 5f;
    public Transform cameraTransform;

    private Rigidbody rb;
    private Vector3 movement;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        // ทิศของกล้อง
        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;

        // ไม่ให้ตัวละครลอยขึ้นตามมุมกล้อง
        camForward.y = 0;
        camRight.y = 0;

        camForward.Normalize();
        camRight.Normalize();

        movement = (camForward * v + camRight * h).normalized;
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);

        // หันหน้าไปทิศที่เดิน
        if (movement != Vector3.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(movement);
            rb.rotation = Quaternion.Slerp(rb.rotation, toRotation, 0.2f);
        }     
    }
}
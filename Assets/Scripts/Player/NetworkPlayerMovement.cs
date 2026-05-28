using System.Collections;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.VirtualTexturing;

[RequireComponent(typeof(Rigidbody))]
public class NetworkPlayerMovement : NetworkBehaviour
{
    [Header("Sound Effects")]
    public AudioClip JumpSfx;

    [Header("Animator")]
    public Animator animator;

    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 9f;
    public float rotationSpeed = 10f;
    public float jumpForce = 8f;
    public float groundDrag = 5f;

    [Header("Jump Settings")]
    public float jumpCooldown = 1.2f;
    private float nextJumpTime;

    [Header("State Flags")]
    public bool isUsingMode = false;

    [Header("Input Action Reference")]
    public InputActionReference moveAction;
    public InputActionReference jumpAction;
    public InputActionReference sprintAction;

    private Rigidbody rb;
    private Vector2 moveInput;
    private bool sprintPressed;

    private bool isTeleporting = false;

    private float jumpPower;
    private float jumpPowerScale;
    private float jumpPowTimer;


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        if (!IsOwner)
        {
            rb.isKinematic = true;
            return;
        }

        CameraFollow cam = FindFirstObjectByType<CameraFollow>();

        if (cam != null)
        {
            cam.target = transform;
        }
    }

    void Update()
    {
        if (!IsOwner) return;

        if (!isUsingMode)
        {
            moveInput = moveAction.action.ReadValue<Vector2>();
            sprintPressed = sprintAction.action.IsPressed();

            // RunAnimation
            //animator.SetBool("Run", moveInput != Vector2.zero);

            jumpPowTimer += Time.deltaTime;
            // Charge jump
            if (jumpAction.action.WasPressedThisFrame() && Time.time >= nextJumpTime)
            {
                jumpPowTimer = 0;
                //JumpAnimation();
            }

            if (jumpAction.action.WasReleasedThisFrame() && Time.time >= nextJumpTime)
            {
                jumpPower = (jumpPowTimer/4 + 1) * jumpForce;
                HandleJump(jumpPower);
                nextJumpTime = Time.time + jumpCooldown;
                //JumpAnimation();
            }
            
            // Original jump
            //  if (jumpAction.action.triggered && Time.time >= nextJumpTime)
            //  {
            //      
            //      HandleJump(jumpPower);
            //      nextJumpTime = Time.time + jumpCooldown;
            //  }
        }
        else
        {
            moveInput = Vector2.zero;
        }

        ControlDrag();
    }

    void FixedUpdate()
    {
        if (!IsOwner || isUsingMode || isTeleporting) return;

        MovePlayer();
        RotatePlayer();
    }

    void JumpAnimation()
    {
        animator.SetTrigger("Jump");
        Debug.Log("JumpAnimation Called!");
    }

    void PlayJumpSFX()
    {
        SFXManager.Instance.PlaySFX2D(JumpSfx);

        //PlayJumpSoundServerRpc(transform.position);
    }

    [ServerRpc]
    private void PlayJumpSoundServerRpc(Vector3 pos)
    {
        PlayJumpSoundClientRpc(pos);
    }
    [ClientRpc]
    private void PlayJumpSoundClientRpc(Vector3 pos)
    {
        // ถ้าเป็นเครื่องตัวเอง ไม่ต้องเล่นซ้ำ (เพราะเล่นแบบ 2D ไปแล้ว)
        if (IsOwner) return;
        // เล่นเสียงกระโดดของเพื่อน แบบ 3D (ใครอยู่ไกลก็จะได้ยินเบาลง)
        SFXManager.Instance.PlaySFXAtPosition(JumpSfx, pos);
    }

    private void MovePlayer()
    {
        float targetSpeed = sprintPressed ? sprintSpeed : walkSpeed;

        Vector3 forward = Vector3.forward;
        Vector3 right = Vector3.right;

        // เช็คว่าถ้าอยู่ใน Lobby ให้ใช้กล้อง Lobby ไม่งั้นใช้ Camera.main
        if (GameManager.instance != null && GameManager.instance.IsInLobby() && GameManager.instance.lobbyCam != null)
        {
            forward = GameManager.instance.lobbyCam.transform.forward;
            right = GameManager.instance.lobbyCam.transform.right;
        }
        else
        {
            Camera cam = Camera.main;
            if (cam == null) cam = FindFirstObjectByType<Camera>();

            if (cam != null)
            {
                forward = cam.transform.forward;
                right = cam.transform.right;
            }
        }

        // กันตัวละครเดินลอยขึ้นฟ้า
        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        Vector3 moveDirection =
            forward * moveInput.y +
            right * moveInput.x;

        moveDirection.Normalize();

        Vector3 targetVelocity = moveDirection * targetSpeed;
        Vector3 currentVelocity = rb.linearVelocity;

        rb.linearVelocity = new Vector3(
            targetVelocity.x,
            currentVelocity.y,
            targetVelocity.z
        );
    }

    private void RotatePlayer()
    {
        Vector3 moveDirection = new Vector3(
            rb.linearVelocity.x,
            0f,
            rb.linearVelocity.z
        );

        if (moveDirection.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(moveDirection);

            rb.MoveRotation(
                Quaternion.Slerp(
                    rb.rotation,
                    targetRotation,
                    rotationSpeed * Time.fixedDeltaTime
                )
            );
        }
    }

    private void HandleJump(float jumpPower)
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(transform.up * jumpPower, ForceMode.Impulse);
        PlayJumpSFX();
        JumpAnimation();
    }

    private void ControlDrag()
    {
        rb.linearDamping = groundDrag;
    }

    public void SetUsingMode(bool value)
    {
        isUsingMode = value;
    }

    public IEnumerator TeleportRoutine(Vector3 targetPos)
    {
        isTeleporting = true;

        var netTransform = GetComponent<NetworkTransform>();

        // ?? ปิด interpolation
        netTransform.Interpolate = false;

        // ?? หยุดแรงทั้งหมด
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // ?? วาร์ป (ใช้ rb แทน transform)
        rb.position = targetPos;

        // กัน physics บัค
        rb.Sleep();

        yield return new WaitForSeconds(0.15f);

        rb.WakeUp();

        // ?? เปิด interpolation กลับ
        netTransform.Interpolate = true;

        isTeleporting = false;
    }

    [ClientRpc]
    public void ForceExitShipClientRpc()
    {
        if (!IsOwner) return;

        var steerings = FindObjectsOfType<SteeringInteract>();
        foreach (var steering in steerings)
        {
            if (steering.IsCurrentController(gameObject))
            {
                steering.ExitShipControl();
                break;
            }
        }
    }
}
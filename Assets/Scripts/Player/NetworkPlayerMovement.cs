using System.Collections;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.VirtualTexturing;

[RequireComponent(typeof(Rigidbody))]
public class NetworkPlayerMovement : NetworkBehaviour
{
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
        }
    }

    void Update()
    {
        if (!IsOwner) return;

        if (!isUsingMode)
        {
            moveInput = moveAction.action.ReadValue<Vector2>();
            sprintPressed = sprintAction.action.IsPressed();
            jumpPowTimer += Time.deltaTime;
            // Charge jump
            if (jumpAction.action.WasPressedThisFrame() && Time.time >= nextJumpTime)
            {
                jumpPowTimer = 0;
            }

            if (jumpAction.action.WasReleasedThisFrame())
            {
                jumpPower = (jumpPowTimer/10 + 1) * jumpForce;
                HandleJump(jumpPower);
                nextJumpTime = Time.time + jumpCooldown;
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

    private void MovePlayer()
    {
        float targetSpeed = sprintPressed ? sprintSpeed : walkSpeed;
        Vector3 moveDirection = new Vector3(moveInput.x, 0f, moveInput.y).normalized;

        if (transform.parent != null)
        {
            moveDirection = transform.parent.TransformDirection(moveDirection);
        }
        Vector3 targetVelocity = moveDirection * targetSpeed;
        Vector3 currentVelocity = rb.linearVelocity;

        rb.linearVelocity = new Vector3(targetVelocity.x, currentVelocity.y, targetVelocity.z);
    }

    private void RotatePlayer()
    {
        Vector3 direction = new Vector3(moveInput.x, 0f, moveInput.y);
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime));
        }
    }

    private void HandleJump(float jumpPower)
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
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

        // ?? �Դ interpolation
        netTransform.Interpolate = false;

        // ?? ��ش�ç������
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // ?? ���� (�� rb ����� transform)
        rb.position = targetPos;

        // �ѹ physics ��
        rb.Sleep();

        yield return new WaitForSeconds(0.15f);

        rb.WakeUp();

        // ? �Դ interpolation ��Ѻ
        netTransform.Interpolate = true;

        isTeleporting = false;
    }
}
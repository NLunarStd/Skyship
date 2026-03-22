using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.InputSystem;

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

            if (jumpAction.action.triggered && Time.time >= nextJumpTime)
            {
                HandleJump();
                nextJumpTime = Time.time + jumpCooldown; 
            }
        }
        else
        {
            moveInput = Vector2.zero;
        }

        ControlDrag();
    }

    void FixedUpdate()
    {
        if (!IsOwner || isUsingMode) return;

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

    private void HandleJump()
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

}
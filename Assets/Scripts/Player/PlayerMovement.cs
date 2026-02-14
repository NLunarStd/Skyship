using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 9f;
    public float rotationSpeed = 10f;
    public float jumpForce = 8f;
    public float groundDrag = 5f;

    [Header("State Flags")]
    public bool isUsingMode = false;
    public bool isGrounded;
    public bool isStunned = false; 

    [Header("Detection")]
    public float playerHeight = 2f;
    public LayerMask groundLayer;

    private Rigidbody rb;
    private Vector2 moveInput;
    private bool sprintPressed;

    [Header("Ladder Settings")]
    public float climbSpeed = 5f;
    public bool isClimbing { get; private set; }


    [Header("Input Action Reference")]
    public InputActionReference moveAction;
    public InputActionReference jumpAction;
    public InputActionReference sprintAction;
    private void OnEnable()
    {
        EventManager.Subscribe<CharacterStunnedEvent>(OnPlayerStunned);
    }

    private void OnDisable()
    {
        EventManager.UnSubscribe<CharacterStunnedEvent>(OnPlayerStunned);
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    void Update()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, groundLayer);

        if (!isUsingMode && !isStunned)
        {
            moveInput = moveAction.action.ReadValue<Vector2>();
            sprintPressed = sprintAction.action.IsPressed();

            if (jumpAction.action.triggered && isGrounded)
            {
                HandleJump();
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
        if (!isUsingMode && !isStunned)
        {
            HandleMovement();
        }
        else if (isStunned)
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        }
    }
    private void HandleMovement()
    {
        if (isClimbing)
        {
            // Ignore normal movement and gravity
            float verticalInput = moveAction.action.ReadValue<Vector2>().y;
            rb.linearVelocity = new Vector3(0, verticalInput * climbSpeed, 0);

            // Exit if jump is pressed
            if (jumpAction.action.triggered) ExitLadder();
            return;
        }

        MovePlayer();
        RotatePlayer();
    }
    public void EnterLadder(Transform ladderTransform)
    {
        isClimbing = true;
        rb.useGravity = false;
        rb.linearVelocity = Vector3.zero;

        Vector3 flatForward = Vector3.ProjectOnPlane(ladderTransform.forward, Vector3.up); 
        transform.rotation = Quaternion.LookRotation(ladderTransform.forward);
    }

    public void ExitLadder()
    {
        isClimbing = false;
        rb.useGravity = true;
    }
    private void MovePlayer()
    {
        bool canSprint = sprintPressed && isGrounded;

        float targetSpeed = canSprint ? sprintSpeed : walkSpeed;
        Vector3 moveDirection = new Vector3(moveInput.x, 0f, moveInput.y).normalized;

        rb.AddForce(moveDirection * targetSpeed * 10f, ForceMode.Force);

        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        if (flatVel.magnitude > targetSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * targetSpeed;
            rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
        }
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
        if (isGrounded)
            rb.linearDamping = groundDrag;
        else
            rb.linearDamping = 0;
    }

    public void SetUsingMode(bool value)
    {
        isUsingMode = value;
    }
    private IEnumerator StunRoutine(float duration)
    {
        isStunned = true;

        if (isClimbing) ExitLadder();

        yield return new WaitForSeconds(duration);

        isStunned = false;
    }
    private void OnPlayerStunned(CharacterStunnedEvent e)
    {
        if (e.Victim == this.gameObject)
        {
            StartCoroutine(StunRoutine(e.Duration));
        }
    }
}

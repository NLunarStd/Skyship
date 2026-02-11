using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class CharacterMovementForTesting : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 8f;
    public float jumpHeight = 1.5f;
    public float gravity = -9.81f;

    [Header("State Flags (For Testing)")]
    public bool isUsingMode = false; 
    public bool isGrounded;          

    private CharacterController controller;
    private Vector2 moveInput;
    private Vector3 velocity;
    private bool sprintPressed;

    [Header("Input Action Reference")]
    public InputActionReference moveAction;
    public InputActionReference jumpAction;
    public InputActionReference sprintAction;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        if (!isUsingMode)
        {
            HandleMovement();
            HandleJump();
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void HandleMovement()
    {
        moveInput = moveAction.action.ReadValue<Vector2>();
        sprintPressed = sprintAction.action.IsPressed();

        float currentSpeed = sprintPressed && isGrounded ? sprintSpeed : walkSpeed;

        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        controller.Move(move * currentSpeed * Time.deltaTime);
    }

    private void HandleJump()
    {
        if (jumpAction.action.triggered && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

            if (sprintPressed)
            {
                Debug.Log("Jumping with Sprint force!");
            }
        }
    }

    public void SetUsingMode(bool value)
    {
        isUsingMode = value;
        if (isUsingMode) moveInput = Vector2.zero; 
    }
}
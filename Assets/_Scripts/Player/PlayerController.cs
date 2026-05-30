using UnityEngine;

/// <summary>
/// Player movement controller for 3D third-person.
/// Reads input from InputManager, moves a CharacterController
/// relative to the camera and rotates the visual model in a
/// "strafing" style (character always looks where the camera looks).
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Player stats component (health, move speed, jump force, etc.).")]
    [SerializeField] private PlayerStats playerStats;

    [Tooltip("Camera transform used as reference for movement (usually main camera or Cinemachine virtual camera).")]
    [SerializeField] private Transform cameraTransform;

    [Tooltip("Root transform of the visual model (rotates to face camera).")]
    [SerializeField] private Transform visualRoot;

    [Header("Movement & Physics")]
    [Tooltip("Gravity value (negative).")]
    [SerializeField] private float gravity = -9.81f;

    [Tooltip("Small downward velocity to keep the character grounded.")]
    [SerializeField] private float groundedGravity = -2f;

    [Tooltip("Speed multiplier when sprinting.")]
    [SerializeField] private float sprintMultiplier = 1.5f;

    private CharacterController characterController;
    private Vector3 verticalVelocity;
    private bool isGrounded;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();

        if (playerStats == null)
            playerStats = GetComponent<PlayerStats>();

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;
    }

    private void Update()
    {
        if (InputManager.Instance == null)
            return;

        HandleMovement();
        HandleJump();

        // Emit sound when moving or running
        EmitSound();

        InputManager.Instance.ResetButtonFlags();
    }

    private void HandleMovement()
    {
        Vector2 moveInput = InputManager.Instance.MoveInput;
        Vector3 moveDirection = Vector3.zero;

        if (moveInput.sqrMagnitude > 0.001f && cameraTransform != null)
        {
            Vector3 forward = cameraTransform.forward;
            forward.y = 0f;
            forward.Normalize();

            Vector3 right = cameraTransform.right;
            right.y = 0f;
            right.Normalize();

            moveDirection = forward * moveInput.y + right * moveInput.x;
            moveDirection.Normalize();
        }

        float speed = 5f;
        float rotationSpeed = 720f;

        if (playerStats != null && playerStats.playerData != null)
        {
            speed = playerStats.playerData.moveSpeed;
            rotationSpeed = playerStats.playerData.rotationSpeed;
        }

        if (InputManager.Instance.IsSprintHeld())
        {
            speed *= sprintMultiplier;
        }

        Vector3 horizontalVelocity = moveDirection * speed;

        isGrounded = characterController.isGrounded;

        if (isGrounded && verticalVelocity.y < 0f)
        {
            verticalVelocity.y = groundedGravity;
        }

        verticalVelocity.y += gravity * Time.deltaTime;

        Vector3 velocity = horizontalVelocity + verticalVelocity;

        characterController.Move(velocity * Time.deltaTime);

        if (cameraTransform != null && visualRoot != null)
        {
            Vector3 cameraForward = cameraTransform.forward;
            cameraForward.y = 0f;
            cameraForward.Normalize();

            if (cameraForward.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(cameraForward);
                visualRoot.rotation = Quaternion.Slerp(
                    visualRoot.rotation,
                    targetRotation,
                    rotationSpeed * Mathf.Deg2Rad * Time.deltaTime
                );
            }
        }
    }

    private void HandleJump()
    {
        if (!isGrounded)
            return;

        if (InputManager.Instance.IsJumpPressed())
        {
            float jumpForce = 5f;

            if (playerStats != null && playerStats.playerData != null)
            {
                jumpForce = playerStats.playerData.jumpForce;
            }

            verticalVelocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
        }
    }

    private void EmitSound()
    {
        if (InputManager.Instance == null || EventBus.Instance == null)
            return;

        Vector2 moveInput = InputManager.Instance.MoveInput;
        bool isSprinting = InputManager.Instance.IsSprintHeld();

        if (moveInput.sqrMagnitude > 0.001f)
        {
            float soundDuration = isSprinting ? 3f : 1f;
            bool isrunning = isSprinting && moveInput.y > 0.5f; // Consider it running if sprinting forward
            EventBus.Instance.TriggerPlayerMadeSound(transform.position, soundDuration, isrunning);
        }
    }
}
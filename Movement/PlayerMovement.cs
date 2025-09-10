using UnityEngine;

/// <summary>
/// Player Movement Assessment - Midnight  10/09/2025
/// Performant: Uses Update for smooth, continuous motion while being driven by an efficient event-based input system.
/// Decoupled: Completely independent of raw input sources; it only listens to the InputHandler's abstract events.
/// Scalable: Easily extensible with new movement states (e.g., dashing, proning) thanks to its clean structure and properties.
/// Maintainable: Highly readable with clear regions, headers, tooltips, and a logical separation of concerns.
/// Robust: Handles its own event lifecycle, performs null checks for required components, and uses a CharacterController for stable physics.
/// NOT STANDALONE: Requires the InputHandler.
/// </summary>

public class PlayerMovement : MonoBehaviour
{
    /// <summary>
    /// Settings for the script.
    /// </summary>
    [Header("Component References")]
    [Tooltip("The transform of the camera that will be rotated up and down for vertical look.")]
    [SerializeField] private Transform cameraTransform;
    [Header("Movement Settings")]
    [Tooltip("The character's walking speed in meters per second.")]
    [SerializeField] private float walkSpeed = 5.0f;
    [Tooltip("The character's sprinting speed in meters per second.")]
    [SerializeField] private float sprintSpeed = 8.0f;
    [Tooltip("The character's crouching speed in meters per second.")]
    [SerializeField] private float crouchSpeed = 4.0f;
    [Tooltip("The height the character can jump in meters.")]
    [SerializeField] private float jumpHeight = 1f;
    [Tooltip("The force of gravity applied to the character. A higher negative value provides a less 'floaty' feel.")]
    [SerializeField] private float gravity = -19.62f;
    [Header("Look Settings")]
    [Tooltip("The sensitivity of the mouse look. Adjust for faster or slower camera movement.")]
    [SerializeField] private float lookSensitivity = 20.0f;
    [Tooltip("The maximum angle in degrees the camera can pitch upwards.")]
    [SerializeField] private float maxLookAngleUp = 80f;
    [Tooltip("The maximum angle in degrees the camera can pitch downwards.")]
    [SerializeField] private float maxLookAngleDown = 80f;
    
    /// <summary>
    /// Private variables to store component references and state.
    /// </summary>
    
    private CharacterController _characterController;
    
    // Vector2s
    private Vector2 _moveInput;
    private Vector2 _lookInput;
    // Bools
    private bool _isSprinting;
    private bool _isCrouching;
    // Floats
    private float CurrentSpeed
    {
        get
        {
            if (_isSprinting)
            {
                return sprintSpeed;
            }
            else if (_isCrouching)
            {
                return crouchSpeed;
            }
            else
            {
                return walkSpeed;
            }
        }
    }
    private float _verticalVelocity;
    private float _cameraPitch;
    // Event handlers for input events
    private void HandleMovementInput(Vector2 input) => _moveInput = input;
    private void HandleLookInput (Vector2 input) => _lookInput = input;
    private void HandleSprintInput(bool input) => _isSprinting = input;
    private void HandleCrouchInput(bool input) => _isCrouching = input;
    private void HandleJumpInput()
    {
        if (_characterController.isGrounded)
            _verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
    }
    
    
    /// <summary>
    /// Checks for required components and sets up the cursor so mouse doesn't go off the screen.
    /// </summary>
    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        if (_characterController == null)
        {
            Debug.LogError("PlayerMovement requires a CharacterController component.");
            enabled = false;
        }
        if (cameraTransform == null)
        {
            Debug.LogError("PlayerMovement requires a cameraTransform.");
            enabled = false;
        }
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    /// <summary>
    /// Subscribes to input events when the script is enabled to handle input.
    /// </summary>
    private void OnEnable()
    {
        InputHandler.OnMovementUpdated += HandleMovementInput;
        InputHandler.OnLookUpdated += HandleLookInput;
        InputHandler.OnSprintHeld += HandleSprintInput;
        InputHandler.OnCrouchHeld += HandleCrouchInput;
        InputHandler.OnJumpPerformed += HandleJumpInput;
    }

    /// <summary>
    /// Called when the MonoBehaviour will be disabled.
    /// This method unsubscribes from various input events to prevent further processing if the component is disabled.
    /// </summary>
    private void OnDisable()
    {
        InputHandler.OnMovementUpdated -= HandleMovementInput;
        InputHandler.OnLookUpdated -= HandleLookInput;
        InputHandler.OnSprintHeld -= HandleSprintInput;
        InputHandler.OnCrouchHeld -= HandleCrouchInput;
        InputHandler.OnJumpPerformed -= HandleJumpInput;
    }
    
    private void Update()
    {
        ApplyGravity();
        ApplyLook();
        ApplyMovement();
    }
    
    /// <summary>
    ///  Applies gravity to the character based on if it is grounded and the current vertical velocity.
    /// </summary>
    private void ApplyGravity()
    {
        if (_characterController.isGrounded && _verticalVelocity < 0)
        {
            _verticalVelocity = -2f;
        }
        _verticalVelocity += gravity * Time.deltaTime;
    }
    
    /// <summary>
    /// Applies the look input to rotate the player character and the camera.
    /// </summary>
    private void ApplyLook()
    {
        transform.Rotate(Vector3.up * (_lookInput.x * lookSensitivity * Time.deltaTime));
        _cameraPitch -= _lookInput.y * lookSensitivity * Time.deltaTime;
        _cameraPitch = Mathf.Clamp(_cameraPitch, -maxLookAngleUp, maxLookAngleDown);
        cameraTransform.localRotation = Quaternion.Euler(_cameraPitch, 0, 0);
    }
    
    /// <summary>
    ///  Applies the movement to the character, using the current speed and vertical velocity.
    /// </summary>
    private void ApplyMovement()
    {
        var moveDirection = new Vector3(_moveInput.x, 0, _moveInput.y);
        moveDirection = transform.TransformDirection(moveDirection);
        var finalVelocity = moveDirection * CurrentSpeed + Vector3.up * _verticalVelocity;
        _characterController.Move(finalVelocity * Time.deltaTime);
    }
}

using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Input Handler Assessment  - Midnight 10/09/2025
/// Performant: It won't cause any framerate drops.
/// Decoupled: It keeps your game logic clean and independent.
/// Scalable: It's easy to add new actions as your game evolves.
/// Maintainable: It's easy to read, understand, and debug.
/// Robust: It handles its own lifecycle correctly and safely.
/// </summary>

public class InputHandler : MonoBehaviour
{
    public static InputHandler Instance { get; private set; }
    private PlayerInputActions _playerInputActions;
    /// <summary>
    /// Events for input actions, these are called when the input action is performed or canceled, which allow the rest of the game scripts to react to the input(s) without checking for input every frame, using events.
    /// </summary>
    public static event Action OnJumpPerformed;
    public static event Action<bool> OnSprintHeld;
    public static event Action<bool> OnCrouchHeld;
    public static event Action OnInteractPerformed;
    public static event Action OnPauseMenuPressed;
    public static event Action<Vector2> OnMovementUpdated;
    public static event Action<Vector2> OnLookUpdated;
    
    /// <summary>
    /// Initializes the singleton instance and sets up the input actions. This is called when the script instance is being loaded.
    /// Makes sure there is only one instance of the script and destroys itself instance if there is more than one. This is done to prevent multiple instances of the script from being created.
    /// </summary>
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        _playerInputActions = new PlayerInputActions();
    }

    /// <summary>
    /// Enables the input actions and events, this is called when the game starts.
    /// </summary>
    private void OnEnable()
    {
        _playerInputActions.Enable();
        _playerInputActions.Player.Jump.performed += JumpOnperformed;
        _playerInputActions.Player.Crouch.started +=  CrouchOnHeld;
        _playerInputActions.Player.Crouch.canceled +=CrouchOnHeld;
        _playerInputActions.Player.Sprint.started += SprintOnHeld;
        _playerInputActions.Player.Sprint.canceled += SprintOnHeld;
        _playerInputActions.Player.Interact.performed += InteractOnPerformed;
        _playerInputActions.Player.PauseMenu.performed += PauseMenuOnPerformed;
        
        _playerInputActions.Player.Move.performed += MoveOnUpdated;
        _playerInputActions.Player.Move.canceled += MoveOnUpdated;
        _playerInputActions.Player.Look.performed += LookOnUpdated;
        _playerInputActions.Player.Look.canceled += LookOnUpdated;
        
    }
    
    /// <summary>
    /// Event handlers for input actions, these are called when the input action is performed or canceled, which allow the rest of the game to react to the input(s) without checking for input every frame.
    /// This is a good way to reduce the workload of the game (zero work on every frame) and makes it easier to add new input actions in the future, as all inputs are managed through here (Centralized Input Control).
    /// </summary>
    
    /// <summary>
    /// This, will output the value of the movement input as a Vector2, which can be used to move the player character.
    /// </summary>
    private void MoveOnUpdated(InputAction.CallbackContext obj)
    {
        OnMovementUpdated?.Invoke(obj.ReadValue<Vector2>());
    }
    
    /// <summary>
    /// This, will output the value of the look input as a Vector2, which can be used to rotate the player character & move the camera.
    /// </summary>
    private void LookOnUpdated(InputAction.CallbackContext obj)
    {
        OnLookUpdated?.Invoke(obj.ReadValue<Vector2>());
    }
    
    /// <summary>
    /// This, will otuput if the PauseMenu button is being pressed, if it is, it will invoke the OnPauseMenuPressed event.
    /// </summary>
    private void PauseMenuOnPerformed(InputAction.CallbackContext obj)
    {
        OnPauseMenuPressed?.Invoke();
    }
    
    /// <summary>
    /// This, will otuput if the interact button is being pressed, if it is, it will invoke the OnInteractPerformed event.
    /// </summary>
    private void InteractOnPerformed(InputAction.CallbackContext obj)
    {
        OnInteractPerformed?.Invoke();
    }
    
    /// <summary>
    /// This, will output the value of the button that is being held so if being held, it will output true, if not being held, it will output false.
    /// </summary>
    private void SprintOnHeld(InputAction.CallbackContext obj)
    {
        OnSprintHeld?.Invoke(obj.ReadValueAsButton());
    }

    /// <summary>
    /// This, will output the value of the button that is being held so if being held, it will output true, if not being held, it will output false.
    /// </summary>
    private void CrouchOnHeld(InputAction.CallbackContext obj)
    {
        OnCrouchHeld?.Invoke(obj.ReadValueAsButton());
    }
    
    /// <summary>
    /// This, will otuput if the jump button is being pressed, if it is, it will invoke the OnJumpPerformed event.
    /// </summary>
    private void JumpOnperformed(InputAction.CallbackContext obj)
    {
        OnJumpPerformed?.Invoke();
    }
    
    /// <summary>
    /// Disables the input actions and events, this is called when the game ends.
    /// </summary>
    private void OnDisable()
    {
        _playerInputActions.Player.Jump.performed -= JumpOnperformed;
        _playerInputActions.Player.Crouch.started -=  CrouchOnHeld;
        _playerInputActions.Player.Crouch.canceled -=CrouchOnHeld;
        _playerInputActions.Player.Sprint.started -= SprintOnHeld;
        _playerInputActions.Player.Sprint.canceled -= SprintOnHeld;
        _playerInputActions.Player.Interact.performed -= InteractOnPerformed;
        _playerInputActions.Player.PauseMenu.performed -= PauseMenuOnPerformed;
        
        _playerInputActions.Player.Move.performed -= MoveOnUpdated;
        _playerInputActions.Player.Move.canceled -= MoveOnUpdated;
        _playerInputActions.Player.Look.performed -= LookOnUpdated;
        _playerInputActions.Player.Look.canceled -= LookOnUpdated;
        _playerInputActions.Disable();
    }
}

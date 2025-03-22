using System;
using FishNet.Object;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCharacterMovement : NetworkBehaviour
{
    // Maximum movement speed of this character.
    public float MaxSpeed;

    // Reference to the character's Rigidbody2D.
    [SerializeField]
    Rigidbody2D _rigidBody;

    // Reference to InputAction for character movement.
    InputAction _moveAction;
    // Reference to InputAction for character rotation.
    InputAction _lookAction;

    // The most recent movement input from the client controlling this character.
    Vector2 _recentMoveInput;
    // The most recent desired rotation for this character.
    float _recentDesiredRotation;

    // Is the component subscribed to the action?
    bool _isSubscribedToAction = false;

    void Awake()
    {
        if (_rigidBody == null)
        {
            Debug.Log("\"rigidBody\" wasn't set.");
            throw new Exception();
        }
        _moveAction = InputSystem.actions.FindAction("Move");
        if (_moveAction == null)
        {
            Debug.Log("\"Move\" action wasn't found.");
            throw new Exception();
        }
        _lookAction = InputSystem.actions.FindAction("Look");
        if (_lookAction == null)
        {
            Debug.Log("\"Look\" action wasn't found.");
            throw new Exception();
        }
    }

    public override void OnStartClient()
    {
        if (base.IsOwner)
        {
            // We are the owning client of this character. Subscribe movement functions to the action.
            SubscribeToAction();
        }
    }

    public override void OnStopClient()
    {
        // We don't check for ownership here, since calling `UnsubscribeFromAction()` when we are not subscribed shouldn't cause any problems.
        UnsubscribeFromAction();
    }

    void OnEnable()
    {
        if (base.IsOwner)
        {
            // We are the owning client of this character. Subscribe movement functions to the action.
            // We need this functionality because we unsubscribe on disable.
            SubscribeToAction();
        }
    }

    void OnDisable()
    {
        // We don't check for ownership here, since calling `UnsubscribeFromAction()` when we are not subscribed shouldn't cause any problems.
        UnsubscribeFromAction();
    }

    void SubscribeToAction()
    {
        if (!_isSubscribedToAction)
        {
            _moveAction.performed += OnMoveAction;
            _moveAction.canceled += OnCancel;
            _lookAction.performed += OnLookAction;
            _isSubscribedToAction = true;
        }
    }
    void UnsubscribeFromAction()
    {
        if (_isSubscribedToAction)
        {
            _moveAction.performed -= OnMoveAction;
            _moveAction.canceled -= OnCancel;
            _lookAction.performed -= OnLookAction;
            _isSubscribedToAction = false;
        }
    }

    // Bound to `moveAction.performed`.
    // Sends the keyboard input to the server.
    void OnMoveAction(InputAction.CallbackContext context)
    {
        Vector2 moveInput = context.ReadValue<Vector2>();
        SendMoveInputRpc(moveInput);
    }

    // Bound to `moveAction.canceled`.
    // Sends the keyboard input to the server.
    // This is required since `moveAction.performed` won't be triggered when the action value is set to zero.
    void OnCancel(InputAction.CallbackContext context)
    {
        Vector2 moveInput = context.ReadValue<Vector2>();
        SendMoveInputRpc(moveInput);
    }

    // Bound to `lookAction.performed`. Sends the desired rotation to the server.
    void OnLookAction(InputAction.CallbackContext context)
    {
        Vector2 mousePosition = context.ReadValue<Vector2>();
        Camera mainCam = Camera.main; // Since this function is called only on the owning client, we surely have a main camera.
        if (mainCam == null)
        {
            // This shouldn't happen, but check just to make sure.
            Debug.Log("\"Look\" action was triggered, but no main cameras were found.");
            return;
        }
        Vector3 screenPosition = new Vector3(mousePosition.x, mousePosition.y, -mainCam.transform.position.z);
        Vector3 worldPosition = mainCam.ScreenToWorldPoint(screenPosition);
        Vector3 viewDirection = worldPosition - transform.position;
        float rotation = Mathf.Atan2(viewDirection.y, viewDirection.x) * Mathf.Rad2Deg - 90;
        SendDesiredRotationRpc(rotation);
    }

    // RPC to set `recentMoveInput` on the server. The value will be read in `FixedUpdate()` for physics-based movement.
    [ServerRpc]
    void SendMoveInputRpc(Vector2 moveInput)
    {
        _recentMoveInput = moveInput;
    }

    // RPC to set `recentDesiredRotation` on the server. The value will be read in `FixedUpdate()` for rotation.
    [ServerRpc]
    void SendDesiredRotationRpc(float desiredRotation)
    {
        _recentDesiredRotation = desiredRotation;
    }

    void FixedUpdate()
    {
        if (!IsServerInitialized)
        {
            // If we are not the server, return. Movement will be taken care there, and be synced over.
            return;
        }
        // Else, we are the server.
        Vector2 moveDirection = _recentMoveInput;
        _rigidBody.linearVelocity = moveDirection * MaxSpeed;

        _rigidBody.MoveRotation(_recentDesiredRotation);
    }
}

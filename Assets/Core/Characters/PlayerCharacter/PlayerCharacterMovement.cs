using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCharacterMovement : NetworkBehaviour
{
    // Maximum movement speed of this character.
    public float maxSpeed;

    // Reference to the character's Rigidbody2D.
    [SerializeField]
    Rigidbody2D rigidBody;

    // Reference to InputAction for character movement.
    InputAction moveAction;
    // Reference to InputAction for character rotation.
    InputAction lookAction;

    // The most recent movement input from the client controlling this character.
    Vector2 recentMoveInput;
    // The most recent desired rotation for this character.
    float recentDesiredRotation;

    // Is the component subscribed to the action?
    bool isSubscribedToAction = false;

    void Awake()
    {
        if (rigidBody == null)
        {
            Debug.Log("\"rigidBody\" wasn't set.");
            throw new Exception();
        }
        moveAction = InputSystem.actions.FindAction("Move");
        if (moveAction == null)
        {
            Debug.Log("\"Move\" action wasn't found.");
            throw new Exception();
        }
        lookAction = InputSystem.actions.FindAction("Look");
        if (lookAction == null)
        {
            Debug.Log("\"Look\" action wasn't found.");
            throw new Exception();
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsOwner)
        {
            // We are the owner of this character. Attach movement functions to the action.
            SubscribeToAction();
        }
    }

    void OnEnable()
    {
        if (IsSpawned && IsOwner)
        {
            // If we are the owner and the network object is spawned, subscribe to actions.
            // We need this functionality because we unsubscribe on disable.
            SubscribeToAction();
        }
    }

    void OnDisable()
    {
        UnsubscribeFromAction();
    }

    void SubscribeToAction()
    {
        if (!isSubscribedToAction)
        {
            moveAction.performed += OnMoveAction;
            moveAction.canceled += OnCancel;
            lookAction.performed += OnLookAction;
            isSubscribedToAction = true;
        }
    }
    void UnsubscribeFromAction()
    {
        if (isSubscribedToAction)
        {
            moveAction.performed -= OnMoveAction;
            moveAction.canceled -= OnCancel;
            lookAction.performed -= OnLookAction;
            isSubscribedToAction = false;
        }
    }

    // Bound to `moveAction.performed`.
    // Sends the keyboard input to the authority.
    void OnMoveAction(InputAction.CallbackContext context)
    {
        Vector2 moveInput = context.ReadValue<Vector2>();
        SendMoveInputRpc(moveInput);
    }

    // Bound to `moveAction.canceled`.
    // Sends the keyboard input to the authority.
    // This is required since `moveAction.performed` won't be triggered when the action value is set to zero.
    void OnCancel(InputAction.CallbackContext context)
    {
        Vector2 moveInput = context.ReadValue<Vector2>();
        SendMoveInputRpc(moveInput);
    }

    // Bound to `lookAction.performed`. Sends the desired rotation to the authority.
    void OnLookAction(InputAction.CallbackContext context)
    {
        Vector2 mousePosition = context.ReadValue<Vector2>();
        Camera mainCam = Camera.main; // Since this function is called only on the owner, we surely have a main camera.
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

    // RPC to set `recentMoveInput` on the authority. The value will be read in `FixedUpdate()` for physics-based movement.
    [Rpc(SendTo.Authority)]
    void SendMoveInputRpc(Vector2 moveInput)
    {
        recentMoveInput = moveInput;
    }

    // RPC to set `recentDesiredRotation` on the authority. The value will be read in `FixedUpdate()` for rotation.
    [Rpc(SendTo.Authority)]
    void SendDesiredRotationRpc(float desiredRotation)
    {
        recentDesiredRotation = desiredRotation;
    }

    void FixedUpdate()
    {
        if (!HasAuthority)
        {
            // If we are not the authority, return. Movement will be taken care there, and be synced over.
            return;
        }
        // Else, we are the authority.
        Vector2 moveDirection = recentMoveInput;
        rigidBody.linearVelocity = moveDirection * maxSpeed;

        rigidBody.MoveRotation(recentDesiredRotation);
    }
}

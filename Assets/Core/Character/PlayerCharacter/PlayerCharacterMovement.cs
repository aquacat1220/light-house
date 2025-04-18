using System;
using FishNet.Object;
using FishNet.Object.Prediction;
using FishNet.Transporting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCharacterMovement : NetworkBehaviour
{
    public struct ReplicateData : IReplicateData
    {
        public ReplicateData(Vector2 moveInput, float desiredRotation)
        {
            MoveInput = moveInput;
            DesiredRotation = desiredRotation;
            _tick = 0;
        }

        public Vector2 MoveInput;

        public float DesiredRotation;

        private uint _tick;

        public void Dispose() { }

        public uint GetTick() => _tick;
        public void SetTick(uint value) => _tick = value;
    }

    public struct ReconcileData : IReconcileData
    {
        public ReconcileData(PredictionRigidbody2D predictionRigidbody2D)
        {
            PredictionRigidbody2D = predictionRigidbody2D;
            _tick = 0;
        }

        public PredictionRigidbody2D PredictionRigidbody2D;

        private uint _tick;

        public void Dispose() { }

        public uint GetTick() => _tick;
        public void SetTick(uint value) => _tick = value;
    }

    public PredictionRigidbody2D PredictionRigidbody2D;

    // Maximum movement speed of this character.
    [SerializeField]
    float _maxSpeed;

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

    // Is the component subscribed to timemanager callbacks?
    bool _isSubscribedToTimeManager = false;

    // Is the component subscribed to input actions?
    bool _isSubscribedToInputActions = false;


    void Awake()
    {
        if (_rigidBody == null)
        {
            Debug.Log("`rigidBody` wasn't set.");
            throw new Exception();
        }
        PredictionRigidbody2D = new PredictionRigidbody2D();
        PredictionRigidbody2D.Initialize(_rigidBody);

        _moveAction = InputSystem.actions.FindAction("Move");
        if (_moveAction == null)
        {
            Debug.Log("`Move` action wasn't found.");
            throw new Exception();
        }
        _lookAction = InputSystem.actions.FindAction("Look");
        if (_lookAction == null)
        {
            Debug.Log("`Look` action wasn't found.");
            throw new Exception();
        }
    }

    public override void OnStartNetwork()
    {
        SubscribeToTimeManager();
    }

    public override void OnStopNetwork()
    {
        UnsubscribeFromTimeManager();
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
            SubscribeToTimeManager();
        }
    }

    void OnDisable()
    {
        // We don't check for ownership here, since calling `UnsubscribeFromAction()` when we are not subscribed shouldn't cause any problems.
        UnsubscribeFromAction();
        UnsubscribeFromTimeManager();
    }

    void SubscribeToAction()
    {
        if (!_isSubscribedToInputActions)
        {
            _moveAction.performed += OnMoveAction;
            _moveAction.canceled += OnCancel;
            _lookAction.performed += OnLookAction;
            _isSubscribedToInputActions = true;
        }
    }

    void UnsubscribeFromAction()
    {
        if (_isSubscribedToInputActions)
        {
            _moveAction.performed -= OnMoveAction;
            _moveAction.canceled -= OnCancel;
            _lookAction.performed -= OnLookAction;
            _isSubscribedToInputActions = false;
        }
    }

    void SubscribeToTimeManager()
    {
        if (!_isSubscribedToTimeManager)
        {
            base.TimeManager.OnTick += OnTimeManagerTick;
            base.TimeManager.OnPostTick += OnTimeManagerPostTick;
            _isSubscribedToTimeManager = true;
        }
    }

    void UnsubscribeFromTimeManager()
    {
        if (_isSubscribedToTimeManager)
        {
            base.TimeManager.OnTick -= OnTimeManagerTick;
            base.TimeManager.OnPostTick -= OnTimeManagerPostTick;
            _isSubscribedToTimeManager = false;
        }
    }

    private void OnTimeManagerTick()
    {
        Replicate(CreateReplicate());
    }

    private void OnTimeManagerPostTick()
    {
        CreateReconcile();
    }

    [Replicate]
    private void Replicate(ReplicateData data, ReplicateState state = ReplicateState.Invalid, Channel channel = Channel.Unreliable)
    {
        if (!state.ContainsCreated())
        {
            // `data` isn't created by the owner; it is a default object provided by FishNet.
            // Return early so rotation doesn't have to snap.
            // This means we don't do any input prediction: we assume the owning client wouldn't be pressing any inputs when we can't see them.
            return;
        }
        Vector2 moveDirection = data.MoveInput.normalized;
        PredictionRigidbody2D.Velocity(moveDirection * _maxSpeed);

        PredictionRigidbody2D.Rotation(data.DesiredRotation);
        // float rotationAmount = data.DesiredRotation - _rigidBody.rotation;
        // PredictionRigidbody2D.AngularVelocity(rotationAmount / (float)base.TimeManager.TickDelta);
        PredictionRigidbody2D.Simulate();
    }

    private ReplicateData CreateReplicate()
    {
        // If non-owning, return default. FishNet will automatically supply the correct values.
        if (!base.IsOwner)
        {
            return default;
        }

        ReplicateData data = new ReplicateData(_recentMoveInput, _recentDesiredRotation);
        return data;
    }

    [Reconcile]
    private void Reconcile(ReconcileData data, Channel channel = Channel.Unreliable)
    {
        PredictionRigidbody2D.Reconcile(data.PredictionRigidbody2D);
    }


    public override void CreateReconcile()
    {
        ReconcileData data = new ReconcileData(PredictionRigidbody2D);
        Reconcile(data);
    }

    // Bound to `moveAction.performed`.
    // Sends the keyboard input to the server.
    void OnMoveAction(InputAction.CallbackContext context)
    {
        Vector2 moveInput = context.ReadValue<Vector2>();
        _recentMoveInput = moveInput;
    }

    // Bound to `moveAction.canceled`.
    // Sends the keyboard input to the server.
    // This is required since `moveAction.performed` won't be triggered when the action value is set to zero.
    void OnCancel(InputAction.CallbackContext context)
    {
        Vector2 moveInput = context.ReadValue<Vector2>();
        _recentMoveInput = moveInput;
    }

    // Bound to `lookAction.performed`. Sends the desired rotation to the server.
    void OnLookAction(InputAction.CallbackContext context)
    {
        Vector2 mousePosition = context.ReadValue<Vector2>();
        Camera mainCam = Camera.main; // Since this function is called only on the owning client, we surely have a main camera.
        if (mainCam == null)
        {
            // This shouldn't happen, but check just to make sure.
            Debug.Log("`Look` action was triggered, but no main cameras were found.");
            return;
        }
        Vector3 screenPosition = new Vector3(mousePosition.x, mousePosition.y, -mainCam.transform.position.z);
        Vector3 worldPosition = mainCam.ScreenToWorldPoint(screenPosition);
        Vector3 viewDirection = worldPosition - transform.position;
        float rotation = Mathf.Atan2(viewDirection.y, viewDirection.x) * Mathf.Rad2Deg - 90;
        _recentDesiredRotation = rotation;
    }
}

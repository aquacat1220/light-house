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
        public ReplicateData(Vector2 moveInput, float angularVelocity)
        {
            MoveInput = moveInput;
            AngularVelocity = angularVelocity;
            _tick = 0;
        }

        public Vector2 MoveInput;

        public float AngularVelocity;

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

    // The most recent movement input from the client controlling this character.
    Vector2 _recentMoveInput;
    // The most recent desired angular velocity for this character.
    float _recentAngularVelocity;

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
        // And call `ResetInputs()` to make sure past inputs don't stay in affect during disabled periods.
        ResetInputs();

        // Unsubscribing from time manager will disrupt client side prediction, resulting in desynced positions.
        // UnsubscribeFromTimeManager();
    }

    void SubscribeToAction()
    {
        if (!_isSubscribedToInputActions)
        {
            var inputManager = InputManager.Singleton;
            if (inputManager == null)
            {
                Debug.Log("`InputManager.Singleton` is null, suggesting an `InputManager` wasn't present in the scene.");
                throw new Exception();
            }
            inputManager.MoveAction += OnMoveAction;
            inputManager.LookAction += OnLookAction;
            _isSubscribedToInputActions = true;
        }
    }

    void UnsubscribeFromAction()
    {
        if (_isSubscribedToInputActions)
        {
            var inputManager = InputManager.Singleton;
            if (inputManager == null)
            {
                Debug.Log("`InputManager.Singleton` is null, suggesting an `InputManager` wasn't present in the scene.");
                throw new Exception();
            }
            inputManager.MoveAction -= OnMoveAction;
            inputManager.LookAction -= OnLookAction;
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


            // {
            //     // Return early so rotation doesn't have to snap.
            //     // Rigidbody has inertia: it will keep its velocities from previous ticks.
            //     // Thus, even without explicit input prediction, this early return implicitly predicts that
            //     // *unobserved inputs* will be the same as the *last observed input*. (See devlog of 2025-04-19 12:13:32 for detail.)
            //     // While this may be desirable in certain cases, this results in a *sudden jolt* when inputs change,
            //     // since we are effectively extrapolating into the future.
            //     return;
            // }

            {
                // Zero out the rigidbody velocity to stop extrapolation.
                PredictionRigidbody2D.Velocity(Vector2.zero);
                // But leave the rotation as it is, since it has nothing to do with inertia.
                PredictionRigidbody2D.Simulate();
                return;
            }
        }
        // `data` is created by the owner.
        Debug.Log($"{data.AngularVelocity}");
        Vector2 localMoveDirection = data.MoveInput;
        Vector2 worldMoveDirection = transform.TransformDirection(localMoveDirection).normalized;
        PredictionRigidbody2D.Velocity(worldMoveDirection * _maxSpeed);
        // Since rigidbody has rotation frozen, we should directly set the rotation, instead of setting angular velocity.
        PredictionRigidbody2D.Rotation(_rigidBody.rotation + data.AngularVelocity * (float)TimeManager.TickDelta);
        PredictionRigidbody2D.Simulate();
    }

    private ReplicateData CreateReplicate()
    {
        // If non-owning, return default. FishNet will automatically supply the correct values.
        if (!base.IsOwner)
        {
            return default;
        }

        ReplicateData data = new ReplicateData(_recentMoveInput, _recentAngularVelocity);
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

    // Bound to `InputManager.Singleton.MoveAction`.
    // Sets `_recentMoveInput` to reflect the input.
    void OnMoveAction(InputAction.CallbackContext context)
    {
        // Any call to this handler will contain the latest value of the input, regardless of action phase.
        Vector2 moveInput = context.ReadValue<Vector2>();
        _recentMoveInput = moveInput;
    }

    // Bound to `InputManager.Singleton.LookAction`.
    // Sets `_recentAngularVelocity` to reflect the input.
    void OnLookAction(InputAction.CallbackContext context)
    {
        Vector2 mouseDelta = context.ReadValue<Vector2>();
        Debug.Log($"OnLookAction: {mouseDelta}");
        float mouseDeltaX = mouseDelta.x;
        _recentAngularVelocity = -mouseDeltaX * 20f;
    }

    // Reset recent movement input to zero.
    void ResetInputs()
    {
        _recentMoveInput = Vector2.zero;
        _recentAngularVelocity = 0f;
    }
}

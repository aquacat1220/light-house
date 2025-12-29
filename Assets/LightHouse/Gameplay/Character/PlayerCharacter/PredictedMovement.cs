namespace LightHouse
{
    using System;
    using FishNet.Managing.Timing;
    using FishNet.Object;
    using FishNet.Object.Prediction;
    using FishNet.Transporting;
    using UnityEngine;
    using Fn;

    public class PredictedMovement : NetworkBehaviour
    {
        public struct ReplicateData : IReplicateData
        {
            public ReplicateData(Vector2 worldMove, float worldRotation)
            {
                WorldMoveX = worldMove.x;
                WorldMoveY = worldMove.y;
                WorldRotation = worldRotation;

                _tick = 0;
            }

            // public Vector2 WorldMove;

            public Vector2 WorldMove
            {
                get { return new Vector2(WorldMoveX, WorldMoveY); }
                // set
                // {
                //     WorldMoveX = value.x;
                //     WorldMoveY = value.y;
                // }
            }
            public float WorldMoveX;
            public float WorldMoveY;

            public float WorldRotation;

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

        PredictionRigidbody2D _predictionRigidbody2D;

        // Maximum movement speed of this character.
        [SerializeField]
        float _maxSpeed;

        // Reference to the character's Rigidbody2D.
        [SerializeField]
        Rigidbody2D _rigidBody;

        // The most recent movement input from the client controlling this character.
        Vector2 _recentMoveInput;
        // The most recent desired angular velocity for this character.
        float _accumulatedMouseDeltaX;

        // Is the component subscribed to timemanager callbacks?
        bool _isSubscribedToTimeManager = false;

        // Is the component subscribed to input actions?
        bool _isInputBlocked = true;


        void Awake()
        {
            if (_rigidBody == null)
            {
                Debug.Log("`rigidBody` wasn't set.");
                throw new Exception();
            }
            _predictionRigidbody2D = new PredictionRigidbody2D();
            _predictionRigidbody2D.Initialize(_rigidBody);
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
            if (base.isActiveAndEnabled && base.IsOwner)
            {
                // We are the owning client of this character. Subscribe movement functions to the action.
                AllowInputs();
            }
        }

        public override void OnStopClient()
        {
            // We don't check for ownership here, since calling `UnsubscribeFromAction()` when we are not subscribed shouldn't cause any problems.
            BlockInputs();
            // And call `ResetInputs()` to make sure past inputs don't stay in affect during disabled periods.
            ResetInputs();
        }

        void OnEnable()
        {
            if (base.IsClientInitialized && base.IsOwner)
            {
                // We are the owning client of this character. Allow inputs to control the character.
                // We need this functionality because we unsubscribe on disable.
                AllowInputs();
            }
        }

        void OnDisable()
        {
            // We don't check for ownership here, since calling `BlockInputs()` when we are not subscribed shouldn't cause any problems.
            BlockInputs();
            // And call `ResetInputs()` to make sure past inputs don't stay in affect during disabled periods.
            ResetInputs();

            // Unsubscribing from time manager will disrupt client side prediction, resulting in desynced positions.
            // UnsubscribeFromTimeManager();
        }

        void AllowInputs()
        {
            if (_isInputBlocked)
            {
                _isInputBlocked = false;
            }
        }

        void BlockInputs()
        {
            if (!_isInputBlocked)
            {
                _isInputBlocked = true;
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
                    _predictionRigidbody2D.Velocity(Vector2.zero);
                    // But leave the rotation as it is, since it has nothing to do with inertia.
                    _predictionRigidbody2D.Simulate();
                    return;
                }
            }
            // `data` is created by the owner.
            Vector2 worldMove = data.WorldMove;
            if (worldMove.magnitude > 1f + 0.001f)
            {
                // We add a small margin of error, since tiny errors can happen in floating point operations.
                Debug.Log($"`data.WorldMove.magnitude > 1f` with a value of {worldMove.magnitude}, this might be an attempt for speed hacking.");
                worldMove.Normalize();
            }
            _predictionRigidbody2D.Velocity(worldMove * _maxSpeed);
            // Since rigidbody has rotation frozen, we should directly set the rotation, instead of setting angular velocity.
            _predictionRigidbody2D.Rotation(data.WorldRotation);
            _predictionRigidbody2D.Simulate();
        }

        private ReplicateData CreateReplicate()
        {
            // If non-owning, return default. FishNet will automatically supply the correct values.
            if (!base.IsOwner)
            {
                return default;
            }

            Vector2 localMove = _recentMoveInput;
            float pr = Math.Min(1f, localMove.magnitude);
            Vector2 worldMove = transform.TransformDirection(localMove).normalized * pr;

            float worldRotation = transform.eulerAngles.z - (5.0f) * (float)TimeManager.TickDelta * _accumulatedMouseDeltaX;

            ReplicateData data = new ReplicateData(worldMove, worldRotation);
            _accumulatedMouseDeltaX = 0f;
            return data;
        }

        [Reconcile]
        private void Reconcile(ReconcileData data, Channel channel = Channel.Unreliable)
        {
            _predictionRigidbody2D.Reconcile(data.PredictionRigidbody2D);
        }


        public override void CreateReconcile()
        {
            ReconcileData data = new ReconcileData(_predictionRigidbody2D);
            Reconcile(data);
        }

        // Called to notify movement input change.
        // Sets `_recentMoveInput` to reflect the input.
        [Client(RequireOwnership = true)]
        public void OnMove(Vector2 moveInput)
        {
            // If input is blocked, ignore it.
            if (_isInputBlocked) { return; }
            _recentMoveInput = moveInput;
        }

        // Called to notify look input change.
        // Sets `_accumulatedMouseDeltaX` to reflect the input.
        [Client(RequireOwnership = true)]
        public void OnLook(Vector2 lookInput)
        {
            // If input is blocked, ignore it.
            if (_isInputBlocked) { return; }
            float mouseDeltaX = lookInput.x;
            _accumulatedMouseDeltaX += mouseDeltaX;
        }

        // Reset recent movement input to zero.
        void ResetInputs()
        {
            _recentMoveInput = Vector2.zero;
            _accumulatedMouseDeltaX = 0f;
        }
    }
}

namespace LightHouse
{
    using System;
    using System.Collections.Generic;
    using FishNet.Managing.Timing;
    using FishNet.Object;
    using FishNet.Object.Prediction;
    using FishNet.Transporting;
    using UnityEngine;

    public struct VelocityOverrideEffect
    {
        public VelocityOverrideEffect(Vector2 velocityOverride)
        {
            VelocityOverride = velocityOverride;
        }

        public Vector2 VelocityOverride;
    }

    public class PredictedMovement : NetworkBehaviour
    {
        public struct ReplicateData : IReplicateData
        {
            public ReplicateData(Vector2 worldMove, float worldRotation, uint tick)
            {
                WorldMoveX = worldMove.x;
                WorldMoveY = worldMove.y;
                WorldRotation = worldRotation;
                Tick = tick;

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
            public uint Tick;

            private uint _tick;

            public void Dispose() { }

            public uint GetTick() => _tick;
            public void SetTick(uint value) => _tick = value;
        }

        public struct ReconcileData : IReconcileData
        {
            public ReconcileData(PredictionRigidbody2D predictionRigidbody2D, uint tick)
            {
                PredictionRigidbody2D = predictionRigidbody2D;
                Tick = tick;
                _tick = 0;
            }

            public PredictionRigidbody2D PredictionRigidbody2D;
            public uint Tick;

            private uint _tick;

            public void Dispose() { }

            public uint GetTick() => _tick;
            public void SetTick(uint value) => _tick = value;
        }

        public class MovementEffectEntry
        {
            public VelocityOverrideEffect Effect;
            public uint AddedTick;
            public uint StartTick;
            public uint? EndTick;

            public MovementEffectEntry(VelocityOverrideEffect effect, uint addedTick, uint startTick, uint? endTick)
            {
                Effect = effect;
                AddedTick = addedTick;
                StartTick = startTick;
                EndTick = endTick;
            }
        }

        public class MovementEffectHandle
        {
            MovementEffectEntry _entry;

            public MovementEffectHandle(MovementEffectEntry entry)
            {
                _entry = entry;
            }
        }

        // Maximum movement speed of this character.
        [SerializeField]
        float _maxSpeed;

        // The `PlayerCharacterInput` this component subscribes to.
        [SerializeField]
        PlayerCharacterInput _input;

        // Reference to the character's Rigidbody2D.
        [SerializeField]
        Rigidbody2D _rigidBody;

        // A snapshot of the rigidbody state, necesary for rollback-ing the character to a known past authoritative state.
        PredictionRigidbody2D _predictionRigidbody2D;
        // A list of all `MovementEffect`s in effect, neccesary for replayed inputs to "tick" the character deterministically.
        List<MovementEffectEntry> _movementEffects = new();

        // The most recent movement input from the client controlling this character.
        Vector2 _recentMoveInput;
        // The most recent desired angular velocity for this character.
        float _accumulatedMouseDeltaX;

        // Is the component subscribed to timemanager callbacks?
        bool _isSubscribedToTimeManager = false;
        bool _isSubscribedToInput = false;

        void Awake()
        {
            if (_rigidBody == null)
            {
                Debug.Log("`rigidBody` wasn't set.");
                throw new Exception();
            }
            _predictionRigidbody2D = new PredictionRigidbody2D();
            _predictionRigidbody2D.Initialize(_rigidBody);

            if (_input == null)
            {
                Debug.Log("`_input` wasn't set.");
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
            if (base.isActiveAndEnabled && base.IsOwner)
            {
                // We are the owning client of this character. Subscribe movement functions to the action.
                SubscribeToInput();
            }
        }

        public override void OnStopClient()
        {
            // We don't check for ownership here, since calling `UnsubscribeFromInput()` when we are not subscribed shouldn't cause any problems.
            UnsubscribeFromInput();
            // And call `ResetInputs()` to make sure past inputs don't stay in affect during disabled periods.
            ResetInputs();
        }

        void OnEnable()
        {
            if (base.IsClientInitialized && base.IsOwner)
            {
                // We are the owning client of this character. Allow inputs to control the character.
                // We need this functionality because we unsubscribe on disable.
                SubscribeToInput();
            }
        }

        void OnDisable()
        {
            // We don't check for ownership here, since calling `UnsubscribeFromInput()` when we are not subscribed shouldn't cause any problems.
            UnsubscribeFromInput();
            // And call `ResetInputs()` to make sure past inputs don't stay in affect during disabled periods.
            ResetInputs();

            // Unsubscribing from time manager will disrupt client side prediction, resulting in desynced positions.
            // UnsubscribeFromTimeManager();
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

        void SubscribeToInput()
        {
            if (!_isSubscribedToInput)
            {
                _input.Move += OnMove;
                _input.Look += OnLook;
                _isSubscribedToInput = true;
            }
        }

        void UnsubscribeFromInput()
        {
            if (_isSubscribedToInput)
            {
                _input.Move -= OnMove;
                _input.Look -= OnLook;
                _isSubscribedToInput = false;
            }
        }

        private void OnTimeManagerTick()
        {
            Debug.Log($"-------- Local: {TimeManager.LocalTick} | Server: {TimeManager.Tick} --------");
            Replicate(CreateReplicate());
        }

        private void OnTimeManagerPostTick()
        {
            CreateReconcile();
        }

        [Replicate]
        private void Replicate(ReplicateData data, ReplicateState state = ReplicateState.Invalid, Channel channel = Channel.Unreliable)
        {
            Debug.Log($"Simulating replicate created on {data.Tick}, fishnet tick {data.GetTick()}");
            if (!state.ContainsCreated())
            {
                // `data` isn't created by the owner; it is a default object provided by FishNet.
                // Zero out the rigidbody velocity to stop extrapolation.
                _predictionRigidbody2D.Velocity(Vector2.zero);
                // But leave the rotation as it is, since it has nothing to do with inertia.
            }
            else
            {
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
            }

            // Now let's fetch the local tick for this particular `Replicate()` call.
            uint currentTick = data.GetTick();
            if (base.IsServerInitialized)
            {
                // On servers, `data.GetTick()` is always owner-ticked.
                // Since servers never replay, we can assume the simulation is on the actual local tick.
                currentTick = TimeManager.LocalTick;
            }
            else if (base.IsClientInitialized && !base.Owner.IsLocalClient)
            {
                // On non-owned clients, `data.GetTick()` are server-ticked.
                currentTick = TimeManager.TickToLocalTick(currentTick);
            }

            foreach (var entry in _movementEffects)
            {
                if (!(entry.StartTick <= currentTick && currentTick < entry.EndTick))
                    continue;

                Debug.Log($"{TimeManager.LocalTick}: Dash in replicate, start: {entry.StartTick}, end: {entry.EndTick}");
                _predictionRigidbody2D.Velocity(entry.Effect.VelocityOverride);
            }
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

            ReplicateData data = new ReplicateData(worldMove, worldRotation, TimeManager.LocalTick);
            _accumulatedMouseDeltaX = 0f;
            return data;
        }

        [Reconcile]
        private void Reconcile(ReconcileData data, Channel channel = Channel.Unreliable)
        {
            Debug.Log($"Reconciling to state created on {data.Tick}, fishnet tick {data.GetTick()}");
            _predictionRigidbody2D.Reconcile(data.PredictionRigidbody2D);
        }


        public override void CreateReconcile()
        {
            // Debug.Log($"{TimeManager.LocalTick}: CreateReconcile");
            ReconcileData data = new ReconcileData(_predictionRigidbody2D, TimeManager.LocalTick);
            Reconcile(data);
        }

        // Called to notify movement input change.
        // Sets `_recentMoveInput` to reflect the input.
        [Client(RequireOwnership = true)]
        public void OnMove(Vector2 moveInput)
        {
            _recentMoveInput = moveInput;
        }

        // Called to notify look input change.
        // Sets `_accumulatedMouseDeltaX` to reflect the input.
        [Client(RequireOwnership = true)]
        public void OnLook(Vector2 lookInput)
        {
            float mouseDeltaX = lookInput.x;
            _accumulatedMouseDeltaX += mouseDeltaX;
        }

        // Reset recent movement input to zero.
        void ResetInputs()
        {
            _recentMoveInput = Vector2.zero;
            _accumulatedMouseDeltaX = 0f;
        }

        // Authoritatively add a `MovementEffect` that will be active for `startLocalTick <= TimeManager.LocalTick < endLocalTick`.
        // [Server]
        public MovementEffectHandle AddMovementEffectAuthoritative(VelocityOverrideEffect effect, uint startLocalTick, uint endLocalTick)
        {
            if (TimeManager.LocalTick > startLocalTick)
            {
                Debug.Log("Attempted to add a movementeffect starting in the past.");
                throw new Exception();
            }
            if (startLocalTick >= endLocalTick)
            {
                Debug.Log("Attempted to add a movementeffect with negative duration.");
                throw new Exception();
            }

            MovementEffectEntry entry = new(effect, TimeManager.LocalTick, startLocalTick, endLocalTick);
            _movementEffects.Add(entry);

            MovementEffectHandle handle = new(entry);
            return handle;
        }

        // Predictively add a `MovementEffect` that will be active for `startLocalTick <= TimeManager.LocalTick < endLocalTick`.
        // Doesn't sync the effect with the server; requires the caller to replicate the effect on the server side.
        // Note that predictive effects will be purged after 1RTT + alpha if the server doesn't validate them.
        // [Client(RequireOwnership = true)]
        public MovementEffectHandle AddMovementEffectPredictive(VelocityOverrideEffect effect, uint startLocalTick, uint endLocalTick)
        {
            if (TimeManager.LocalTick > startLocalTick)
            {
                Debug.Log("Attempted to add a movementeffect starting in the past.");
                throw new Exception();
            }
            if (startLocalTick >= endLocalTick)
            {
                Debug.Log("Attempted to add a movementeffect with negative duration.");
                throw new Exception();
            }

            MovementEffectEntry entry = new(effect, TimeManager.LocalTick, startLocalTick, endLocalTick);
            _movementEffects.Add(entry);

            MovementEffectHandle handle = new(entry);
            return handle;
        }


    }
}

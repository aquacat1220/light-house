namespace LightHouse
{
    using System;
    using FishNet.Managing.Timing;
    using FishNet.Object;
    using UnityEngine;

    public class DashController : NetworkBehaviour
    {
        [SerializeField]
        float _dashLength = 3f;
        [SerializeField]
        float _dashSpeed = 10f;

        uint _dashDurationInTicks = TimeManager.UNSET_TICK;

        // The `PlayerCharacterInput` this component subscribes to.
        [SerializeField]
        PlayerCharacterInput _input;
        [SerializeField]
        PredictedMovement _predictedMovement;

        bool _isSubscribedToInput = false;

        Vector2 _recentMoveInput;

        void Awake()
        {
            if (_dashSpeed == 0f)
            {
                Debug.Log("Dash speed was set to 0.");
                throw new Exception();
            }

            if (_input == null)
            {
                Debug.Log("`_input` wasn't set.");
                throw new Exception();
            }
            if (_predictedMovement == null)
            {
                Debug.Log("`_predictedMovement` wasn't set.");
                throw new Exception();
            }
        }

        public override void OnStartClient()
        {
            float dashDuration = _dashLength / _dashSpeed;
            _dashDurationInTicks = TimeManager.TimeToTicks(dashDuration, TickRounding.RoundUp);

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
        }

        void SubscribeToInput()
        {
            if (!_isSubscribedToInput)
            {
                _input.Move += OnMove;
                _input.Dash += OnDash;
                _isSubscribedToInput = true;
            }
        }

        void UnsubscribeFromInput()
        {
            if (_isSubscribedToInput)
            {
                _input.Move -= OnMove;
                _input.Dash -= OnDash;
                _isSubscribedToInput = false;
            }
        }

        [Client(RequireOwnership = true)]
        void OnMove(Vector2 moveInput)
        {
            _recentMoveInput = moveInput;
        }

        [Client(RequireOwnership = true)]
        void OnDash(bool isPerformed)
        {
            if (!isPerformed)
                return;

            Vector2 dashDirection = _recentMoveInput.normalized;
            if (dashDirection == Vector2.zero)
                dashDirection = Vector2.up;
            Vector2 dashWorldDirection = transform.TransformDirection(dashDirection);
            VelocityOverrideEffect dashEffect = new(_dashSpeed * dashWorldDirection);

            if (base.IsServerInitialized)
            {
                // If we are the server, no need to do this predictively.
                _predictedMovement.AddMovementEffectAuthoritative(dashEffect, TimeManager.LocalTick, TimeManager.LocalTick + _dashDurationInTicks);
                Debug.Log("Performed authoritative dash.");
            }
            else
            {
                _predictedMovement.AddMovementEffectPredictive(dashEffect, TimeManager.LocalTick, TimeManager.LocalTick + _dashDurationInTicks);
                Debug.Log("Performed predictive dash.");
                DashRpc(dashWorldDirection);
            }
        }

        [ServerRpc(RequireOwnership = true)]
        void DashRpc(Vector2 dashWorldDirection)
        {
            if (dashWorldDirection.sqrMagnitude > 1f + 0.0001f)
            {
                // We add a small margin of error, since tiny errors can happen in floating point operations.
                Debug.Log($"`data.WorldMove.magnitude > 1f` with a value of {dashWorldDirection.magnitude}, this might be an attempt for speed hacking.");
                dashWorldDirection.Normalize();
            }

            VelocityOverrideEffect dashEffect = new(_dashSpeed * dashWorldDirection);
            _predictedMovement.AddMovementEffectAuthoritative(dashEffect, TimeManager.LocalTick, TimeManager.LocalTick + _dashDurationInTicks);
            Debug.Log("Performed authoritative dash.");
        }
    }
}
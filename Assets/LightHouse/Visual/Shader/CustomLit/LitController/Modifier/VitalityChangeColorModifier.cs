namespace LightHouse
{
    using System;
    using FishNet.Object;
    using UnityEngine;
    using UnityEngine.Assertions;

    public class VitalityChangeColorModifier : NetworkBehaviour
    {
        [SerializeField]
        LitController _controller;
        [SerializeField]
        Vitality _vitality;
        [SerializeField]
        TimerBase _timer;

        [SerializeField]
        Color _colorOnHit = Color.white;
        [SerializeField]
        float _duration = 0.25f;

        [SerializeField]
        int _order = 2;

        float _oldVit;
        Alarm _alarm;

        void Awake()
        {
            Assert.IsNotNull(_controller);
            _controller.AddModifier(Modifier, _order);

            Assert.IsNotNull(_vitality);
            _oldVit = _vitality.Vit;
            _vitality.VitChange += OnVitChange;

            if (_timer == null)
                _timer = TimerManager.Singleton;
        }

        void OnDestroy()
        {
            _vitality.VitChange -= OnVitChange;
            _controller.RemoveModifier(Modifier);
        }

        void OnVitChange(float newVit)
        {
            if (_oldVit == newVit)
                return;
            _oldVit = newVit;
            if (_alarm != null)
            {
                // We already have a running alarm. Reset it.
                _alarm.Reset(_duration);
            }
            else
            {
                _alarm = _timer.AddAlarm(
                    cooldown: _duration,
                    callback: OnAlarm,
                    startImmediately: true,
                    armImmediately: true,
                    autoRestart: false,
                    autoRearm: false,
                    initialCooldown: _duration,
                    destroyAfterTriggered: true
                );
                _controller.ReevaluateModifiers();
            }
        }

        void OnAlarm(float _)
        {
            // We use `destroyAfterTriggered: true`.
            // _alarm.Remove();
            _alarm = null;
            _controller.ReevaluateModifiers();
        }

        Color Modifier(Color color)
        {
            // `_alarm` is non-null for `_duration` seconds after vitality change.
            // We want to show `_colorOnHit` for that period.
            // Of course the modifier needs to be reevaluated.
            if (_alarm != null)
                return _colorOnHit;
            return color;
        }
    }
}
namespace LightHouse
{
    using System;
    using UnityEngine;

    [Serializable]
    public class LimitPulse
    {
        Action<bool> _inner;
        float _delay = 1f;
        // `true` if we want to buffer a pulse-up that arrived during the delay.
        bool _bufferPulse = false;
        TimerBase _timer;

        // If `_alarm != null`, this means we are in cooldown.
        Alarm _alarm;
        bool _innerUp = false;
        bool _bufferedPulse = false;

        public LimitPulse(
            Action<bool> inner,
            float delay = 1f,
            bool bufferPulse = false,
            TimerBase timer = null
        )
        {
            _inner = inner;
            _delay = delay;
            _bufferPulse = bufferPulse;
            _timer = timer;
            if (_timer == null)
                _timer = TimerManager.Singleton;
        }

        void OnAlarm(float _)
        {
            if (_bufferedPulse)
            {
                InnerUp();
                _alarm.Arm();
                _alarm.Start();
                _bufferedPulse = false;
                return;
            }
            _alarm.Remove();
            _alarm = null;
        }

        public void Invoke(bool isUp)
        {
            if (isUp)
            {
                if (_alarm != null)
                {
                    _bufferedPulse = _bufferPulse;
                    return;
                }

                InnerUp();
                _alarm = _timer.AddAlarm(
                    cooldown: _delay,
                    callback: OnAlarm,
                    startImmediately: true,
                    armImmediately: true,
                    autoRestart: false,
                    autoRearm: false,
                    initialCooldown: _delay,
                    destroyAfterTriggered: false
                );
            }
            else
            {
                _bufferedPulse = false;
                InnerDown();
            }
        }

        void InnerUp()
        {
            if (!_innerUp)
            {
                _inner?.Invoke(true);
                _innerUp = true;
            }
        }

        void InnerDown()
        {
            if (_innerUp)
            {
                _inner?.Invoke(false);
                _innerUp = false;
            }
        }
    }
}
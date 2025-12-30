namespace LightHouse
{
    using System;

    [Serializable]
    public class RepeatPulse
    {
        Action<bool> _inner;
        float _repeatDelay;
        TimerBase _timer;

        Alarm _alarm;
        bool _isUp = false;

        public RepeatPulse(
            Action<bool> inner,
            float repeatDelay = 1f,
            TimerBase timer = null
        )
        {
            _inner = inner;
            _repeatDelay = repeatDelay;
            _timer = timer;
            if (_timer == null)
                _timer = TimerManager.Singleton;
        }

        void OnAlarm(float _)
        {
            if (_isUp)
            {
                _inner?.Invoke(true);
                _inner?.Invoke(false);
            }
            else
            {
                _alarm.Remove();
                _alarm = null;
            }
        }

        public void Invoke(bool isUp)
        {
            _isUp = isUp;
            if (_isUp && _alarm == null)
            {
                _inner?.Invoke(true);
                _inner?.Invoke(false);
                _alarm = _timer.AddAlarm(
                    cooldown: _repeatDelay,
                    callback: OnAlarm,
                    startImmediately: true,
                    armImmediately: true,
                    autoRestart: true,
                    autoRearm: true,
                    initialCooldown: _repeatDelay,
                    destroyAfterTriggered: false
                );
            }
        }
    }
}
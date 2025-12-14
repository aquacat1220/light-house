namespace LightHouse
{
    using System;
    using UnityEngine;
    using Fn;

    [Serializable]
    public class RepeatPulse : IFn<ITuple<bool>, Fn.Tuple>
    {
        [SerializeReference]
        [PolySelector]
        IFn<Fn.Tuple<bool>, Fn.Tuple> _inner;

        [SerializeField]
        float _repeatDelay = 0.25f;

        Alarm _alarm;
        bool _isUp = false;

        public RepeatPulse() { }
        public RepeatPulse(
            IFn<Fn.Tuple<bool>, Fn.Tuple> inner
        )
        {
            _inner = inner;
        }

        void OnAlarm(float _)
        {
            if (_isUp)
            {
                _inner?.Invoke(new Fn.Tuple<bool>(true));
                _inner?.Invoke(new Fn.Tuple<bool>(false));
            }
            else
            {
                _alarm.Remove();
                _alarm = null;
            }
        }

        public Fn.Tuple Invoke(ITuple<bool> param)
        {
            _isUp = param.Item1;
            if (_isUp && _alarm == null)
            {
                _alarm = TimerManager.Singleton.AddAlarm(
                    cooldown: _repeatDelay,
                    callback: OnAlarm,
                    startImmediately: true,
                    armImmediately: true,
                    autoRestart: true,
                    autoRearm: true,
                    initialCooldown: 0f,
                    destroyAfterTriggered: false
                );
            }
            return Fn.Tuple.Unit;
        }
    }
}
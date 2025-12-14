namespace LightHouse
{
    using System;
    using UnityEngine;
    using Fn;

    [Serializable]
    public class LimitPulse : IFn<ITuple<bool>, Fn.Tuple>
    {
        [SerializeReference]
        [PolySelector]
        IFn<Fn.Tuple<bool>, Fn.Tuple> _inner;

        [SerializeField]
        float _delay = 1f;
        // `true` if we want to buffer a pulse-up that arrived during the delay.
        [SerializeField]
        bool _bufferInput = false;

        Alarm _alarm;
        // `true` if the next alarm trigger should propagate a pulse-up.
        bool _bufferedUp = false;

        public LimitPulse() { }
        public LimitPulse(
            IFn<Fn.Tuple<bool>, Fn.Tuple> inner
        )
        {
            _inner = inner;
        }

        void OnAlarm(float _)
        {
            if (_bufferedUp)
            {
                _inner?.Invoke(new Fn.Tuple<bool>(true));
                _bufferedUp = false;
            }
            else
            {
                _alarm.Remove();
                _alarm = null;
            }
        }

        public Fn.Tuple Invoke(ITuple<bool> param)
        {
            bool isUp = param.Item1;
            if (isUp)
            {
                if (_bufferInput)
                    _bufferedUp = true;
                if (_alarm == null)
                {
                    _bufferedUp = true;
                    _alarm = TimerManager.Singleton.AddAlarm(
                        cooldown: _delay,
                        callback: OnAlarm,
                        startImmediately: true,
                        armImmediately: true,
                        autoRestart: true,
                        autoRearm: true,
                        initialCooldown: 0f,
                        destroyAfterTriggered: false
                    );
                }
            }
            else
            {
                _bufferedUp = false;
                _inner?.Invoke(new Fn.Tuple<bool>(false));
            }
            return Fn.Tuple.Unit;
        }
    }
}
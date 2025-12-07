namespace LightHouse
{
    using System;
    using FishNet.Object;
    using UnityEngine;
    using Fn;
    public class Sanity : NetworkBehaviour
    {
        [SerializeField]
        Vector2 _minMaxSan = new Vector2(0f, 100f);

        [SerializeField]
        [Min(0f)]
        float _initialSan = 100.0f;

        [SerializeField]
        Event<float> _sanChange;
        [SerializeField]
        Fn.Event _sanBelowZero;

        float _san = 0f;
        public float San
        {
            get { return _san; }
        }

        void Awake()
        {
            _san = Math.Clamp(_initialSan, _minMaxSan.x, _minMaxSan.y);
            _sanChange?.Invoke(_san);
            if (_san <= 0f)
                _sanBelowZero?.Invoke();
        }

        [Server]
        public void ApplyDamage(float damage, bool canHeal = false)
        {
            if (!canHeal)
                damage = Math.Max(damage, 0f);
            var oldSan = _san;
            _san = Math.Clamp(_san - damage, _minMaxSan.x, _minMaxSan.y);
            if (oldSan != _san)
            {
                _sanChange?.Invoke(_san);
                if (_san <= 0f)
                    _sanBelowZero?.Invoke();
                SanChangeRpc(_san);
            }
        }

        [ObserversRpc(ExcludeServer = true, BufferLast = true)]
        void SanChangeRpc(float newSan)
        {
            _san = newSan;
            _sanChange?.Invoke(_san);
            if (_san <= 0f)
                _sanBelowZero?.Invoke();
        }
    }
}

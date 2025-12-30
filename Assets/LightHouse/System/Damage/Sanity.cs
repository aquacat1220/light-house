namespace LightHouse
{
    using System;
    using FishNet.Object;
    using UnityEngine;

    public class Sanity : NetworkBehaviour
    {
        [SerializeField]
        float _maxSan = 100f;
        public float MaxSan
        {
            get => _maxSan;
        }

        [SerializeField]
        [Min(0f)]
        float _initialSan = 100.0f;

        float _san = 0f;
        public float San
        {
            get => _san;
        }
        public event Action<float> SanChange;
        public event Action SanBelowZero;

        void Awake()
        {
            _san = Mathf.Min(_initialSan, _maxSan);
            SanChange?.Invoke(_san);
            if (_san <= 0f)
                SanBelowZero?.Invoke();
        }

        [Server]
        public void ApplyDamage(float damage, bool canHeal = false)
        {
            if (!canHeal)
                damage = Math.Max(damage, 0f);
            var oldSan = _san;
            _san = Mathf.Min(_san - damage, _maxSan);
            if (oldSan != _san)
            {
                SanChange?.Invoke(_san);
                if (_san <= 0f)
                    SanBelowZero?.Invoke();
                SanChangeRpc(_san);
            }
        }

        [ObserversRpc(ExcludeServer = true, BufferLast = true)]
        void SanChangeRpc(float newSan)
        {
            _san = newSan;
            SanChange?.Invoke(_san);
            if (_san <= 0f)
                SanBelowZero?.Invoke();
        }
    }
}

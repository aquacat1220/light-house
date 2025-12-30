namespace LightHouse
{
    using System;
    using FishNet.Object;
    using UnityEngine;

    public class Vitality : NetworkBehaviour
    {
        [SerializeField]
        float _maxVit = 100f;
        public float MaxVit
        {
            get => _maxVit;
        }

        [SerializeField]
        [Min(0f)]
        float _initialVit = 100.0f;

        float _vit = 0f;
        public float Vit
        {
            get => _vit;
        }
        public event Action<float> VitChange;
        public event Action VitBelowZero;

        void Awake()
        {
            _vit = Mathf.Min(_initialVit, _maxVit);
            VitChange?.Invoke(_vit);
            if (_vit <= 0f)
                VitBelowZero?.Invoke();
        }

        [Server]
        public void ApplyDamage(float damage, bool canHeal = false)
        {
            if (!canHeal)
                damage = Math.Max(damage, 0f);
            var oldVit = _vit;
            _vit = Mathf.Min(_vit - damage, _maxVit);
            if (oldVit != _vit)
            {
                VitChange?.Invoke(_vit);
                if (_vit <= 0f)
                    VitBelowZero?.Invoke();
                VitChangeRpc(_vit);
            }
        }

        [ObserversRpc(ExcludeServer = true, BufferLast = true)]
        void VitChangeRpc(float newVit)
        {
            _vit = newVit;
            VitChange?.Invoke(_vit);
            if (_vit <= 0f)
                VitBelowZero?.Invoke();
        }
    }
}

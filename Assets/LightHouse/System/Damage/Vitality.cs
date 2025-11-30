namespace LightHouse
{
    using System;
    using FishNet.Object;
    using NaughtyAttributes;
    using UnityEngine;
    using Fn;

    public class Vitality : NetworkBehaviour
    {
        [SerializeField]
        [MinMaxSlider(-100f, 500f)]
        Vector2 _minMaxVit = new Vector2(0f, 100f);

        [SerializeField]
        [Min(0f)]
        float _initialVit = 100.0f;

        [SerializeField]
        Event<float> _vitChange;
        [SerializeField]
        Fn.Event _vitBelowZero;

        float _vit = 0f;
        public float Vit
        {
            get { return _vit; }
        }

        void Awake()
        {
            _vit = Math.Clamp(_initialVit, _minMaxVit.x, _minMaxVit.y);
            _vitChange?.Invoke(_vit);
            if (_vit <= 0f)
                _vitBelowZero?.Invoke();
        }

        [Server]
        public void ApplyDamage(float damage, bool canHeal = false)
        {
            if (!canHeal)
                damage = Math.Max(damage, 0f);
            var oldVit = _vit;
            _vit = Math.Clamp(_vit - damage, _minMaxVit.x, _minMaxVit.y);
            if (oldVit != _vit)
            {
                _vitChange?.Invoke(_vit);
                if (_vit <= 0f)
                    _vitBelowZero?.Invoke();
                VitChangeRpc(_vit);
            }
        }

        [ObserversRpc(ExcludeServer = true, BufferLast = true)]
        void VitChangeRpc(float newVit)
        {
            _vit = newVit;
            _vitChange?.Invoke(_vit);
            if (_vit <= 0f)
                _vitBelowZero?.Invoke();
        }
    }
}

namespace LightHouse
{
    using System;
    using FishNet.Object;
    using UnityEngine;
    using UnityEngine.Assertions;

    public class VitalityColorModifier : NetworkBehaviour
    {
        [SerializeField]
        LitController _controller;
        [SerializeField]
        Vitality _vitality;

        [SerializeField]
        Color _colorAtZero = Color.black;

        [SerializeField]
        int _order = 1;

        float _maxVit;
        float _vit;

        void Awake()
        {
            if (_controller == null)
            {
                Debug.Log("`_controller` was not set.");
                throw new Exception();
            }

            Assert.IsNotNull(_vitality);
            _maxVit = _vitality.MaxVit;
            _vitality.VitChange += OnVitChange;
            OnVitChange(_vitality.Vit);

            _controller.AddModifier(Modifier, _order);
        }

        void OnDestroy()
        {
            _vitality.VitChange -= OnVitChange;
            _controller.RemoveModifier(Modifier);
        }

        void OnVitChange(float newVit)
        {
            _vit = newVit;
            _controller.ReevaluateModifiers();
        }

        Color Modifier(Color color)
        {
            return Color.Lerp(_colorAtZero, color, _vit / _maxVit);
        }
    }
}
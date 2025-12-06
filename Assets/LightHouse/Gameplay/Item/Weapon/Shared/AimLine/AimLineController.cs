namespace LightHouse
{
    using System;
    using LightHouse.Fn;
    using NaughtyAttributes;
    using UnityEngine;
    using UnityEngine.UIElements;

    public class AimLineController : MonoBehaviour
    {
        [SerializeField]
        [Required]
        UIDocument _aimLineDocument;

        [SerializeField]
        [Required]
        RandomSpread _randomSpread;

        // Pixels per unit. Should be set in Assets/LightHouse/System/UI/Setting/WorldPanelSettings.asset, but unity doesn't expose the corresponding property.
        [SerializeField]
        float _pixelsPerUnit = 100f;

        // The intended aimline length in world units.
        [SerializeField]
        float _aimLineLength = 1f;
        public float AimLineLength
        {
            get => _aimLineLength;
            set => _aimLineLength = value;
        }

        // The intended aimline width in world units.
        [SerializeField]
        float _aimLineWidth = 0.02f;
        public float AimLineWidth
        {
            get => _aimLineWidth;
            set => _aimLineWidth = value;
        }

        // The intended stub length in world units.
        [SerializeField]
        float _stubLength = 0.02f;
        public float StubLength
        {
            get => _stubLength;
            set => _stubLength = value;
        }

        AimLine _aimLine;

        void Awake()
        {
            if (_aimLineDocument == null)
            {
                Debug.Log("`_aimLineDocument` wasn't set.");
                throw new Exception();
            }
            if (_randomSpread == null)
            {
                Debug.Log("`_randomSpread` wasn't set.");
                throw new Exception();
            }
            _aimLineDocument.enabled = false;
            Refresh();
        }

        [Button]
        void Refresh()
        {
            if (_aimLineDocument == null)
            {
                Debug.Log("`_aimLineDocument` wasn't set.");
                throw new Exception();
            }
            // UQuery doesn't seem to work when the document isn't enabled.
            if (!_aimLineDocument.isActiveAndEnabled)
                return;
            _aimLine = _aimLineDocument.rootVisualElement.Q<AimLine>(className: "aim-line");
            if (_aimLine != null)
            {
                _aimLine.InnerAngle = _randomSpread.AimVariance;
                _aimLine.OuterAngle = _randomSpread.WeaponVariance;
                _aimLine.LineLength = AimLineLength * _pixelsPerUnit;
                _aimLine.LineWidth = AimLineWidth * _pixelsPerUnit;
                _aimLine.StubLength = StubLength * _pixelsPerUnit;
            }
        }

        void Update()
        {
            if (_aimLine != null)
            {
                _aimLine.InnerAngle = _randomSpread.AimVariance;
                _aimLine.OuterAngle = _randomSpread.WeaponVariance;
                _aimLine.LineLength = AimLineLength * _pixelsPerUnit;
                _aimLine.LineWidth = AimLineWidth * _pixelsPerUnit;
                _aimLine.StubLength = StubLength * _pixelsPerUnit;
            }
        }

        [Serializable]
        public class OnRegisterFn : IFn<ITuple<ItemSlot>, Fn.Tuple>
        {
            public AimLineController AimLineController;
            public Fn.Tuple Invoke(ITuple<ItemSlot> param)
            {
                AimLineController?.OnRegister(param.Item1);
                return Fn.Tuple.Unit;
            }
        }

        [Serializable]
        public class OnUnregisterFn : IFn<Fn.Tuple, Fn.Tuple>
        {
            public AimLineController AimLineController;
            public Fn.Tuple Invoke(Fn.Tuple _)
            {
                AimLineController?.OnUnregister();
                return Fn.Tuple.Unit;
            }
        }

        public void OnRegister(ItemSlot itemSlot)
        {
            _aimLineDocument.enabled = itemSlot.Owner.IsLocalClient;
            Refresh();
        }

        public void OnUnregister()
        {
            _aimLineDocument.enabled = false;
        }
    }
}
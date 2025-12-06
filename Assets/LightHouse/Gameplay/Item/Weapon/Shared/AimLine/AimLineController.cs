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

        // Maximum aimline length in world units.
        [SerializeField]
        float _maxAimLineLength = 10f;
        // Ratio of aimline length to the orthographic camera size.
        [SerializeField]
        float _cameraSizeToAimLineLength = 0.6f;

        float _aimLineLength = 0f;
        float _aimLineWidth = 0f;
        float _stubLength = 0f;

        static float _lengtoToWidth = 0.005f;
        static float _lengthToStub = 0.015f;

        PlayerCharacterCamera _playerCharacterCamera;
        OnSizeChangeFn _sizeChangeListener = null;

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

        [Serializable]
        public class OnSizeChangeFn : IFn<ITuple<float>, Fn.Tuple>
        {
            public AimLineController AimLineController;
            public Fn.Tuple Invoke(ITuple<float> param)
            {
                AimLineController?.OnSizeChange(param.Item1);
                return Fn.Tuple.Unit;
            }
        }

        public void OnSizeChange(float newSize)
        {
            _aimLineLength = Mathf.Min(_maxAimLineLength, newSize * _cameraSizeToAimLineLength);
            _aimLineWidth = _aimLineLength * _lengtoToWidth;
            _stubLength = _aimLineLength * _lengthToStub;
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
                _aimLine.InnerAngle = _randomSpread.AimSpread;
                _aimLine.OuterAngle = _randomSpread.WeaponSpread;
                _aimLine.LineLength = _aimLineLength * _pixelsPerUnit;
                _aimLine.LineWidth = _aimLineWidth * _pixelsPerUnit;
                _aimLine.StubLength = _stubLength * _pixelsPerUnit;
            }
        }

        void Update()
        {
            // Debug.Log($"{_aimLineLength}, {_aimLineWidth}, {_stubLength}");
            if (_aimLine != null)
            {
                _aimLine.InnerAngle = _randomSpread.AimSpread;
                _aimLine.OuterAngle = _randomSpread.WeaponSpread;
                _aimLine.LineLength = _aimLineLength * _pixelsPerUnit;
                _aimLine.LineWidth = _aimLineWidth * _pixelsPerUnit;
                _aimLine.StubLength = _stubLength * _pixelsPerUnit;
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
            if (!itemSlot.Owner.IsLocalClient)
                return;
            _aimLineDocument.enabled = true;
            _playerCharacterCamera = itemSlot.User.GetComponent<PlayerCharacterCamera>();
            if (_playerCharacterCamera != null)
            {
                _sizeChangeListener = new OnSizeChangeFn();
                _sizeChangeListener.AimLineController = this;
                _playerCharacterCamera.AddSizeChangedListener(_sizeChangeListener);
                OnSizeChange(_playerCharacterCamera.Size);
            }
            Refresh();
        }

        public void OnUnregister()
        {
            _aimLineDocument.enabled = false;
            if (_playerCharacterCamera != null)
            {
                _playerCharacterCamera.RemoveSizeChangedListener(_sizeChangeListener);
                _sizeChangeListener = null;
                _playerCharacterCamera = null;
            }
        }
    }
}
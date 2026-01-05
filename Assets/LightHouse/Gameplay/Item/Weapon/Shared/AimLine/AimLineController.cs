namespace LightHouse
{
    using System;
    using LightHouse.Fn;
    using UnityEngine;
    using UnityEngine.UIElements;

    public class AimLineController : MonoBehaviour
    {
        [SerializeField]
        Item _item;

        [SerializeField]
        UIDocument _aimLineDocument;

        [SerializeField]
        RandomSpread _randomSpread;

        // Pixels per unit. Should be set in Assets/LightHouse/System/UI/Setting/WorldPanelSettings.asset, but unity doesn't expose the corresponding property.
        [SerializeField]
        float _pixelsPerUnit = 100f;

        // Maximum aimline length in world units.
        [SerializeField]
        float _maxAimLineLength = 10f;
        // Ratio of aimline length to the orthographic camera size.
        [SerializeField]
        float _cameraSizeToAimLineLength = 0.8f;

        float _aimLineLength = 0f;
        float _aimLineWidth = 0f;
        float _stubLength = 0f;

        static float _lengtoToWidth = 0.005f;
        static float _lengthToStub = 0.015f;

        PlayerCharacterCamera _playerCharacterCamera;

        AimLine _aimLine;

        void Awake()
        {
            if (_item == null)
            {
                Debug.Log("`_item` was not set.");
                throw new Exception();
            }
            _item.Register += OnRegister;
            _item.Unregister += OnUnregister;
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

        void OnDestroy()
        {
            _item.Register -= OnRegister;
            _item.Unregister -= OnUnregister;
        }

        void OnFrontSizeChange(float newFrontSize)
        {
            _aimLineLength = Mathf.Min(_maxAimLineLength, newFrontSize * _cameraSizeToAimLineLength);
            _aimLineWidth = _aimLineLength * _lengtoToWidth;
            _stubLength = _aimLineLength * _lengthToStub;
            Refresh();
        }

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

        void OnRegister(ItemSlot itemSlot)
        {
            if (!itemSlot.Owner.IsLocalClient)
                return;
            _aimLineDocument.enabled = true;
            _playerCharacterCamera = itemSlot.User.GetComponent<PlayerCharacterCamera>();
            if (_playerCharacterCamera != null)
            {
                _playerCharacterCamera.FrontSizeChanged += OnFrontSizeChange;
                OnFrontSizeChange(_playerCharacterCamera.FrontSize);
            }
            Refresh();
        }

        void OnUnregister()
        {
            _aimLineDocument.enabled = false;
            if (_playerCharacterCamera != null)
            {
                _playerCharacterCamera.FrontSizeChanged -= OnFrontSizeChange;
                _playerCharacterCamera = null;
            }
        }
    }
}
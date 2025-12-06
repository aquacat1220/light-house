namespace LightHouse
{
    using System;
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
        // The intended aimline width in world units.
        [SerializeField]
        float _aimLineWidth = 0.02f;

        AimLine _aimLine;

        void Awake()
        {
            if (_aimLineDocument == null)
            {
                Debug.Log("`_aimLineDocument` wasn't set.");
                throw new Exception();
            }
            _aimLine = _aimLineDocument.rootVisualElement.Q<AimLine>(className: "aim-line");
            if (_randomSpread == null)
            {
                Debug.Log("`_randomSpread` wasn't set.");
                throw new Exception();
            }
        }

        void Update()
        {
            if (_aimLine != null)
            {
                _aimLine.InnerAngle = _randomSpread.AimVariance;
                _aimLine.OuterAngle = _randomSpread.WeaponVariance;
                _aimLine.LineLength = _aimLineLength * _pixelsPerUnit;
                _aimLine.LineWidth = _aimLineWidth * _pixelsPerUnit;
            }
        }
    }
}
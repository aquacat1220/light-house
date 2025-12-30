namespace LightHouse
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public class LitController : MonoBehaviour
    {
        static string _targetShaderName = "Shader Graphs/Lit";
        static int _isVectorSpriteId = Shader.PropertyToID("_IsVectorSprite");
        static int _tintColorId = Shader.PropertyToID("_TintColor");
        static int _alwaysLitId = Shader.PropertyToID("_AlwaysLit");
        static int _alwaysVisibleId = Shader.PropertyToID("_AlwaysVisible");

        [SerializeField]
        Renderer _target;

        [SerializeField]
        bool _isVectorSprite = false;
        [SerializeField]
        Color _tintColor = Color.white;
        [SerializeField]
        bool _alwaysLit = false;
        [SerializeField]
        bool _alwaysVisible = false;

        List<(Func<Color, Color> Modifier, int Order)> _modifiers = new();
        bool _unsorted = true;

        Material _material;

        void Awake()
        {
            _material = _target.material;
            if (_material.shader.name != _targetShaderName)
            {
                Debug.Log($"`_target` should be using a material backed by \"{_targetShaderName}\".");
                throw new Exception();
            }

            _material.SetFloat(_isVectorSpriteId, _isVectorSprite ? 1f : 0f);
            AlwaysLit = _alwaysLit;
            ReevaluateModifiers();
            AlwaysVisible = _alwaysVisible;
        }

        public bool AlwaysLit
        {
            get => _material.GetFloat(_alwaysLitId) != 0f;
            set => _material.SetFloat(_alwaysLitId, value ? 1f : 0f);
        }

        public bool AlwaysVisible
        {
            get => _material.GetFloat(_alwaysVisibleId) != 0f;
            set => _material.SetFloat(_alwaysVisibleId, value ? 1f : 0f);
        }

        public void ReevaluateModifiers()
        {
            if (_unsorted)
            {
                _modifiers.Sort((x, y) =>
                {
                    return x.Order.CompareTo(y.Order);
                });
                _unsorted = false;
            }

            var color = _tintColor;
            foreach (var modifier in _modifiers)
            {
                color = modifier.Modifier.Invoke(color);
            }

            _material.SetColor(_tintColorId, color);
        }

        public void AddModifier(Func<Color, Color> modifier, int order)
        {
            _modifiers.Add((modifier, order));
            _unsorted = true;
            ReevaluateModifiers();
        }

        public void RemoveModifier(Func<Color, Color> modifier)
        {
            _modifiers.RemoveAll(x => x.Modifier == modifier);
            _unsorted = true;
            ReevaluateModifiers();
        }

        void OnDestroy()
        {
            if (_material != null)
                Destroy(_material);
        }
    }
}
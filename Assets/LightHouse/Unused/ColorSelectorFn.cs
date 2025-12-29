namespace LightHouse
{
    using System;
    using Fn;
    using UnityEngine;

    [Serializable]
    public class ColorSelectorFn : IFn<ITupleBase, Fn.Tuple>
    {
        [SerializeField]
        LitController _controller;
        [SerializeField]
        Color _color;
        [SerializeField]
        int _order = 0;

        FuncFn<Color, Color> _modifier;

        public Fn.Tuple Invoke(ITupleBase _)
        {
            if (_modifier != null)
            {
                _controller.RemoveModifier(_modifier);
                _modifier = null;
            }
            _modifier = new FuncFn<Color, Color>((_) => _color);
            _controller.AddModifier(_modifier, _order);
            return new Fn.Tuple();
        }
    }
}
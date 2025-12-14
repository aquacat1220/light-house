namespace LightHouse.Fn
{
    using System;
    using UnityEngine;

    [Serializable]
    public class Branch<TParam, TResult> : IFn<TParam, TResult> where TParam : ITupleBase
    {
        [SerializeReference]
        [PolySelector]
        IFn<TParam, bool> _condition;
        [SerializeReference]
        [PolySelector]
        IFn<TParam, TResult> _true;
        [SerializeReference]
        [PolySelector]
        IFn<TParam, TResult> _false;

        public Branch() { }
        public Branch(
            IFn<TParam, bool> condition,
            IFn<TParam, TResult> @true,
            IFn<TParam, TResult> @false
        )
        {
            _condition = condition;
            _true = @true;
            _false = @false;
        }

        public TResult Invoke(TParam param)
        {
            if (_condition.Invoke(param))
            {
                if (_true != null)
                    return _true.Invoke(param);
                return default;
            }
            else
            {
                if (_false != null)
                    return _false.Invoke(param);
                return default;
            }
        }
    }
}
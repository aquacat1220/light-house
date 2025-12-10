namespace LightHouse.Fn
{
    using System;
    using UnityEngine;

    [Serializable]
    public class Branch<TResult> : IFn<ITuple<bool>, TResult>
    {
        [SerializeReference]
        [PolySelector]
        IFn<Tuple, TResult> _true;
        [SerializeReference]
        [PolySelector]
        IFn<Tuple, TResult> _false;

        public Branch() { }
        public Branch(
            IFn<Tuple, TResult> @true,
            IFn<Tuple, TResult> @false
        )
        {
            _true = @true;
            _false = @false;
        }

        public TResult Invoke(ITuple<bool> param)
        {
            if (param.Item1)
            {
                if (_true != null)
                    return _true.Invoke(Tuple.Unit);
                return default;
            }
            else
            {
                if (_false != null)
                    return _false.Invoke(Tuple.Unit);
                return default;
            }
        }
    }

    [Serializable]
    public class Branch<TParam, TResult> : IFn<ITuple<bool, TParam>, TResult> where TParam : ITupleBase
    {
        [SerializeReference]
        [PolySelector]
        IFn<TParam, TResult> _true;
        [SerializeReference]
        [PolySelector]
        IFn<TParam, TResult> _false;

        public Branch() { }
        public Branch(
            IFn<TParam, TResult> @true,
            IFn<TParam, TResult> @false
        )
        {
            _true = @true;
            _false = @false;
        }

        public TResult Invoke(ITuple<bool, TParam> param)
        {
            if (param.Item1)
            {
                if (_true != null)
                    return _true.Invoke(param.Item2);
                return default;
            }
            else
            {
                if (_false != null)
                    return _false.Invoke(param.Item2);
                return default;
            }
        }
    }
}
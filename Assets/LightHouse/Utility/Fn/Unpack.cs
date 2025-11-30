namespace LightHouse.Fn
{
    using System;
    using UnityEngine;

    [Serializable]
    public class Unpack<TParam, TResult> : IFn<ITuple<TParam>, TResult> where TParam : ITupleBase
    {
        [SerializeReference]
        [SubclassSelector]
        IFn<TParam, TResult> _inner;

        public Unpack() { }
        public Unpack(
            IFn<TParam, TResult> inner
        )
        {
            _inner = inner;
        }

        public TResult Invoke(ITuple<TParam> param)
        {
            return _inner.Invoke(param.Item1);
        }
    }
}
namespace LightHouse.Fn
{
    using System;
    using UnityEngine;

    [Serializable]
    public class Unpack<TParam, TResult> : IFn<ITuple<TParam>, TResult> where TParam : ITupleBase
    {
        [SerializeReference]
        [SubclassSelector]
        public IFn<TParam, TResult> Inner;

        public Unpack() { }
        public Unpack(
            IFn<TParam, TResult> inner
        )
        {
            Inner = inner;
        }

        public TResult Invoke(ITuple<TParam> param)
        {
            return Inner.Invoke(param.Item1);
        }
    }
}
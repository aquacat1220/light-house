namespace LightHouse.Fn
{
    using System;
    using UnityEngine;

    [Serializable]
    public class Array<TParam> : IFn<TParam, Tuple> where TParam : ITupleBase
    {
        [SerializeReference]
        [PolySelector]
        IFn<TParam, Tuple>[] _inners = new IFn<TParam, Tuple>[0];

        public Array() { }
        public Array(
            IFn<TParam, Tuple>[] inners
        )
        {
            _inners = inners;
        }

        public Tuple Invoke(TParam param)
        {
            foreach (var inner in _inners)
            {
                inner.Invoke(param);
            }
            return Tuple.Unit;
        }
    }
}
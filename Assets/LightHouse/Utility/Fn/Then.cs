namespace LightHouse.Fn
{
    using System;
    using UnityEngine;

    [Serializable]
    public class Then<TParam, TInter, TResult> : IFn<TParam, TResult> where TParam : ITupleBase
    {
        [SerializeReference]
        [SubclassSelector]
        IFn<TParam, TInter> _first;
        [SerializeReference]
        [SubclassSelector]
        IFn<ITuple<TInter>, TResult> _second;

        public Then() { }
        public Then(
            IFn<TParam, TInter> first,
            IFn<ITuple<TInter>, TResult> second
        )
        {
            _first = first;
            _second = second;
        }

        public TResult Invoke(TParam param)
        {
            return _second.Invoke(new Tuple<TInter>(_first.Invoke(param)));
        }
    }
}
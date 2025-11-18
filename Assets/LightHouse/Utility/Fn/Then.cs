namespace LightHouse.Fn
{
    using System;
    using UnityEngine;

    [Serializable]
    public class Then<TParam, TInter, TResult> : IFn<TParam, TResult> where TParam : ITupleBase
    {
        [SerializeReference]
        [SubclassSelector]
        public IFn<TParam, TInter> First;
        [SerializeReference]
        [SubclassSelector]
        public IFn<ITuple<TInter>, TResult> Second;

        public Then() { }
        public Then(
            IFn<TParam, TInter> first,
            IFn<ITuple<TInter>, TResult> second
        )
        {
            First = first;
            Second = second;
        }

        public TResult Invoke(TParam param)
        {
            return Second.Invoke(new Tuple<TInter>(First.Invoke(param)));
        }
    }
}
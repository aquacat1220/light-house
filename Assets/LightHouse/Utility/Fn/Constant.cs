namespace LightHouse.Fn
{
    using System;
    using UnityEngine;

    [Serializable]
    public class Constant<TResult> : IFn<ITupleBase, TResult>
    {
        [SerializeReference]
        [SubclassSelector]
        public TResult Value;

        public Constant() { }
        public Constant(
            TResult value
        )
        {
            Value = value;
        }

        public TResult Invoke(ITupleBase param)
        {
            return Value;
        }
    }
}
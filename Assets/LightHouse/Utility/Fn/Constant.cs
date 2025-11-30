namespace LightHouse.Fn
{
    using System;
    using UnityEngine;

    [Serializable]
    public class ConstantRef<TResult> : IFn<ITupleBase, TResult> where TResult : class
    {
        [SerializeReference]
        [SubclassSelector]
        public TResult Value;

        public ConstantRef() { }
        public ConstantRef(
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

    [Serializable]
    public class ConstantVal<TResult> : IFn<ITupleBase, TResult> where TResult : struct
    {
        [SubclassSelector]
        public TResult Value;

        public ConstantVal() { }
        public ConstantVal(
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
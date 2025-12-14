namespace LightHouse.Fn
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    [Serializable]
    public class And<TParam> : IFn<TParam, bool> where TParam : ITupleBase
    {
        [SerializeReference, PolySelector]
        public List<IFn<TParam, bool>> Conditions;
        public bool Invoke(TParam param)
        {
            foreach (var condition in Conditions)
            {
                if (!condition.Invoke(param))
                    return false;
            }
            return true;
        }
    }
}
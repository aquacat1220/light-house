namespace LightHouse.Fn
{
    using System;
    using UnityEngine;

    [Serializable]
    public class EnableFn : IFn<ITuple<bool>, Tuple>, IFn<Tuple, Tuple>
    {
        public MonoBehaviour MonoBehaviour;
        public bool DefaultParam = false;

        public Tuple Invoke(ITuple<bool> param)
        {
            if (MonoBehaviour != null)
                MonoBehaviour.enabled = param.Item1;
            return Tuple.Unit;
        }

        public Tuple Invoke(Tuple _)
        {
            if (MonoBehaviour != null)
                MonoBehaviour.enabled = DefaultParam;
            return Tuple.Unit;
        }
    }
}
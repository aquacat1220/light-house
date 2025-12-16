namespace LightHouse
{
    using System;
    using LightHouse.Fn;
    using UnityEngine;

    [Serializable]
    public class Logger : IFn<ITuple<string>, Fn.Tuple>, IFn<Fn.ITupleBase, Fn.Tuple>
    {
        public string DefaultArgument = "Log string.";
        public Fn.Tuple Invoke(ITuple<string> param)
        {
            Debug.Log($"{Time.time}: {param.Item1}");
            return Fn.Tuple.Unit;
        }

        public Fn.Tuple Invoke(Fn.ITupleBase _)
        {
            Debug.Log($"{Time.time}: {DefaultArgument}");
            return Fn.Tuple.Unit;
        }
    }
}

using System;

namespace LightHouse.Fn
{
    [Serializable]
    public class Event<TParam> where TParam : ITupleBase
    {
        [UnityEngine.SerializeReference]
        [SubclassSelector]
        IFn<TParam, Tuple>[] _listeners = new IFn<TParam, Tuple>[0];
        public void Invoke(TParam param)
        {
            foreach (var listener in _listeners)
            {
                listener.Invoke(param);
            }
        }
    }
}
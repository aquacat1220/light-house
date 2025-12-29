namespace LightHouse.Fn
{
    using System;
    using System.Collections.Generic;

    [Serializable]
    public class Event
    {
        [UnityEngine.SerializeReference]
        [PolySelector]
        public List<IFn<ITuple, Tuple>> _listeners = new List<IFn<ITuple, Tuple>>();
        public void Invoke()
        {
            foreach (var listener in _listeners)
            {
                listener.Invoke(new Tuple());
            }
        }
    }

    [Serializable]
    public class Event<T1>
    {
        [UnityEngine.SerializeReference]
        [PolySelector]
        public List<IFn<ITuple<T1>, Tuple>> _listeners = new List<IFn<ITuple<T1>, Tuple>>();
        public void Invoke(T1 param1)
        {
            foreach (var listener in _listeners)
            {
                listener.Invoke(new Tuple<T1>(param1));
            }
        }
    }

    [Serializable]
    public class Event<T1, T2>
    {
        [UnityEngine.SerializeReference]
        [PolySelector]
        public List<IFn<ITuple<T1, T2>, Tuple>> _listeners = new List<IFn<ITuple<T1, T2>, Tuple>>();
        public void Invoke(T1 param1, T2 param2)
        {
            foreach (var listener in _listeners)
            {
                listener.Invoke(new Tuple<T1, T2>(param1, param2));
            }
        }
    }

    [Serializable]
    public class Event<T1, T2, T3>
    {
        [UnityEngine.SerializeReference]
        [PolySelector]
        public List<IFn<ITuple<T1, T2, T3>, Tuple>> _listeners = new List<IFn<ITuple<T1, T2, T3>, Tuple>>();
        public void Invoke(T1 param1, T2 param2, T3 param3)
        {
            foreach (var listener in _listeners)
            {
                listener.Invoke(new Tuple<T1, T2, T3>(param1, param2, param3));
            }
        }
    }

    [Serializable]
    public class Event<T1, T2, T3, T4>
    {
        [UnityEngine.SerializeReference]
        [PolySelector]
        public List<IFn<ITuple<T1, T2, T3, T4>, Tuple>> _listeners = new List<IFn<ITuple<T1, T2, T3, T4>, Tuple>>();
        public void Invoke(T1 param1, T2 param2, T3 param3, T4 param4)
        {
            foreach (var listener in _listeners)
            {
                listener.Invoke(new Tuple<T1, T2, T3, T4>(param1, param2, param3, param4));
            }
        }
    }

    [Serializable]
    public class Event<T1, T2, T3, T4, T5>
    {
        [UnityEngine.SerializeReference]
        [PolySelector]
        public List<IFn<ITuple<T1, T2, T3, T4, T5>, Tuple>> _listeners = new List<IFn<ITuple<T1, T2, T3, T4, T5>, Tuple>>();
        public void Invoke(T1 param1, T2 param2, T3 param3, T4 param4, T5 param5)
        {
            foreach (var listener in _listeners)
            {
                listener.Invoke(new Tuple<T1, T2, T3, T4, T5>(param1, param2, param3, param4, param5));
            }
        }
    }

    [Serializable]
    public class Event<T1, T2, T3, T4, T5, T6>
    {
        [UnityEngine.SerializeReference]
        [PolySelector]
        public List<IFn<ITuple<T1, T2, T3, T4, T5, T6>, Tuple>> _listeners = new List<IFn<ITuple<T1, T2, T3, T4, T5, T6>, Tuple>>();
        public void Invoke(T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6)
        {
            foreach (var listener in _listeners)
            {
                listener.Invoke(new Tuple<T1, T2, T3, T4, T5, T6>(param1, param2, param3, param4, param5, param6));
            }
        }
    }

    [Serializable]
    public class Event<T1, T2, T3, T4, T5, T6, T7>
    {
        [UnityEngine.SerializeReference]
        [PolySelector]
        public List<IFn<ITuple<T1, T2, T3, T4, T5, T6, T7>, Tuple>> _listeners = new List<IFn<ITuple<T1, T2, T3, T4, T5, T6, T7>, Tuple>>();
        public void Invoke(T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7)
        {
            foreach (var listener in _listeners)
            {
                listener.Invoke(new Tuple<T1, T2, T3, T4, T5, T6, T7>(param1, param2, param3, param4, param5, param6, param7));
            }
        }
    }

    [Serializable]
    public class Event<T1, T2, T3, T4, T5, T6, T7, T8>
    {
        [UnityEngine.SerializeReference]
        [PolySelector]
        public List<IFn<ITuple<T1, T2, T3, T4, T5, T6, T7, T8>, Tuple>> _listeners = new List<IFn<ITuple<T1, T2, T3, T4, T5, T6, T7, T8>, Tuple>>();
        public void Invoke(T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, T8 param8)
        {
            foreach (var listener in _listeners)
            {
                listener.Invoke(new Tuple<T1, T2, T3, T4, T5, T6, T7, T8>(param1, param2, param3, param4, param5, param6, param7, param8));
            }
        }
    }
}
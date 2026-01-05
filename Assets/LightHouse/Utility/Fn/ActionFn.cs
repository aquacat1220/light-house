namespace LightHouse.Fn
{
    using System;

    [Serializable]
    public class ActionFn<T1> : IFn<ITuple<T1>, Tuple>
    {
        Action<T1> _delegate;

        public Tuple Invoke(ITuple<T1> param)
        {
            _delegate(param.Item1);
            return new Tuple();
        }

        public ActionFn(Action<T1> @delegate)
        {
            _delegate = @delegate;
        }
    }

    [Serializable]
    public class ActionFn<T1, T2> : IFn<ITuple<T1, T2>, Tuple>
    {
        Action<T1, T2> _delegate;

        public Tuple Invoke(ITuple<T1, T2> param)
        {
            _delegate(param.Item1, param.Item2);
            return new Tuple();
        }

        public ActionFn(Action<T1, T2> @delegate)
        {
            _delegate = @delegate;
        }
    }

    [Serializable]
    public class ActionFn<T1, T2, T3> : IFn<ITuple<T1, T2, T3>, Tuple>
    {
        Action<T1, T2, T3> _delegate;

        public Tuple Invoke(ITuple<T1, T2, T3> param)
        {
            _delegate(param.Item1, param.Item2, param.Item3);
            return new Tuple();
        }

        public ActionFn(Action<T1, T2, T3> @delegate)
        {
            _delegate = @delegate;
        }
    }

    [Serializable]
    public class ActionFn<T1, T2, T3, T4> : IFn<ITuple<T1, T2, T3, T4>, Tuple>
    {
        Action<T1, T2, T3, T4> _delegate;

        public Tuple Invoke(ITuple<T1, T2, T3, T4> param)
        {
            _delegate(param.Item1, param.Item2, param.Item3, param.Item4);
            return new Tuple();
        }

        public ActionFn(Action<T1, T2, T3, T4> @delegate)
        {
            _delegate = @delegate;
        }
    }

    [Serializable]
    public class ActionFn<T1, T2, T3, T4, T5> : IFn<ITuple<T1, T2, T3, T4, T5>, Tuple>
    {
        Action<T1, T2, T3, T4, T5> _delegate;

        public Tuple Invoke(ITuple<T1, T2, T3, T4, T5> param)
        {
            _delegate(param.Item1, param.Item2, param.Item3, param.Item4, param.Item5);
            return new Tuple();
        }

        public ActionFn(Action<T1, T2, T3, T4, T5> @delegate)
        {
            _delegate = @delegate;
        }
    }

    [Serializable]
    public class ActionFn<T1, T2, T3, T4, T5, T6> : IFn<ITuple<T1, T2, T3, T4, T5, T6>, Tuple>
    {
        Action<T1, T2, T3, T4, T5, T6> _delegate;

        public Tuple Invoke(ITuple<T1, T2, T3, T4, T5, T6> param)
        {
            _delegate(param.Item1, param.Item2, param.Item3, param.Item4, param.Item5, param.Item6);
            return new Tuple();
        }

        public ActionFn(Action<T1, T2, T3, T4, T5, T6> @delegate)
        {
            _delegate = @delegate;
        }
    }

    [Serializable]
    public class ActionFn<T1, T2, T3, T4, T5, T6, T7> : IFn<ITuple<T1, T2, T3, T4, T5, T6, T7>, Tuple>
    {
        Action<T1, T2, T3, T4, T5, T6, T7> _delegate;

        public Tuple Invoke(ITuple<T1, T2, T3, T4, T5, T6, T7> param)
        {
            _delegate(param.Item1, param.Item2, param.Item3, param.Item4, param.Item5, param.Item6, param.Item7);
            return new Tuple();
        }

        public ActionFn(Action<T1, T2, T3, T4, T5, T6, T7> @delegate)
        {
            _delegate = @delegate;
        }
    }

    [Serializable]
    public class ActionFn<T1, T2, T3, T4, T5, T6, T7, T8> : IFn<ITuple<T1, T2, T3, T4, T5, T6, T7, T8>, Tuple>
    {
        Action<T1, T2, T3, T4, T5, T6, T7, T8> _delegate;

        public Tuple Invoke(ITuple<T1, T2, T3, T4, T5, T6, T7, T8> param)
        {
            _delegate(param.Item1, param.Item2, param.Item3, param.Item4, param.Item5, param.Item6, param.Item7, param.Item8);
            return new Tuple();
        }

        public ActionFn(Action<T1, T2, T3, T4, T5, T6, T7, T8> @delegate)
        {
            _delegate = @delegate;
        }
    }
}
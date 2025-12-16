namespace LightHouse.Fn
{
    using System;

    [Serializable]
    public class FuncFn<TResult> : IFn<Tuple, TResult>
    {
        Func<TResult> _delegate;

        public TResult Invoke(Tuple _)
        {
            return _delegate();
        }

        public FuncFn(Func<TResult> @delegate)
        {
            _delegate = @delegate;
        }
    }

    [Serializable]
    public class FuncFn<T1, TResult> : IFn<ITuple<T1>, TResult>
    {
        Func<T1, TResult> _delegate;

        public TResult Invoke(ITuple<T1> param)
        {
            return _delegate(param.Item1);
        }

        public FuncFn(Func<T1, TResult> @delegate)
        {
            _delegate = @delegate;
        }
    }

    [Serializable]
    public class FuncFn<T1, T2, TResult> : IFn<ITuple<T1, T2>, TResult>
    {
        Func<T1, T2, TResult> _delegate;

        public TResult Invoke(ITuple<T1, T2> param)
        {
            return _delegate(param.Item1, param.Item2);
        }

        public FuncFn(Func<T1, T2, TResult> @delegate)
        {
            _delegate = @delegate;
        }
    }

    [Serializable]
    public class FuncFn<T1, T2, T3, TResult> : IFn<ITuple<T1, T2, T3>, TResult>
    {
        Func<T1, T2, T3, TResult> _delegate;

        public TResult Invoke(ITuple<T1, T2, T3> param)
        {
            return _delegate(param.Item1, param.Item2, param.Item3);
        }

        public FuncFn(Func<T1, T2, T3, TResult> @delegate)
        {
            _delegate = @delegate;
        }
    }

    [Serializable]
    public class FuncFn<T1, T2, T3, T4, TResult> : IFn<ITuple<T1, T2, T3, T4>, TResult>
    {
        Func<T1, T2, T3, T4, TResult> _delegate;

        public TResult Invoke(ITuple<T1, T2, T3, T4> param)
        {
            return _delegate(param.Item1, param.Item2, param.Item3, param.Item4);
        }

        public FuncFn(Func<T1, T2, T3, T4, TResult> @delegate)
        {
            _delegate = @delegate;
        }
    }

    [Serializable]
    public class FuncFn<T1, T2, T3, T4, T5, TResult> : IFn<ITuple<T1, T2, T3, T4, T5>, TResult>
    {
        Func<T1, T2, T3, T4, T5, TResult> _delegate;

        public TResult Invoke(ITuple<T1, T2, T3, T4, T5> param)
        {
            return _delegate(param.Item1, param.Item2, param.Item3, param.Item4, param.Item5);
        }

        public FuncFn(Func<T1, T2, T3, T4, T5, TResult> @delegate)
        {
            _delegate = @delegate;
        }
    }

    [Serializable]
    public class FuncFn<T1, T2, T3, T4, T5, T6, TResult> : IFn<ITuple<T1, T2, T3, T4, T5, T6>, TResult>
    {
        Func<T1, T2, T3, T4, T5, T6, TResult> _delegate;

        public TResult Invoke(ITuple<T1, T2, T3, T4, T5, T6> param)
        {
            return _delegate(param.Item1, param.Item2, param.Item3, param.Item4, param.Item5, param.Item6);
        }

        public FuncFn(Func<T1, T2, T3, T4, T5, T6, TResult> @delegate)
        {
            _delegate = @delegate;
        }
    }

    [Serializable]
    public class FuncFn<T1, T2, T3, T4, T5, T6, T7, TResult> : IFn<ITuple<T1, T2, T3, T4, T5, T6, T7>, TResult>
    {
        Func<T1, T2, T3, T4, T5, T6, T7, TResult> _delegate;

        public TResult Invoke(ITuple<T1, T2, T3, T4, T5, T6, T7> param)
        {
            return _delegate(param.Item1, param.Item2, param.Item3, param.Item4, param.Item5, param.Item6, param.Item7);
        }

        public FuncFn(Func<T1, T2, T3, T4, T5, T6, T7, TResult> @delegate)
        {
            _delegate = @delegate;
        }
    }

    [Serializable]
    public class FuncFn<T1, T2, T3, T4, T5, T6, T7, T8, TResult> : IFn<ITuple<T1, T2, T3, T4, T5, T6, T7, T8>, TResult>
    {
        Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> _delegate;

        public TResult Invoke(ITuple<T1, T2, T3, T4, T5, T6, T7, T8> param)
        {
            return _delegate(param.Item1, param.Item2, param.Item3, param.Item4, param.Item5, param.Item6, param.Item7, param.Item8);
        }

        public FuncFn(Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> @delegate)
        {
            _delegate = @delegate;
        }
    }
}
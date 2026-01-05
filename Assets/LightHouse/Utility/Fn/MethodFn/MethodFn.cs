namespace LightHouse.Fn
{
    using System;
    using UnityEngine;

    [Serializable]
    public class MethodInfo
    {
        public UnityEngine.Object Target;
        public string MethodName;
    }

    [Serializable]
    public class MethodFn<TResult> : IFn<ITupleBase, TResult>
    {
        public MethodInfo MethodInfo;

        Func<TResult> _delegate;
        public TResult Invoke(ITupleBase _)
        {
            if (_delegate == null)
            {
                if (MethodInfo == null || MethodInfo.Target == null || string.IsNullOrEmpty(MethodInfo.MethodName))
                    throw new Exception();
                string methodName = MethodInfo.MethodName.Split("(")[0].Split(" ")[^1];
                if (typeof(TResult) == typeof(Tuple))
                {
                    Action method = (Action)Delegate.CreateDelegate(type: typeof(Action), target: MethodInfo.Target, method: methodName);
                    _delegate = () =>
                    {
                        method();
                        return (TResult)(object)(new Tuple()); // Cast is safe because `typeof(TResult) == typeof(Tuple)`.
                    };
                }
                else
                    _delegate = (Func<TResult>)Delegate.CreateDelegate(type: typeof(Func<TResult>), target: MethodInfo.Target, method: methodName);
            }
            return _delegate();
        }
    }

    [Serializable]
    public class MethodFn<T1, TResult> : IFn<ITuple<T1>, TResult>, IFn<ITupleBase, TResult>
    {
        public MethodInfo MethodInfo;
        public Tuple<T1> DefaultParam;

        Func<T1, TResult> _delegate;
        public TResult Invoke(ITuple<T1> param)
        {

            if (_delegate == null)
            {
                if (MethodInfo == null || MethodInfo.Target == null || string.IsNullOrEmpty(MethodInfo.MethodName))
                    throw new Exception();
                string methodName = MethodInfo.MethodName.Split("(")[0].Split(" ")[^1];
                if (typeof(TResult) == typeof(Tuple))
                {
                    Action<T1> method = (Action<T1>)Delegate.CreateDelegate(type: typeof(Action<T1>), target: MethodInfo.Target, method: methodName);
                    _delegate = (p1) =>
                    {
                        method(p1);
                        return (TResult)(object)(new Tuple()); // Cast is safe because `typeof(TResult) == typeof(Tuple)`.
                    };
                }
                else
                    _delegate = (Func<T1, TResult>)Delegate.CreateDelegate(type: typeof(Func<T1, TResult>), target: MethodInfo.Target, method: methodName);
            }
            return _delegate(param.Item1);
        }

        public TResult Invoke(ITupleBase _)
        {
            if (_delegate == null)
            {
                if (MethodInfo == null || MethodInfo.Target == null || string.IsNullOrEmpty(MethodInfo.MethodName))
                    throw new Exception();
                string methodName = MethodInfo.MethodName.Split("(")[0].Split(" ")[^1];
                if (typeof(TResult) == typeof(Tuple))
                {
                    Action<T1> method = (Action<T1>)Delegate.CreateDelegate(type: typeof(Action<T1>), target: MethodInfo.Target, method: methodName);
                    _delegate = (p1) =>
                    {
                        method(p1);
                        return (TResult)(object)(new Tuple()); // Cast is safe because `typeof(TResult) == typeof(Tuple)`.
                    };
                }
                else
                    _delegate = (Func<T1, TResult>)Delegate.CreateDelegate(type: typeof(Func<T1, TResult>), target: MethodInfo.Target, method: methodName);
            }
            return _delegate(DefaultParam.Item1);
        }
    }

    [Serializable]
    public class MethodFn<T1, T2, TResult> : IFn<ITuple<T1, T2>, TResult>, IFn<ITupleBase, TResult>
    {
        public MethodInfo MethodInfo;
        public Tuple<T1, T2> DefaultParam;

        Func<T1, T2, TResult> _delegate;
        public TResult Invoke(ITuple<T1, T2> param)
        {
            if (_delegate == null)
            {
                if (MethodInfo == null || MethodInfo.Target == null || string.IsNullOrEmpty(MethodInfo.MethodName))
                    throw new Exception();
                string methodName = MethodInfo.MethodName.Split("(")[0].Split(" ")[^1];
                if (typeof(TResult) == typeof(Tuple))
                {
                    Action<T1, T2> method = (Action<T1, T2>)Delegate.CreateDelegate(type: typeof(Action<T1, T2>), target: MethodInfo.Target, method: methodName);
                    _delegate = (p1, p2) =>
                    {
                        method(p1, p2);
                        return (TResult)(object)(new Tuple());
                    };
                }
                else
                    _delegate = (Func<T1, T2, TResult>)Delegate.CreateDelegate(type: typeof(Func<T1, T2, TResult>), target: MethodInfo.Target, method: methodName);
            }
            return _delegate(param.Item1, param.Item2);
        }

        public TResult Invoke(ITupleBase _)
        {
            if (_delegate == null)
            {
                if (MethodInfo == null || MethodInfo.Target == null || string.IsNullOrEmpty(MethodInfo.MethodName))
                    throw new Exception();
                string methodName = MethodInfo.MethodName.Split("(")[0].Split(" ")[^1];
                if (typeof(TResult) == typeof(Tuple))
                {
                    Action<T1, T2> method = (Action<T1, T2>)Delegate.CreateDelegate(type: typeof(Action<T1, T2>), target: MethodInfo.Target, method: methodName);
                    _delegate = (p1, p2) =>
                    {
                        method(p1, p2);
                        return (TResult)(object)(new Tuple());
                    };
                }
                else
                    _delegate = (Func<T1, T2, TResult>)Delegate.CreateDelegate(type: typeof(Func<T1, T2, TResult>), target: MethodInfo.Target, method: methodName);
            }
            return _delegate(DefaultParam.Item1, DefaultParam.Item2);
        }
    }

    [Serializable]
    public class MethodFn<T1, T2, T3, TResult> : IFn<ITuple<T1, T2, T3>, TResult>, IFn<ITupleBase, TResult>
    {
        public MethodInfo MethodInfo;
        public Tuple<T1, T2, T3> DefaultParam;

        Func<T1, T2, T3, TResult> _delegate;
        public TResult Invoke(ITuple<T1, T2, T3> param)
        {
            if (_delegate == null)
            {
                if (MethodInfo == null || MethodInfo.Target == null || string.IsNullOrEmpty(MethodInfo.MethodName))
                    throw new Exception();
                string methodName = MethodInfo.MethodName.Split("(")[0].Split(" ")[^1];
                if (typeof(TResult) == typeof(Tuple))
                {
                    Action<T1, T2, T3> method = (Action<T1, T2, T3>)Delegate.CreateDelegate(type: typeof(Action<T1, T2, T3>), target: MethodInfo.Target, method: methodName);
                    _delegate = (p1, p2, p3) =>
                    {
                        method(p1, p2, p3);
                        return (TResult)(object)(new Tuple());
                    };
                }
                else
                    _delegate = (Func<T1, T2, T3, TResult>)Delegate.CreateDelegate(type: typeof(Func<T1, T2, T3, TResult>), target: MethodInfo.Target, method: methodName);
            }
            return _delegate(param.Item1, param.Item2, param.Item3);
        }

        public TResult Invoke(ITupleBase _)
        {
            if (_delegate == null)
            {
                if (MethodInfo == null || MethodInfo.Target == null || string.IsNullOrEmpty(MethodInfo.MethodName))
                    throw new Exception();
                string methodName = MethodInfo.MethodName.Split("(")[0].Split(" ")[^1];
                if (typeof(TResult) == typeof(Tuple))
                {
                    Action<T1, T2, T3> method = (Action<T1, T2, T3>)Delegate.CreateDelegate(type: typeof(Action<T1, T2, T3>), target: MethodInfo.Target, method: methodName);
                    _delegate = (p1, p2, p3) =>
                    {
                        method(p1, p2, p3);
                        return (TResult)(object)(new Tuple());
                    };
                }
                else
                    _delegate = (Func<T1, T2, T3, TResult>)Delegate.CreateDelegate(type: typeof(Func<T1, T2, T3, TResult>), target: MethodInfo.Target, method: methodName);
            }
            return _delegate(DefaultParam.Item1, DefaultParam.Item2, DefaultParam.Item3);
        }
    }

    [Serializable]
    public class MethodFn<T1, T2, T3, T4, TResult> : IFn<ITuple<T1, T2, T3, T4>, TResult>, IFn<ITupleBase, TResult>
    {
        public MethodInfo MethodInfo;
        public Tuple<T1, T2, T3, T4> DefaultParam;

        Func<T1, T2, T3, T4, TResult> _delegate;
        public TResult Invoke(ITuple<T1, T2, T3, T4> param)
        {
            if (_delegate == null)
            {
                if (MethodInfo == null || MethodInfo.Target == null || string.IsNullOrEmpty(MethodInfo.MethodName))
                    throw new Exception();
                string methodName = MethodInfo.MethodName.Split("(")[0].Split(" ")[^1];
                if (typeof(TResult) == typeof(Tuple))
                {
                    Action<T1, T2, T3, T4> method = (Action<T1, T2, T3, T4>)Delegate.CreateDelegate(type: typeof(Action<T1, T2, T3, T4>), target: MethodInfo.Target, method: methodName);
                    _delegate = (p1, p2, p3, p4) =>
                    {
                        method(p1, p2, p3, p4);
                        return (TResult)(object)(new Tuple());
                    };
                }
                else
                    _delegate = (Func<T1, T2, T3, T4, TResult>)Delegate.CreateDelegate(type: typeof(Func<T1, T2, T3, T4, TResult>), target: MethodInfo.Target, method: methodName);
            }
            return _delegate(param.Item1, param.Item2, param.Item3, param.Item4);
        }

        public TResult Invoke(ITupleBase _)
        {
            if (_delegate == null)
            {
                if (MethodInfo == null || MethodInfo.Target == null || string.IsNullOrEmpty(MethodInfo.MethodName))
                    throw new Exception();
                string methodName = MethodInfo.MethodName.Split("(")[0].Split(" ")[^1];
                if (typeof(TResult) == typeof(Tuple))
                {
                    Action<T1, T2, T3, T4> method = (Action<T1, T2, T3, T4>)Delegate.CreateDelegate(type: typeof(Action<T1, T2, T3, T4>), target: MethodInfo.Target, method: methodName);
                    _delegate = (p1, p2, p3, p4) =>
                    {
                        method(p1, p2, p3, p4);
                        return (TResult)(object)(new Tuple());
                    };
                }
                else
                    _delegate = (Func<T1, T2, T3, T4, TResult>)Delegate.CreateDelegate(type: typeof(Func<T1, T2, T3, T4, TResult>), target: MethodInfo.Target, method: methodName);
            }
            return _delegate(DefaultParam.Item1, DefaultParam.Item2, DefaultParam.Item3, DefaultParam.Item4);
        }
    }

    [Serializable]
    public class MethodFn<T1, T2, T3, T4, T5, TResult> : IFn<ITuple<T1, T2, T3, T4, T5>, TResult>, IFn<ITupleBase, TResult>
    {
        public MethodInfo MethodInfo;
        public Tuple<T1, T2, T3, T4, T5> DefaultParam;

        Func<T1, T2, T3, T4, T5, TResult> _delegate;
        public TResult Invoke(ITuple<T1, T2, T3, T4, T5> param)
        {
            if (_delegate == null)
            {
                if (MethodInfo == null || MethodInfo.Target == null || string.IsNullOrEmpty(MethodInfo.MethodName))
                    throw new Exception();
                string methodName = MethodInfo.MethodName.Split("(")[0].Split(" ")[^1];
                if (typeof(TResult) == typeof(Tuple))
                {
                    Action<T1, T2, T3, T4, T5> method = (Action<T1, T2, T3, T4, T5>)Delegate.CreateDelegate(type: typeof(Action<T1, T2, T3, T4, T5>), target: MethodInfo.Target, method: methodName);
                    _delegate = (p1, p2, p3, p4, p5) =>
                    {
                        method(p1, p2, p3, p4, p5);
                        return (TResult)(object)(new Tuple());
                    };
                }
                else
                    _delegate = (Func<T1, T2, T3, T4, T5, TResult>)Delegate.CreateDelegate(type: typeof(Func<T1, T2, T3, T4, T5, TResult>), target: MethodInfo.Target, method: methodName);
            }
            return _delegate(param.Item1, param.Item2, param.Item3, param.Item4, param.Item5);
        }

        public TResult Invoke(ITupleBase _)
        {
            if (_delegate == null)
            {
                if (MethodInfo == null || MethodInfo.Target == null || string.IsNullOrEmpty(MethodInfo.MethodName))
                    throw new Exception();
                string methodName = MethodInfo.MethodName.Split("(")[0].Split(" ")[^1];
                if (typeof(TResult) == typeof(Tuple))
                {
                    Action<T1, T2, T3, T4, T5> method = (Action<T1, T2, T3, T4, T5>)Delegate.CreateDelegate(type: typeof(Action<T1, T2, T3, T4, T5>), target: MethodInfo.Target, method: methodName);
                    _delegate = (p1, p2, p3, p4, p5) =>
                    {
                        method(p1, p2, p3, p4, p5);
                        return (TResult)(object)(new Tuple());
                    };
                }
                else
                    _delegate = (Func<T1, T2, T3, T4, T5, TResult>)Delegate.CreateDelegate(type: typeof(Func<T1, T2, T3, T4, T5, TResult>), target: MethodInfo.Target, method: methodName);
            }
            return _delegate(DefaultParam.Item1, DefaultParam.Item2, DefaultParam.Item3, DefaultParam.Item4, DefaultParam.Item5);
        }
    }

    [Serializable]
    public class MethodFn<T1, T2, T3, T4, T5, T6, TResult> : IFn<ITuple<T1, T2, T3, T4, T5, T6>, TResult>, IFn<ITupleBase, TResult>
    {
        public MethodInfo MethodInfo;
        public Tuple<T1, T2, T3, T4, T5, T6> DefaultParam;

        Func<T1, T2, T3, T4, T5, T6, TResult> _delegate;
        public TResult Invoke(ITuple<T1, T2, T3, T4, T5, T6> param)
        {
            if (_delegate == null)
            {
                if (MethodInfo == null || MethodInfo.Target == null || string.IsNullOrEmpty(MethodInfo.MethodName))
                    throw new Exception();
                string methodName = MethodInfo.MethodName.Split("(")[0].Split(" ")[^1];
                if (typeof(TResult) == typeof(Tuple))
                {
                    Action<T1, T2, T3, T4, T5, T6> method = (Action<T1, T2, T3, T4, T5, T6>)Delegate.CreateDelegate(type: typeof(Action<T1, T2, T3, T4, T5, T6>), target: MethodInfo.Target, method: methodName);
                    _delegate = (p1, p2, p3, p4, p5, p6) =>
                    {
                        method(p1, p2, p3, p4, p5, p6);
                        return (TResult)(object)(new Tuple());
                    };
                }
                else
                    _delegate = (Func<T1, T2, T3, T4, T5, T6, TResult>)Delegate.CreateDelegate(type: typeof(Func<T1, T2, T3, T4, T5, T6, TResult>), target: MethodInfo.Target, method: methodName);
            }
            return _delegate(param.Item1, param.Item2, param.Item3, param.Item4, param.Item5, param.Item6);
        }

        public TResult Invoke(ITupleBase _)
        {
            if (_delegate == null)
            {
                if (MethodInfo == null || MethodInfo.Target == null || string.IsNullOrEmpty(MethodInfo.MethodName))
                    throw new Exception();
                string methodName = MethodInfo.MethodName.Split("(")[0].Split(" ")[^1];
                if (typeof(TResult) == typeof(Tuple))
                {
                    Action<T1, T2, T3, T4, T5, T6> method = (Action<T1, T2, T3, T4, T5, T6>)Delegate.CreateDelegate(type: typeof(Action<T1, T2, T3, T4, T5, T6>), target: MethodInfo.Target, method: methodName);
                    _delegate = (p1, p2, p3, p4, p5, p6) =>
                    {
                        method(p1, p2, p3, p4, p5, p6);
                        return (TResult)(object)(new Tuple());
                    };
                }
                else
                    _delegate = (Func<T1, T2, T3, T4, T5, T6, TResult>)Delegate.CreateDelegate(type: typeof(Func<T1, T2, T3, T4, T5, T6, TResult>), target: MethodInfo.Target, method: methodName);
            }
            return _delegate(DefaultParam.Item1, DefaultParam.Item2, DefaultParam.Item3, DefaultParam.Item4, DefaultParam.Item5, DefaultParam.Item6);
        }
    }

    [Serializable]
    public class MethodFn<T1, T2, T3, T4, T5, T6, T7, TResult> : IFn<ITuple<T1, T2, T3, T4, T5, T6, T7>, TResult>, IFn<ITupleBase, TResult>
    {
        public MethodInfo MethodInfo;
        public Tuple<T1, T2, T3, T4, T5, T6, T7> DefaultParam;

        Func<T1, T2, T3, T4, T5, T6, T7, TResult> _delegate;
        public TResult Invoke(ITuple<T1, T2, T3, T4, T5, T6, T7> param)
        {
            if (_delegate == null)
            {
                if (MethodInfo == null || MethodInfo.Target == null || string.IsNullOrEmpty(MethodInfo.MethodName))
                    throw new Exception();
                string methodName = MethodInfo.MethodName.Split("(")[0].Split(" ")[^1];
                if (typeof(TResult) == typeof(Tuple))
                {
                    Action<T1, T2, T3, T4, T5, T6, T7> method = (Action<T1, T2, T3, T4, T5, T6, T7>)Delegate.CreateDelegate(type: typeof(Action<T1, T2, T3, T4, T5, T6, T7>), target: MethodInfo.Target, method: methodName);
                    _delegate = (p1, p2, p3, p4, p5, p6, p7) =>
                    {
                        method(p1, p2, p3, p4, p5, p6, p7);
                        return (TResult)(object)(new Tuple());
                    };
                }
                else
                    _delegate = (Func<T1, T2, T3, T4, T5, T6, T7, TResult>)Delegate.CreateDelegate(type: typeof(Func<T1, T2, T3, T4, T5, T6, T7, TResult>), target: MethodInfo.Target, method: methodName);
            }
            return _delegate(param.Item1, param.Item2, param.Item3, param.Item4, param.Item5, param.Item6, param.Item7);
        }

        public TResult Invoke(ITupleBase _)
        {
            if (_delegate == null)
            {
                if (MethodInfo == null || MethodInfo.Target == null || string.IsNullOrEmpty(MethodInfo.MethodName))
                    throw new Exception();
                string methodName = MethodInfo.MethodName.Split("(")[0].Split(" ")[^1];
                if (typeof(TResult) == typeof(Tuple))
                {
                    Action<T1, T2, T3, T4, T5, T6, T7> method = (Action<T1, T2, T3, T4, T5, T6, T7>)Delegate.CreateDelegate(type: typeof(Action<T1, T2, T3, T4, T5, T6, T7>), target: MethodInfo.Target, method: methodName);
                    _delegate = (p1, p2, p3, p4, p5, p6, p7) =>
                    {
                        method(p1, p2, p3, p4, p5, p6, p7);
                        return (TResult)(object)(new Tuple());
                    };
                }
                else
                    _delegate = (Func<T1, T2, T3, T4, T5, T6, T7, TResult>)Delegate.CreateDelegate(type: typeof(Func<T1, T2, T3, T4, T5, T6, T7, TResult>), target: MethodInfo.Target, method: methodName);
            }
            return _delegate(DefaultParam.Item1, DefaultParam.Item2, DefaultParam.Item3, DefaultParam.Item4, DefaultParam.Item5, DefaultParam.Item6, DefaultParam.Item7);
        }
    }

    [Serializable]
    public class MethodFn<T1, T2, T3, T4, T5, T6, T7, T8, TResult> : IFn<ITuple<T1, T2, T3, T4, T5, T6, T7, T8>, TResult>, IFn<ITupleBase, TResult>
    {
        public MethodInfo MethodInfo;
        public Tuple<T1, T2, T3, T4, T5, T6, T7, T8> DefaultParam;

        Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> _delegate;
        public TResult Invoke(ITuple<T1, T2, T3, T4, T5, T6, T7, T8> param)
        {
            if (_delegate == null)
            {
                if (MethodInfo == null || MethodInfo.Target == null || string.IsNullOrEmpty(MethodInfo.MethodName))
                    throw new Exception();
                string methodName = MethodInfo.MethodName.Split("(")[0].Split(" ")[^1];
                if (typeof(TResult) == typeof(Tuple))
                {
                    Action<T1, T2, T3, T4, T5, T6, T7, T8> method = (Action<T1, T2, T3, T4, T5, T6, T7, T8>)Delegate.CreateDelegate(type: typeof(Action<T1, T2, T3, T4, T5, T6, T7, T8>), target: MethodInfo.Target, method: methodName);
                    _delegate = (p1, p2, p3, p4, p5, p6, p7, p8) =>
                    {
                        method(p1, p2, p3, p4, p5, p6, p7, p8);
                        return (TResult)(object)(new Tuple());
                    };
                }
                else
                    _delegate = (Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult>)Delegate.CreateDelegate(type: typeof(Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult>), target: MethodInfo.Target, method: methodName);
            }
            return _delegate(param.Item1, param.Item2, param.Item3, param.Item4, param.Item5, param.Item6, param.Item7, param.Item8);
        }

        public TResult Invoke(ITupleBase _)
        {
            if (_delegate == null)
            {
                if (MethodInfo == null || MethodInfo.Target == null || string.IsNullOrEmpty(MethodInfo.MethodName))
                    throw new Exception();
                string methodName = MethodInfo.MethodName.Split("(")[0].Split(" ")[^1];
                if (typeof(TResult) == typeof(Tuple))
                {
                    Action<T1, T2, T3, T4, T5, T6, T7, T8> method = (Action<T1, T2, T3, T4, T5, T6, T7, T8>)Delegate.CreateDelegate(type: typeof(Action<T1, T2, T3, T4, T5, T6, T7, T8>), target: MethodInfo.Target, method: methodName);
                    _delegate = (p1, p2, p3, p4, p5, p6, p7, p8) =>
                    {
                        method(p1, p2, p3, p4, p5, p6, p7, p8);
                        return (TResult)(object)(new Tuple());
                    };
                }
                else
                    _delegate = (Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult>)Delegate.CreateDelegate(type: typeof(Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult>), target: MethodInfo.Target, method: methodName);
            }
            return _delegate(DefaultParam.Item1, DefaultParam.Item2, DefaultParam.Item3, DefaultParam.Item4, DefaultParam.Item5, DefaultParam.Item6, DefaultParam.Item7, DefaultParam.Item8);
        }
    }
}
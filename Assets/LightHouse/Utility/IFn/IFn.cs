namespace LightHouse.Fn
{
    using System;

    public interface ITupleBase { }
    public interface ITuple : ITupleBase { }
    public interface ITuple<out T1> : ITupleBase
    {
        public T1 Item1 { get; }
    }
    public interface ITuple<out T1, out T2> : ITupleBase
    {
        public T1 Item1 { get; }
        public T2 Item2 { get; }
    }
    public interface ITuple<out T1, out T2, out T3> : ITupleBase
    {
        public T1 Item1 { get; }
        public T2 Item2 { get; }
        public T3 Item3 { get; }
    }
    public interface ITuple<out T1, out T2, out T3, out T4> : ITupleBase
    {
        public T1 Item1 { get; }
        public T2 Item2 { get; }
        public T3 Item3 { get; }
        public T4 Item4 { get; }
    }
    public interface ITuple<out T1, out T2, out T3, out T4, out T5> : ITupleBase
    {
        public T1 Item1 { get; }
        public T2 Item2 { get; }
        public T3 Item3 { get; }
        public T4 Item4 { get; }
        public T5 Item5 { get; }
    }
    public interface ITuple<out T1, out T2, out T3, out T4, out T5, out T6> : ITupleBase
    {
        public T1 Item1 { get; }
        public T2 Item2 { get; }
        public T3 Item3 { get; }
        public T4 Item4 { get; }
        public T5 Item5 { get; }
        public T6 Item6 { get; }
    }
    public interface ITuple<out T1, out T2, out T3, out T4, out T5, out T6, out T7> : ITupleBase
    {
        public T1 Item1 { get; }
        public T2 Item2 { get; }
        public T3 Item3 { get; }
        public T4 Item4 { get; }
        public T5 Item5 { get; }
        public T6 Item6 { get; }
        public T7 Item7 { get; }
    }
    public interface ITuple<out T1, out T2, out T3, out T4, out T5, out T6, out T7, out T8> : ITupleBase
    {
        public T1 Item1 { get; }
        public T2 Item2 { get; }
        public T3 Item3 { get; }
        public T4 Item4 { get; }
        public T5 Item5 { get; }
        public T6 Item6 { get; }
        public T7 Item7 { get; }
        public T8 Item8 { get; }
    }


    [Serializable]
    public struct Tuple<T1> : ITuple<T1>
    {
        public T1 Item1 { get; set; }

        public Tuple(T1 item1)
        {
            Item1 = item1;
        }
    }
    [Serializable]
    public struct Tuple<T1, T2> : ITuple<T1, T2>
    {
        public T1 Item1 { get; set; }
        public T2 Item2 { get; set; }

        public Tuple(T1 item1, T2 item2)
        {
            Item1 = item1;
            Item2 = item2;
        }
    }

    [Serializable]
    public struct Tuple<T1, T2, T3> : ITuple<T1, T2, T3>
    {
        public T1 Item1 { get; set; }
        public T2 Item2 { get; set; }
        public T3 Item3 { get; set; }

        public Tuple(T1 item1, T2 item2, T3 item3)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
        }
    }
    [Serializable]
    public struct Tuple<T1, T2, T3, T4> : ITuple<T1, T2, T3, T4>
    {
        public T1 Item1 { get; set; }
        public T2 Item2 { get; set; }
        public T3 Item3 { get; set; }
        public T4 Item4 { get; set; }

        public Tuple(T1 item1, T2 item2, T3 item3, T4 item4)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
            Item4 = item4;
        }
    }
    [Serializable]
    public struct Tuple<T1, T2, T3, T4, T5> : ITuple<T1, T2, T3, T4, T5>
    {
        public T1 Item1 { get; set; }
        public T2 Item2 { get; set; }
        public T3 Item3 { get; set; }
        public T4 Item4 { get; set; }
        public T5 Item5 { get; set; }


        public Tuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
            Item4 = item4;
            Item5 = item5;
        }
    }
    [Serializable]
    public struct Tuple<T1, T2, T3, T4, T5, T6> : ITuple<T1, T2, T3, T4, T5, T6>
    {
        public T1 Item1 { get; set; }
        public T2 Item2 { get; set; }
        public T3 Item3 { get; set; }
        public T4 Item4 { get; set; }
        public T5 Item5 { get; set; }
        public T6 Item6 { get; set; }

        public Tuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
            Item4 = item4;
            Item5 = item5;
            Item6 = item6;
        }
    }
    [Serializable]
    public struct Tuple<T1, T2, T3, T4, T5, T6, T7> : ITuple<T1, T2, T3, T4, T5, T6, T7>
    {
        public T1 Item1 { get; set; }
        public T2 Item2 { get; set; }
        public T3 Item3 { get; set; }
        public T4 Item4 { get; set; }
        public T5 Item5 { get; set; }
        public T6 Item6 { get; set; }
        public T7 Item7 { get; set; }

        public Tuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
            Item4 = item4;
            Item5 = item5;
            Item6 = item6;
            Item7 = item7;
        }
    }
    [Serializable]
    public struct Tuple<T1, T2, T3, T4, T5, T6, T7, T8> : ITuple<T1, T2, T3, T4, T5, T6, T7, T8>
    {
        public T1 Item1 { get; set; }
        public T2 Item2 { get; set; }
        public T3 Item3 { get; set; }
        public T4 Item4 { get; set; }
        public T5 Item5 { get; set; }
        public T6 Item6 { get; set; }
        public T7 Item7 { get; set; }
        public T8 Item8 { get; set; }

        public Tuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7, T8 item8)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
            Item4 = item4;
            Item5 = item5;
            Item6 = item6;
            Item7 = item7;
            Item8 = item8;
        }
    }

    public interface IFn<in TParam, out TResult> where TParam : ITupleBase
    {
        public TResult Invoke(TParam param);
    }

    [Serializable]
    public class Then<TParam, TInter, TResult> : IFn<TParam, TResult> where TParam : ITupleBase
    {
        public IFn<TParam, TInter> First;
        public IFn<ITuple<TInter>, TResult> Second;

        public Then(
            IFn<TParam, TInter> first,
            IFn<ITuple<TInter>, TResult> second
        )
        {
            First = first;
            Second = second;
        }

        public TResult Invoke(TParam param)
        {
            return Second.Invoke(new Tuple<TInter>(First.Invoke(param)));
        }
    }

    [Serializable]
    public class Join<TParam, TResult> : IFn<TParam, Tuple<TResult>> where TParam : ITupleBase
    {
        public IFn<TParam, TResult> First;

        public Join(
            IFn<TParam, TResult> first
        )
        {
            First = first;
        }

        public Tuple<TResult> Invoke(TParam param)
        {
            return new Tuple<TResult>(
                First.Invoke(param)
            );
        }
    }
    [Serializable]
    public class Join<TParam, TResult1, TResult2> : IFn<TParam, Tuple<TResult1, TResult2>> where TParam : ITupleBase
    {
        public IFn<TParam, TResult1> First;
        public IFn<TParam, TResult2> Second;

        public Join(
            IFn<TParam, TResult1> first,
            IFn<TParam, TResult2> second
        )
        {
            First = first;
            Second = second;
        }

        public Tuple<TResult1, TResult2> Invoke(TParam param)
        {
            return new Tuple<TResult1, TResult2>(
                First.Invoke(param),
                Second.Invoke(param)
            );
        }
    }
    [Serializable]
    public class Join<TParam, TResult1, TResult2, TResult3> : IFn<TParam, Tuple<TResult1, TResult2, TResult3>> where TParam : ITupleBase
    {
        public IFn<TParam, TResult1> First;
        public IFn<TParam, TResult2> Second;
        public IFn<TParam, TResult3> Third;

        public Join(
            IFn<TParam, TResult1> first,
            IFn<TParam, TResult2> second,
            IFn<TParam, TResult3> third
        )
        {
            First = first;
            Second = second;
            Third = third;
        }

        public Tuple<TResult1, TResult2, TResult3> Invoke(TParam param)
        {
            return new Tuple<TResult1, TResult2, TResult3>(
                First.Invoke(param),
                Second.Invoke(param),
                Third.Invoke(param)
            );
        }
    }
    [Serializable]
    public class Join<TParam, TResult1, TResult2, TResult3, TResult4> : IFn<TParam, Tuple<TResult1, TResult2, TResult3, TResult4>> where TParam : ITupleBase
    {
        public IFn<TParam, TResult1> First;
        public IFn<TParam, TResult2> Second;
        public IFn<TParam, TResult3> Third;
        public IFn<TParam, TResult4> Fourth;

        public Join(
            IFn<TParam, TResult1> first,
            IFn<TParam, TResult2> second,
            IFn<TParam, TResult3> third,
            IFn<TParam, TResult4> fourth
        )
        {
            First = first;
            Second = second;
            Third = third;
            Fourth = fourth;
        }

        public Tuple<TResult1, TResult2, TResult3, TResult4> Invoke(TParam param)
        {
            return new Tuple<TResult1, TResult2, TResult3, TResult4>(
                First.Invoke(param),
                Second.Invoke(param),
                Third.Invoke(param),
                Fourth.Invoke(param)
            );
        }
    }
    [Serializable]
    public class Join<TParam, TResult1, TResult2, TResult3, TResult4, TResult5> : IFn<TParam, Tuple<TResult1, TResult2, TResult3, TResult4, TResult5>> where TParam : ITupleBase
    {
        public IFn<TParam, TResult1> First;
        public IFn<TParam, TResult2> Second;
        public IFn<TParam, TResult3> Third;
        public IFn<TParam, TResult4> Fourth;
        public IFn<TParam, TResult5> Fifth;

        public Join(
            IFn<TParam, TResult1> first,
            IFn<TParam, TResult2> second,
            IFn<TParam, TResult3> third,
            IFn<TParam, TResult4> fourth,
            IFn<TParam, TResult5> fifth
        )
        {
            First = first;
            Second = second;
            Third = third;
            Fourth = fourth;
            Fifth = fifth;
        }

        public Tuple<TResult1, TResult2, TResult3, TResult4, TResult5> Invoke(TParam param)
        {
            return new Tuple<TResult1, TResult2, TResult3, TResult4, TResult5>(
                First.Invoke(param),
                Second.Invoke(param),
                Third.Invoke(param),
                Fourth.Invoke(param),
                Fifth.Invoke(param)
            );
        }
    }
    [Serializable]
    public class Join<TParam, TResult1, TResult2, TResult3, TResult4, TResult5, TResult6> : IFn<TParam, Tuple<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6>> where TParam : ITupleBase
    {
        public IFn<TParam, TResult1> First;
        public IFn<TParam, TResult2> Second;
        public IFn<TParam, TResult3> Third;
        public IFn<TParam, TResult4> Fourth;
        public IFn<TParam, TResult5> Fifth;
        public IFn<TParam, TResult6> Sixth;

        public Join(
            IFn<TParam, TResult1> first,
            IFn<TParam, TResult2> second,
            IFn<TParam, TResult3> third,
            IFn<TParam, TResult4> fourth,
            IFn<TParam, TResult5> fifth,
            IFn<TParam, TResult6> sixth
        )
        {
            First = first;
            Second = second;
            Third = third;
            Fourth = fourth;
            Fifth = fifth;
            Sixth = sixth;
        }

        public Tuple<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6> Invoke(TParam param)
        {
            return new Tuple<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6>(
                First.Invoke(param),
                Second.Invoke(param),
                Third.Invoke(param),
                Fourth.Invoke(param),
                Fifth.Invoke(param),
                Sixth.Invoke(param)
            );
        }
    }
    [Serializable]
    public class Join<TParam, TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7> : IFn<TParam, Tuple<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7>> where TParam : ITupleBase
    {
        public IFn<TParam, TResult1> First;
        public IFn<TParam, TResult2> Second;
        public IFn<TParam, TResult3> Third;
        public IFn<TParam, TResult4> Fourth;
        public IFn<TParam, TResult5> Fifth;
        public IFn<TParam, TResult6> Sixth;
        public IFn<TParam, TResult7> Seventh;

        public Join(
            IFn<TParam, TResult1> first,
            IFn<TParam, TResult2> second,
            IFn<TParam, TResult3> third,
            IFn<TParam, TResult4> fourth,
            IFn<TParam, TResult5> fifth,
            IFn<TParam, TResult6> sixth,
            IFn<TParam, TResult7> seventh
        )
        {
            First = first;
            Second = second;
            Third = third;
            Fourth = fourth;
            Fifth = fifth;
            Sixth = sixth;
            Seventh = seventh;
        }

        public Tuple<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7> Invoke(TParam param)
        {
            return new Tuple<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7>(
                First.Invoke(param),
                Second.Invoke(param),
                Third.Invoke(param),
                Fourth.Invoke(param),
                Fifth.Invoke(param),
                Sixth.Invoke(param),
                Seventh.Invoke(param)
            );
        }
    }
    [Serializable]
    public class Join<TParam, TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8> : IFn<TParam, Tuple<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8>> where TParam : ITupleBase
    {
        public IFn<TParam, TResult1> First;
        public IFn<TParam, TResult2> Second;
        public IFn<TParam, TResult3> Third;
        public IFn<TParam, TResult4> Fourth;
        public IFn<TParam, TResult5> Fifth;
        public IFn<TParam, TResult6> Sixth;
        public IFn<TParam, TResult7> Seventh;
        public IFn<TParam, TResult8> Eighth;

        public Join(
            IFn<TParam, TResult1> first,
            IFn<TParam, TResult2> second,
            IFn<TParam, TResult3> third,
            IFn<TParam, TResult4> fourth,
            IFn<TParam, TResult5> fifth,
            IFn<TParam, TResult6> sixth,
            IFn<TParam, TResult7> seventh,
            IFn<TParam, TResult8> eighth
        )
        {
            First = first;
            Second = second;
            Third = third;
            Fourth = fourth;
            Fifth = fifth;
            Sixth = sixth;
            Seventh = seventh;
            Eighth = eighth;
        }

        public Tuple<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8> Invoke(TParam param)
        {
            return new Tuple<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8>(
                First.Invoke(param),
                Second.Invoke(param),
                Third.Invoke(param),
                Fourth.Invoke(param),
                Fifth.Invoke(param),
                Sixth.Invoke(param),
                Seventh.Invoke(param),
                Eighth.Invoke(param)
            );
        }
    }

    [Serializable]
    public class Unpack<TParam, TResult> : IFn<ITuple<TParam>, TResult> where TParam : ITupleBase
    {
        public IFn<TParam, TResult> Inner;

        public Unpack(
            IFn<TParam, TResult> inner
        )
        {
            Inner = inner;
        }

        public TResult Invoke(ITuple<TParam> param)
        {
            return Inner.Invoke(param.Item1);
        }
    }
}
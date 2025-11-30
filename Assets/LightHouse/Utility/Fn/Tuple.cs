namespace LightHouse.Fn
{
    using System;

    public interface ITupleBase { }
    [Serializable]
    public class Tuple : ITupleBase
    {
        static Tuple _unit = new Tuple();
        public static Tuple Unit { get => _unit; }
    }
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
    public class Tuple<T1> : ITuple<T1>
    {
        [UnityEngine.SerializeField]
        T1 _item1;
        public T1 Item1
        {
            get => _item1;
            set => _item1 = value;
        }

        public Tuple() { }
        public Tuple(T1 item1)
        {
            Item1 = item1;
        }
    }
    [Serializable]
    public class Tuple<T1, T2> : ITuple<T1, T2>
    {
        [UnityEngine.SerializeField]
        T1 _item1;
        [UnityEngine.SerializeField]
        T2 _item2;

        public T1 Item1 { get => _item1; set => _item1 = value; }
        public T2 Item2 { get => _item2; set => _item2 = value; }

        public Tuple() { }
        public Tuple(T1 item1, T2 item2)
        {
            Item1 = item1;
            Item2 = item2;
        }
    }

    [Serializable]
    public class Tuple<T1, T2, T3> : ITuple<T1, T2, T3>
    {
        [UnityEngine.SerializeField]
        T1 _item1;
        [UnityEngine.SerializeField]
        T2 _item2;
        [UnityEngine.SerializeField]
        T3 _item3;

        public T1 Item1 { get => _item1; set => _item1 = value; }
        public T2 Item2 { get => _item2; set => _item2 = value; }
        public T3 Item3 { get => _item3; set => _item3 = value; }

        public Tuple() { }
        public Tuple(T1 item1, T2 item2, T3 item3)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
        }
    }
    [Serializable]
    public class Tuple<T1, T2, T3, T4> : ITuple<T1, T2, T3, T4>
    {
        [UnityEngine.SerializeField]
        T1 _item1;
        [UnityEngine.SerializeField]
        T2 _item2;
        [UnityEngine.SerializeField]
        T3 _item3;
        [UnityEngine.SerializeField]
        T4 _item4;

        public T1 Item1 { get => _item1; set => _item1 = value; }
        public T2 Item2 { get => _item2; set => _item2 = value; }
        public T3 Item3 { get => _item3; set => _item3 = value; }
        public T4 Item4 { get => _item4; set => _item4 = value; }

        public Tuple() { }
        public Tuple(T1 item1, T2 item2, T3 item3, T4 item4)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
            Item4 = item4;
        }
    }
    [Serializable]
    public class Tuple<T1, T2, T3, T4, T5> : ITuple<T1, T2, T3, T4, T5>
    {
        [UnityEngine.SerializeField]
        T1 _item1;
        [UnityEngine.SerializeField]
        T2 _item2;
        [UnityEngine.SerializeField]
        T3 _item3;
        [UnityEngine.SerializeField]
        T4 _item4;
        [UnityEngine.SerializeField]
        T5 _item5;

        public T1 Item1 { get => _item1; set => _item1 = value; }
        public T2 Item2 { get => _item2; set => _item2 = value; }
        public T3 Item3 { get => _item3; set => _item3 = value; }
        public T4 Item4 { get => _item4; set => _item4 = value; }
        public T5 Item5 { get => _item5; set => _item5 = value; }

        public Tuple() { }
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
    public class Tuple<T1, T2, T3, T4, T5, T6> : ITuple<T1, T2, T3, T4, T5, T6>
    {
        [UnityEngine.SerializeField]
        T1 _item1;
        [UnityEngine.SerializeField]
        T2 _item2;
        [UnityEngine.SerializeField]
        T3 _item3;
        [UnityEngine.SerializeField]
        T4 _item4;
        [UnityEngine.SerializeField]
        T5 _item5;
        [UnityEngine.SerializeField]
        T6 _item6;

        public T1 Item1 { get => _item1; set => _item1 = value; }
        public T2 Item2 { get => _item2; set => _item2 = value; }
        public T3 Item3 { get => _item3; set => _item3 = value; }
        public T4 Item4 { get => _item4; set => _item4 = value; }
        public T5 Item5 { get => _item5; set => _item5 = value; }
        public T6 Item6 { get => _item6; set => _item6 = value; }

        public Tuple() { }
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
    public class Tuple<T1, T2, T3, T4, T5, T6, T7> : ITuple<T1, T2, T3, T4, T5, T6, T7>
    {
        [UnityEngine.SerializeField]
        T1 _item1;
        [UnityEngine.SerializeField]
        T2 _item2;
        [UnityEngine.SerializeField]
        T3 _item3;
        [UnityEngine.SerializeField]
        T4 _item4;
        [UnityEngine.SerializeField]
        T5 _item5;
        [UnityEngine.SerializeField]
        T6 _item6;
        [UnityEngine.SerializeField]
        T7 _item7;

        public T1 Item1 { get => _item1; set => _item1 = value; }
        public T2 Item2 { get => _item2; set => _item2 = value; }
        public T3 Item3 { get => _item3; set => _item3 = value; }
        public T4 Item4 { get => _item4; set => _item4 = value; }
        public T5 Item5 { get => _item5; set => _item5 = value; }
        public T6 Item6 { get => _item6; set => _item6 = value; }
        public T7 Item7 { get => _item7; set => _item7 = value; }

        public Tuple() { }
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
    public class Tuple<T1, T2, T3, T4, T5, T6, T7, T8> : ITuple<T1, T2, T3, T4, T5, T6, T7, T8>
    {
        [UnityEngine.SerializeField]
        T1 _item1;
        [UnityEngine.SerializeField]
        T2 _item2;
        [UnityEngine.SerializeField]
        T3 _item3;
        [UnityEngine.SerializeField]
        T4 _item4;
        [UnityEngine.SerializeField]
        T5 _item5;
        [UnityEngine.SerializeField]
        T6 _item6;
        [UnityEngine.SerializeField]
        T7 _item7;
        [UnityEngine.SerializeField]
        T8 _item8;

        public T1 Item1 { get => _item1; set => _item1 = value; }
        public T2 Item2 { get => _item2; set => _item2 = value; }
        public T3 Item3 { get => _item3; set => _item3 = value; }
        public T4 Item4 { get => _item4; set => _item4 = value; }
        public T5 Item5 { get => _item5; set => _item5 = value; }
        public T6 Item6 { get => _item6; set => _item6 = value; }
        public T7 Item7 { get => _item7; set => _item7 = value; }
        public T8 Item8 { get => _item8; set => _item8 = value; }

        public Tuple() { }
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
}
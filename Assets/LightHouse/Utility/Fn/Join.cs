namespace LightHouse.Fn
{
    using System;
    using UnityEngine;

    [Serializable]
    public class Join<TParam, TResult> : IFn<TParam, Tuple<TResult>> where TParam : ITupleBase
    {
        [SerializeReference]
        [PolySelector]
        IFn<TParam, TResult> _first;

        public Join() { }
        public Join(
            IFn<TParam, TResult> first
        )
        {
            _first = first;
        }

        public Tuple<TResult> Invoke(TParam param)
        {
            return new Tuple<TResult>(
                _first.Invoke(param)
            );
        }
    }
    [Serializable]
    public class Join<TParam, TResult1, TResult2> : IFn<TParam, Tuple<TResult1, TResult2>> where TParam : ITupleBase
    {
        [SerializeReference]
        [PolySelector]
        IFn<TParam, TResult1> _first;
        [SerializeReference]
        [PolySelector]
        IFn<TParam, TResult2> _second;

        public Join() { }
        public Join(
            IFn<TParam, TResult1> first,
            IFn<TParam, TResult2> second
        )
        {
            _first = first;
            _second = second;
        }

        public Tuple<TResult1, TResult2> Invoke(TParam param)
        {
            return new Tuple<TResult1, TResult2>(
                _first.Invoke(param),
                _second.Invoke(param)
            );
        }
    }
    [Serializable]
    public class Join<TParam, TResult1, TResult2, TResult3> : IFn<TParam, Tuple<TResult1, TResult2, TResult3>> where TParam : ITupleBase
    {
        [SerializeReference]
        [PolySelector]
        IFn<TParam, TResult1> _first;
        [SerializeReference]
        [PolySelector]
        IFn<TParam, TResult2> _second;
        [SerializeReference]
        [PolySelector]
        IFn<TParam, TResult3> _third;

        public Join() { }
        public Join(
            IFn<TParam, TResult1> first,
            IFn<TParam, TResult2> second,
            IFn<TParam, TResult3> third
        )
        {
            _first = first;
            _second = second;
            _third = third;
        }

        public Tuple<TResult1, TResult2, TResult3> Invoke(TParam param)
        {
            return new Tuple<TResult1, TResult2, TResult3>(
                _first.Invoke(param),
                _second.Invoke(param),
                _third.Invoke(param)
            );
        }
    }
    [Serializable]
    public class Join<TParam, TResult1, TResult2, TResult3, TResult4> : IFn<TParam, Tuple<TResult1, TResult2, TResult3, TResult4>> where TParam : ITupleBase
    {
        [SerializeReference]
        [PolySelector]
        IFn<TParam, TResult1> _first;
        [SerializeReference]
        [PolySelector]
        IFn<TParam, TResult2> _second;
        [SerializeReference]
        [PolySelector]
        IFn<TParam, TResult3> _third;
        [SerializeReference]
        [PolySelector]
        IFn<TParam, TResult4> _fourth;

        public Join() { }
        public Join(
            IFn<TParam, TResult1> first,
            IFn<TParam, TResult2> second,
            IFn<TParam, TResult3> third,
            IFn<TParam, TResult4> fourth
        )
        {
            _first = first;
            _second = second;
            _third = third;
            _fourth = fourth;
        }

        public Tuple<TResult1, TResult2, TResult3, TResult4> Invoke(TParam param)
        {
            return new Tuple<TResult1, TResult2, TResult3, TResult4>(
                _first.Invoke(param),
                _second.Invoke(param),
                _third.Invoke(param),
                _fourth.Invoke(param)
            );
        }
    }
    [Serializable]
    public class Join<TParam, TResult1, TResult2, TResult3, TResult4, TResult5> : IFn<TParam, Tuple<TResult1, TResult2, TResult3, TResult4, TResult5>> where TParam : ITupleBase
    {
        [SerializeReference]
        [PolySelector]
        IFn<TParam, TResult1> _first;
        [SerializeReference]
        [PolySelector]
        IFn<TParam, TResult2> _second;
        [SerializeReference]
        [PolySelector]
        IFn<TParam, TResult3> _third;
        [SerializeReference]
        [PolySelector]
        IFn<TParam, TResult4> _fourth;
        [SerializeReference]
        [PolySelector]
        IFn<TParam, TResult5> _fifth;

        public Join() { }
        public Join(
            IFn<TParam, TResult1> first,
            IFn<TParam, TResult2> second,
            IFn<TParam, TResult3> third,
            IFn<TParam, TResult4> fourth,
            IFn<TParam, TResult5> fifth
        )
        {
            _first = first;
            _second = second;
            _third = third;
            _fourth = fourth;
            _fifth = fifth;
        }

        public Tuple<TResult1, TResult2, TResult3, TResult4, TResult5> Invoke(TParam param)
        {
            return new Tuple<TResult1, TResult2, TResult3, TResult4, TResult5>(
                _first.Invoke(param),
                _second.Invoke(param),
                _third.Invoke(param),
                _fourth.Invoke(param),
                _fifth.Invoke(param)
            );
        }
    }
    [Serializable]
    public class Join<TParam, TResult1, TResult2, TResult3, TResult4, TResult5, TResult6> : IFn<TParam, Tuple<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6>> where TParam : ITupleBase
    {
        [SerializeReference]
        [PolySelector]
        IFn<TParam, TResult1> _first;
        [SerializeReference]
        [PolySelector]
        IFn<TParam, TResult2> _second;
        [SerializeReference]
        [PolySelector]
        IFn<TParam, TResult3> _third;
        [SerializeReference]
        [PolySelector]
        IFn<TParam, TResult4> _fourth;
        [SerializeReference]
        [PolySelector]
        IFn<TParam, TResult5> _fifth;
        [SerializeReference]
        [PolySelector]
        IFn<TParam, TResult6> _sixth;

        public Join() { }
        public Join(
            IFn<TParam, TResult1> first,
            IFn<TParam, TResult2> second,
            IFn<TParam, TResult3> third,
            IFn<TParam, TResult4> fourth,
            IFn<TParam, TResult5> fifth,
            IFn<TParam, TResult6> sixth
        )
        {
            _first = first;
            _second = second;
            _third = third;
            _fourth = fourth;
            _fifth = fifth;
            _sixth = sixth;
        }

        public Tuple<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6> Invoke(TParam param)
        {
            return new Tuple<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6>(
                _first.Invoke(param),
                _second.Invoke(param),
                _third.Invoke(param),
                _fourth.Invoke(param),
                _fifth.Invoke(param),
                _sixth.Invoke(param)
            );
        }
    }
    [Serializable]
    public class Join<TParam, TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7> : IFn<TParam, Tuple<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7>> where TParam : ITupleBase
    {
        [SerializeReference]
        [PolySelector]
        IFn<TParam, TResult1> _first;
        [SerializeReference]
        [PolySelector]
        IFn<TParam, TResult2> _second;
        [SerializeReference]
        [PolySelector]
        IFn<TParam, TResult3> _third;
        [SerializeReference]
        [PolySelector]
        IFn<TParam, TResult4> _fourth;
        [SerializeReference]
        [PolySelector]
        IFn<TParam, TResult5> _fifth;
        [SerializeReference]
        [PolySelector]
        IFn<TParam, TResult6> _sixth;
        [SerializeReference]
        [PolySelector]
        IFn<TParam, TResult7> _seventh;

        public Join() { }
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
            _first = first;
            _second = second;
            _third = third;
            _fourth = fourth;
            _fifth = fifth;
            _sixth = sixth;
            _seventh = seventh;
        }

        public Tuple<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7> Invoke(TParam param)
        {
            return new Tuple<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7>(
                _first.Invoke(param),
                _second.Invoke(param),
                _third.Invoke(param),
                _fourth.Invoke(param),
                _fifth.Invoke(param),
                _sixth.Invoke(param),
                _seventh.Invoke(param)
            );
        }
    }
    [Serializable]
    public class Join<TParam, TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8> : IFn<TParam, Tuple<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8>> where TParam : ITupleBase
    {
        [SerializeReference]
        [PolySelector]
        IFn<TParam, TResult1> _first;
        [SerializeReference]
        [PolySelector]
        IFn<TParam, TResult2> _second;
        [SerializeReference]
        [PolySelector]
        IFn<TParam, TResult3> _third;
        [SerializeReference]
        [PolySelector]
        IFn<TParam, TResult4> _fourth;
        [SerializeReference]
        [PolySelector]
        IFn<TParam, TResult5> _fifth;
        [SerializeReference]
        [PolySelector]
        IFn<TParam, TResult6> _sixth;
        [SerializeReference]
        [PolySelector]
        IFn<TParam, TResult7> _seventh;
        [SerializeReference]
        [PolySelector]
        IFn<TParam, TResult8> _eighth;

        public Join() { }
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
            _first = first;
            _second = second;
            _third = third;
            _fourth = fourth;
            _fifth = fifth;
            _sixth = sixth;
            _seventh = seventh;
            _eighth = eighth;
        }

        public Tuple<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8> Invoke(TParam param)
        {
            return new Tuple<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8>(
                _first.Invoke(param),
                _second.Invoke(param),
                _third.Invoke(param),
                _fourth.Invoke(param),
                _fifth.Invoke(param),
                _sixth.Invoke(param),
                _seventh.Invoke(param),
                _eighth.Invoke(param)
            );
        }
    }
}
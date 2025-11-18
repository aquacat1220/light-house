namespace LightHouse.Fn
{
    using System;
    using UnityEngine;

    [Serializable]
    public class Join<TParam, TResult> : IFn<TParam, Tuple<TResult>> where TParam : ITupleBase
    {
        [SerializeReference]
        [SubclassSelector]
        public IFn<TParam, TResult> First;

        public Join() { }
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
        [SerializeReference]
        [SubclassSelector]
        public IFn<TParam, TResult1> First;
        [SerializeReference]
        [SubclassSelector]
        public IFn<TParam, TResult2> Second;

        public Join() { }
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
        [SerializeReference]
        [SubclassSelector]
        public IFn<TParam, TResult1> First;
        [SerializeReference]
        [SubclassSelector]
        public IFn<TParam, TResult2> Second;
        [SerializeReference]
        [SubclassSelector]
        public IFn<TParam, TResult3> Third;

        public Join() { }
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
        [SerializeReference]
        [SubclassSelector]
        public IFn<TParam, TResult1> First;
        [SerializeReference]
        [SubclassSelector]
        public IFn<TParam, TResult2> Second;
        [SerializeReference]
        [SubclassSelector]
        public IFn<TParam, TResult3> Third;
        [SerializeReference]
        [SubclassSelector]
        public IFn<TParam, TResult4> Fourth;

        public Join() { }
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
        [SerializeReference]
        [SubclassSelector]
        public IFn<TParam, TResult1> First;
        [SerializeReference]
        [SubclassSelector]
        public IFn<TParam, TResult2> Second;
        [SerializeReference]
        [SubclassSelector]
        public IFn<TParam, TResult3> Third;
        [SerializeReference]
        [SubclassSelector]
        public IFn<TParam, TResult4> Fourth;
        [SerializeReference]
        [SubclassSelector]
        public IFn<TParam, TResult5> Fifth;

        public Join() { }
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
        [SerializeReference]
        [SubclassSelector]
        public IFn<TParam, TResult1> First;
        [SerializeReference]
        [SubclassSelector]
        public IFn<TParam, TResult2> Second;
        [SerializeReference]
        [SubclassSelector]
        public IFn<TParam, TResult3> Third;
        [SerializeReference]
        [SubclassSelector]
        public IFn<TParam, TResult4> Fourth;
        [SerializeReference]
        [SubclassSelector]
        public IFn<TParam, TResult5> Fifth;
        [SerializeReference]
        [SubclassSelector]
        public IFn<TParam, TResult6> Sixth;

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
        [SerializeReference]
        [SubclassSelector]
        public IFn<TParam, TResult1> First;
        [SerializeReference]
        [SubclassSelector]
        public IFn<TParam, TResult2> Second;
        [SerializeReference]
        [SubclassSelector]
        public IFn<TParam, TResult3> Third;
        [SerializeReference]
        [SubclassSelector]
        public IFn<TParam, TResult4> Fourth;
        [SerializeReference]
        [SubclassSelector]
        public IFn<TParam, TResult5> Fifth;
        [SerializeReference]
        [SubclassSelector]
        public IFn<TParam, TResult6> Sixth;
        [SerializeReference]
        [SubclassSelector]
        public IFn<TParam, TResult7> Seventh;

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
        [SerializeReference]
        [SubclassSelector]
        public IFn<TParam, TResult1> First;
        [SerializeReference]
        [SubclassSelector]
        public IFn<TParam, TResult2> Second;
        [SerializeReference]
        [SubclassSelector]
        public IFn<TParam, TResult3> Third;
        [SerializeReference]
        [SubclassSelector]
        public IFn<TParam, TResult4> Fourth;
        [SerializeReference]
        [SubclassSelector]
        public IFn<TParam, TResult5> Fifth;
        [SerializeReference]
        [SubclassSelector]
        public IFn<TParam, TResult6> Sixth;
        [SerializeReference]
        [SubclassSelector]
        public IFn<TParam, TResult7> Seventh;
        [SerializeReference]
        [SubclassSelector]
        public IFn<TParam, TResult8> Eighth;

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
}
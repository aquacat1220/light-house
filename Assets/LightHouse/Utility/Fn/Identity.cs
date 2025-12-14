namespace LightHouse.Fn
{
    using System;

    [Serializable]
    public class Identity<TParam> : IFn<ITuple<TParam>, TParam>
    {
        public TParam Invoke(ITuple<TParam> param)
        {
            return param.Item1;
        }
    }
}
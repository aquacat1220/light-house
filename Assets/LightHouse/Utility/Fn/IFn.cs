namespace LightHouse.Fn
{
    public interface IFn<in TParam, out TResult> where TParam : ITupleBase
    {
        public TResult Invoke(TParam param);
    }
}
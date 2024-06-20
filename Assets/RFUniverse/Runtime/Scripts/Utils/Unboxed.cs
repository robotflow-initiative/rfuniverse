namespace RFUniverse
{
    public class Unboxed<T> where T : struct
    {
        public T Value;

        public static implicit operator Unboxed<T>(T value)
        {
            return new Unboxed<T> { Value = value };
        }
        public static explicit operator T(Unboxed<T> obj)
        {
            return obj.Value;
        }
    }
}

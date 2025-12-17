namespace FamoNET.Model.Args
{
    public class GenericEventArgs<T> : EventArgs
    {
        public T Value { get; }
        public GenericEventArgs(T value)
        {
            Value = value;
        }
    }
}

namespace FamoNET.Model.Args
{
    public abstract class ViewportChangedEventArgs<T> : EventArgs
    {
        public Guid ChartGuid { get; }
        public ViewportParams<T> Viewport { get; }

        public ViewportChangedEventArgs(Guid guid, ViewportParams<T> viewport) 
        {
            ChartGuid = guid;
            Viewport = viewport;
        }

    }
}

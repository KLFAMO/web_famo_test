namespace FamoNET.Model.Args
{
    public class DateViewportEventArgs : ViewportChangedEventArgs<DateTime>
    {
        public DateViewportEventArgs(Guid guid, ViewportParams<DateTime> viewport) : base(guid, viewport)
        {
        }
    }
}

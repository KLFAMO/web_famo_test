
namespace FamoNET.Model.Args
{
    public class MjdViewportEventArgs : ViewportChangedEventArgs<double>
    {
        public MjdViewportEventArgs(Guid guid, ViewportParams<double> viewport) : base(guid, viewport)
        {
        }
    }
}

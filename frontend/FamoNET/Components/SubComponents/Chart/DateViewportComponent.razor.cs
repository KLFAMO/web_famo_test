using FamoNET.Model.Args;
using FamoNET.Utils;

namespace FamoNET.Components.SubComponents.Chart
{
    public partial class DateViewportComponent : ViewportComponentBase
    {
        public DateTime StartDate
        {
            get => FamoMath.Convert_MJDToDateTime(MjdViewportParams.MinX).ToLocalTime();
            set => MjdViewportParams.MinX = FamoMath.Convert_DateTimeToMjd(value);
        }

        public DateTime EndDate
        {
            get => FamoMath.Convert_MJDToDateTime(MjdViewportParams.MaxX).ToLocalTime();
            set => MjdViewportParams.MaxX = FamoMath.Convert_DateTimeToMjd(value);
        }
        
        protected override void ChartManagerService_OnViewportChanged(object sender, EventArgs e)
        {
            var eventArgs = e as MjdViewportEventArgs;
            if (eventArgs == null)
            {
                return;
            }

            if (eventArgs.ChartGuid != CurrentChartGuid)
            {
                return;
            }

            MjdViewportParams = eventArgs.Viewport;
            //MjdViewportParams.MinX = FamoMath.Convert_MJDToDateTime(eventArgs.Viewport.MinX);
            //MjdViewportParams.MaxX = FamoMath.Convert_MJDToDateTime(eventArgs.Viewport.MaxX);
            //MjdViewportParams.MinY = eventArgs.Viewport.MinY;
            //MjdViewportParams.MaxY = eventArgs.Viewport.MaxY;
            InvokeAsync(StateHasChanged);
        }
    }
}

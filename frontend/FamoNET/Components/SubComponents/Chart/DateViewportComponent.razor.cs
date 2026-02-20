using FamoNET.Model;
using FamoNET.Model.Args;
using FamoNET.Utils;

namespace FamoNET.Components.SubComponents.Chart
{
    public partial class DateViewportComponent : ViewportComponentBase
    {
        public ViewportParams<DateTime> DateViewportParams { get; set; }
        protected override void ChartManagerService_OnViewportChanged(object sender, EventArgs e)
        {
            var eventArgs = e as DateViewportEventArgs;
            if (eventArgs == null)
            {
                return;
            }

            if (eventArgs.ChartGuid != CurrentChartGuid)
            {
                return;
            }

            DateViewportParams = eventArgs.Viewport;

            MjdViewportParams.MinX = FamoMath.Convert_MJDToDateTime(DateViewportParams.MinX);
            MjdViewportParams.MaxX = FamoMath.Convert_MJDToDateTime(DateViewportParams.MaxX);
            MjdViewportParams.MinY = DateViewportParams.MinY;
            MjdViewportParams.MaxY = DateViewportParams.MaxY;
            InvokeAsync(StateHasChanged);
        }
    }
}

using FamoNET.Model.Args;

namespace FamoNET.Components.SubComponents.Chart
{
    public partial class SecondsViewportComponent : ViewportComponentBase
    {
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
            InvokeAsync(StateHasChanged);
        }
    }
}

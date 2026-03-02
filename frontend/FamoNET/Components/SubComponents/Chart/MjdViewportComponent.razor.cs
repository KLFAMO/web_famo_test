using FamoNET.Model;
using FamoNET.Model.Args;
using FamoNET.Services;

namespace FamoNET.Components.SubComponents.Chart
{
    public partial class MjdViewportComponent : ViewportComponentBase
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

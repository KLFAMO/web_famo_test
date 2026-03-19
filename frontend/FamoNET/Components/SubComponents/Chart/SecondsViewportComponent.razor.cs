using FamoNET.Model.Args;
using Microsoft.AspNetCore.Components;

namespace FamoNET.Components.SubComponents.Chart
{
    public partial class SecondsViewportComponent : ViewportComponentBase
    {
        [Parameter]
        public double ZeroPointMjd { get; set; }
        [Parameter]
        public EventCallback<double> ZeroPointMjdChanged { get; set; }

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

        protected override async Task ApplyParameters()
        {
            await ZeroPointMjdChanged.InvokeAsync(ZeroPointMjd);
        }
    }
}

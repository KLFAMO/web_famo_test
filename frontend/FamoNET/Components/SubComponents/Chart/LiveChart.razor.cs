using FamoNET.Model;

namespace FamoNET.Components.SubComponents.Chart
{
    public partial class LiveChart : ChartWithAllanComponentBase<double>
    {
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            if (firstRender)
            {
                await Initialize(new ChartParameters<double>() { Title = "Data overview", DisableXLabels = false, DisableEvents = false, AxisMode = AxisMode.Mjd });
            }
        }
    }
}

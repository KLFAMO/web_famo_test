using FamoNET.Model;
using FamoNET.Services;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace FamoNET.Components.SubComponents.Chart
{
    public partial class ViewportComponent : ComponentBase
    {       
        [Inject]
        protected ChartManagerService ChartManagerService { get; set; }
        [Parameter]
        public Guid ChartGuid { get; set; }
        private Guid _currentChartGuid { get; set; }
        [Parameter]
        public EventHandler<AxisMode> AxisModeChanged { get; set; }
        public double ZeroPointMjd { get; set; }
        [Parameter]
        public EventCallback<double> ZeroPointMjdChanged { get; set; }
        public AxisMode AxisMode { get; set; }
        private MjdViewportComponent MjdViewportComponent;
        private DateViewportComponent DateViewportComponent;
        private SecondsViewportComponent SecondsViewportComponent;
        
        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();

            if (ChartGuid == _currentChartGuid)
                return;

            _currentChartGuid = ChartGuid;
        }

        public async Task RefreshParameters()
        {
            if (AxisMode == AxisMode.Mjd)
            {
                await MjdViewportComponent.RefreshParameters();
            }
            else if (AxisMode == AxisMode.Date)
            {
                await DateViewportComponent.RefreshParameters();
            }
            else if (AxisMode == AxisMode.Seconds)
            {
                await SecondsViewportComponent.RefreshParameters();
            }
        }

        private async Task OnAxisModeChange()
        {            
            await RefreshParameters();

            if (AxisMode == AxisMode.Date)
            {
                await ChartManagerService.InitializeChart(ChartGuid, new ChartParameters<DateTime>() { Title = "Data overview", DisableXLabels = false, DisableEvents = false, AxisMode = AxisMode.Date });
            }
            else if (AxisMode == AxisMode.Mjd)
            {
                await ChartManagerService.InitializeChart(ChartGuid, new ChartParameters<double>() { Title = "Data overview", DisableXLabels = false, DisableEvents = false, AxisMode = AxisMode.Mjd });
            }
            else if (AxisMode == AxisMode.Seconds)
            {
                await ChartManagerService.InitializeChart(ChartGuid, new ChartParameters<double>() { Title = "Data overview", DisableXLabels = false, DisableEvents = false, AxisMode = AxisMode.Seconds });
            }

            AxisModeChanged?.Invoke(this, AxisMode);
        }

        private async Task OnZeroPointMjdChanged()
        {             
            await ZeroPointMjdChanged.InvokeAsync(ZeroPointMjd);
        }
    }
}

using FamoNET.Model;
using Microsoft.AspNetCore.Components;

namespace FamoNET.Components.SubComponents.Chart
{
    public partial class ViewportComponent : ComponentBase
    {
        [Parameter]
        public Guid ChartGuid { get; set; }
        private Guid _currentChartGuid { get; set; }
        public AxisMode AxisMode { get; set; }
        
        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();

            if (ChartGuid == _currentChartGuid)
                return;

            _currentChartGuid = ChartGuid;
        }
    }
}

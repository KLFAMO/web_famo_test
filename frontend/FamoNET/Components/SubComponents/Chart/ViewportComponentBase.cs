using FamoNET.Model;
using FamoNET.Services;
using Microsoft.AspNetCore.Components;

namespace FamoNET.Components.SubComponents.Chart
{
    public abstract class ViewportComponentBase : ComponentBase
    {        
        [Inject]
        protected ChartManagerService ChartManagerService { get; set; }
        [Parameter]
        public Guid ChartGuid { get; set; }
        protected ViewportParams<double> MjdViewportParams { get; set; }
        protected Guid CurrentChartGuid { get; set; }
        protected string Title { get; set; }

        protected override async Task OnInitializedAsync()
        {
            ChartManagerService.OnViewportChanged += ChartManagerService_OnViewportChanged;
            await base.OnInitializedAsync();
        }

        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();

            if (CurrentChartGuid == ChartGuid)
            {
                return;
            }

            CurrentChartGuid = ChartGuid;
            StateHasChanged();
        }

        public async Task RefreshParameters()
        {
            MjdViewportParams = await ChartManagerService.GetViewportParameters(CurrentChartGuid);
            StateHasChanged();
        }


        protected async Task ApplyParameters()
        {
            await ChartManagerService.SetChartParameters(CurrentChartGuid, new ChartParameters<double>() { Title = Title });
            await ChartManagerService.SetViewportParameters(CurrentChartGuid, MjdViewportParams);
        }


        protected abstract void ChartManagerService_OnViewportChanged(object sender, EventArgs e);        
    }
}

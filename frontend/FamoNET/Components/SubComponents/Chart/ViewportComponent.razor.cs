using FamoNET.Model;
using FamoNET.Model.Args;
using FamoNET.Services;
using Microsoft.AspNetCore.Components;
using System;

namespace FamoNET.Components.SubComponents.Chart
{
    public partial class ViewportComponent : ComponentBase
    {
        [Inject]
        private ChartManagerService _chartManagerService { get; set; }
        [Parameter]
        public Guid ChartGuid { get; set; }
        
        private Guid _currentChartGuid { get; set; }
        private string Title { get; set; }
        private ViewportParams<double> ViewportParams { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            _chartManagerService.OnViewportChanged += ChartManagerService_OnViewportChanged;
        }

        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();

            if (_currentChartGuid == ChartGuid)
            {
                return;
            }
            
            _currentChartGuid = ChartGuid;            
            StateHasChanged();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            //component still empty on initial render. Need to populate ViewportParams somehow (after data load). Check why OnViewPortChanged not firing after render.
            if (_chartManagerService.IsInitialized && ViewportParams == null)
            {
                ViewportParams = await _chartManagerService.GetViewportParameters(_currentChartGuid);
                StateHasChanged();
            }
        }
        private async Task ApplyParameters()
        {
            await _chartManagerService.SetChartParameters(_currentChartGuid, new ChartParameters<double>() { Title = Title } );
            await _chartManagerService.SetViewportParameters(_currentChartGuid, ViewportParams );            
        }

        protected void ChartManagerService_OnViewportChanged(object sender, EventArgs e)
        {
            var eventArgs = e as MjdViewportEventArgs;
            if (eventArgs == null)
            {
                return;
            }

            if (eventArgs.ChartGuid != ChartGuid)
            {
                return;
            }
         
            ViewportParams = eventArgs.Viewport;
            StateHasChanged();
        }
    }
}

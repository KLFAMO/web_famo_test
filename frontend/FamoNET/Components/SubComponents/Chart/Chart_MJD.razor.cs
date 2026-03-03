using FamoNET.Model;
using FamoNET.Model.Args;
using FamoNET.Model.Interfaces;
using FamoNET.Services;
using Microsoft.AspNetCore.Components;

namespace FamoNET.Components.SubComponents.Chart
{
    public partial class Chart_MJD : ChartWithAllanComponentBase<double>
    {
        [Inject]
        private ISystemNotificationService _notificationService { get; set; }        
        private DataSeries<double> SelectedSeries { get; set; }
        private SeriesListComponent SeriesListComponent;
        private ViewportComponent ViewportComponent;
        public Chart_MJD()
        {
            Model = new ChartParameters<double>();
        }        

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            if (firstRender)
            {
                await Initialize(new ChartParameters<double>() { Title = "Data overview", DisableXLabels = false, DisableEvents = false, AxisMode = AxisMode.Mjd });
            }
        }

        public override Task LoadData(List<DataPoint<double>> data, string label)
        {                        
            SeriesListComponent.AddSeries(label, data);            
            return Task.CompletedTask;
        }

        public async Task Redraw()
        {
            await ChartManagerService.ClearDataSets(ChartGuid, false);
            for (int i = 0; i < SeriesListComponent.SeriesList.Count; ++i)
            {
                await ChartManagerService.AddDataSet(
                    ChartGuid,
                    SeriesListComponent.SeriesList[i].ModifiedData ?? SeriesListComponent.SeriesList[i].OriginalData,
                    ViewportComponent.AxisMode,
                    i == SeriesListComponent.SeriesList.Count - 1);
            }

            Model.Viewport = await ChartManagerService.GetViewportParameters(ChartGuid);
            await ViewportComponent.RefreshParameters();

            await SendToAllan();
            await InvokeAsync(StateHasChanged);
        } 

        public override async Task SetViewport(ViewportParams<double> viewport)
        {            
            await ChartManagerService.SetViewportParameters(ChartGuid, viewport);
        }

        protected override async void ChartManagerService_OnViewportChanged(object sender, EventArgs e)
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

            base.Model.Viewport = eventArgs.Viewport;
            
            await SendToAllan();
            StateHasChanged();
        }

        public override async Task SendToAllan()
        {
            await AllanVariance.ClearChart();

            var allanData = new List<DataPoint<double>>();
            foreach (var dp in SelectedSeries.OriginalData.Where(dp => dp.X >= Model.Viewport.MinX && dp.X <= Model.Viewport.MaxX))
            {
                allanData.Add(new DataPoint<double>(dp.X, dp.Y));
            }

            await AllanVariance.LoadData(allanData, "Allan deviation");
        }

        public async void OnAxisModeChanged(object sender, AxisMode axisMode)
        {
            await Redraw();
        }
    }
}

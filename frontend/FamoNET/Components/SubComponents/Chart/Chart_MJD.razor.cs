using FamoNET.Model;
using FamoNET.Model.Args;
using FamoNET.Services;

namespace FamoNET.Components.SubComponents.Chart
{
    public partial class Chart_MJD : ChartWithAllanComponentBase<double>
    {              
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

        public override async Task LoadData(List<DataPoint<double>> data, string label)
        {                        
            await SeriesListComponent.AddSeries(label, data);                        
        }

        public async Task Redraw()
        {
            IsDataLoading = true;
            StateHasChanged();

            await ChartManagerService.ClearDataSets(ChartGuid, false);
            for (int i = 0; i < SeriesListComponent.SeriesList.Count; ++i)
            {
                await ChartManagerService.AddDataSet(
                    ChartGuid,
                    SeriesListComponent.SeriesList[i].ModifiedData ?? SeriesListComponent.SeriesList[i].OriginalData,
                    ViewportComponent.AxisMode,
                    i == SeriesListComponent.SeriesList.Count - 1);
            }

            if (Model.Viewport == null)
            {
                Model.Viewport = await ChartManagerService.GetViewportParameters(ChartGuid);
            }
            else
            {
                await ChartManagerService.SetViewportParameters(ChartGuid, Model.Viewport);
            }
            
            await ViewportComponent.RefreshParameters();

            IsDataLoading = false;
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
            if (!EnableAllan)
                return;

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

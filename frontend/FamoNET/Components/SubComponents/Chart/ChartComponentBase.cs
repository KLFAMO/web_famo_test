using FamoNET.Model;
using FamoNET.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using NLog;

namespace FamoNET.Components.SubComponents.Chart
{
    public abstract class ChartComponentBase<T> : ComponentBase, IAsyncDisposable
    {
        [Parameter]
        public bool IsEditable { get; set; } = true;

        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private bool _isInitialized = false;
        protected List<DataPoint<T>> OriginalCollection;

        [Inject]
        protected ChartManagerService ChartManagerService { get; set; }
        public ChartParameters<T> Model { get; set; }
        protected Guid ChartGuid { get; } = Guid.NewGuid();
                
        public bool IsDataLoaded { get; set; }        
        protected virtual async Task Initialize(ChartParameters<T> chartParameters)
        {
            if (_isInitialized)
                return;            

            await ChartManagerService.InitializeChart(ChartGuid, chartParameters);
            _isInitialized = true;
        }
        protected override async Task OnParametersSetAsync()                        
        {
            await base.OnParametersSetAsync();
            ChartManagerService.OnViewportChanged += ChartManagerService_OnViewportChanged;            
        }                

        public async Task ClearChart(bool instantRender = true)
        {            
            await ChartManagerService.ClearDataSets(ChartGuid, instantRender);
            //await ChartManagerService.ResetViewport(ChartGuid);
            IsDataLoaded = false;            
        }

        public async ValueTask DisposeAsync()
        {
            Logger.Debug($"Disposing {ChartGuid}");
            await ChartManagerService.DisposeChart(ChartGuid);
        }

        public virtual async Task UpdateParameters()
        {
            if (Model == null)
                return;

            await ChartManagerService.SetChartParameters(ChartGuid, new ChartParameters<T>(Model), false);
            await ChartManagerService.SetViewportParameters(ChartGuid, Model.Viewport);
        }

        public async Task OnChartClick(MouseEventArgs e)
        {
            var previousVP = await ChartManagerService.PopPreviousViewport(ChartGuid);
            if (previousVP == null)
            {
                await ChartManagerService.ResetViewport(ChartGuid);
            }
            else
            {                
                if (previousVP.AxisMode == AxisMode.Date)
                {
                    var dateVP = new ViewportParams<DateTime>()
                    {
                        MinX = DateTimeOffset.FromUnixTimeMilliseconds((long)previousVP.MinX).UtcDateTime,
                        MaxX = DateTimeOffset.FromUnixTimeMilliseconds((long)previousVP.MaxX).UtcDateTime,
                        MinY = previousVP.MinY,
                        MaxY = previousVP.MaxY,
                        AxisMode = AxisMode.Date };
                    await ChartManagerService.SetViewportParameters(ChartGuid, dateVP);
                }
                else
                {
                    await ChartManagerService.SetViewportParameters(ChartGuid, previousVP);
                }               
            }
        }

        public abstract Task LoadData(List<DataPoint<double>> data);
        public abstract Task SetViewport(ViewportParams<double> viewport);
        protected abstract void ChartManagerService_OnViewportChanged(object sender, EventArgs e);        
    }
}

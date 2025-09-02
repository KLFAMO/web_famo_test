using FamoNET.Components.SubComponents;
using FamoNET.Components.SubComponents.Chart;
using FamoNET.Model;
using FamoNET.Services;
using FamoNET.Services.DataServices;
using Microsoft.AspNetCore.Components;
using NLog;

namespace FamoNET.Components.Pages
{
    public partial class LiveBrowser : ComponentBase, IAsyncDisposable
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        [Inject]
        private AndaDataService _andaDataService { get; set; }
        protected Chart_MJD Chart_MJD { get; set; }
        protected AllanVariance AllanVarianceComponent { get; set; }
        protected int Points { get; set; } = 300;
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        public List<string> TableNames { get; set; } = new List<string>();     
        public string SelectedTableName { get; set; }

        private List<DataPoint<double>> AllanCollection = new List<Model.DataPoint<double>>();

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            if (firstRender)
            {
                TableNames = await _andaDataService.GetTableNamesAsync();
                StateHasChanged();
            }
        }

        protected async Task StartLive()
        {
            try
            {
                AllanCollection.Clear();
                await _cancellationTokenSource?.CancelAsync();
                _cancellationTokenSource = new CancellationTokenSource();
                string tableName = SelectedTableName;

                Chart_MJD.Model.Title = "Live";
                await Chart_MJD.UpdateParameters();

                while (!_cancellationTokenSource.IsCancellationRequested)
                {
                    var utcNow = DateTime.UtcNow;
                    var mjdNow = TimeService.GetMJD(utcNow);
                    var mjdPoints = TimeService.GetMJD(utcNow.AddSeconds(Points * (-1)));

                    var data = await _andaDataService.GetDataAsync(mjdPoints, mjdNow, tableName);
                    AddToAllanCollection(data);

                    await Chart_MJD.ClearChart(false);
                    await Chart_MJD.LoadData(data);

                    await AllanVarianceComponent.ClearChart();
                    await AllanVarianceComponent.LoadData(AllanCollection);
                   
                    await Task.Delay(2000);
                }
            }
            catch (Exception ex)
            {
                if (ex is TaskCanceledException)
                    return;

                _logger.Error(ex);
            }
        }
       
        private void AddToAllanCollection(List<DataPoint<double>> dataPoints)
        {
            if (AllanCollection.Count == 0)
            {
                AllanCollection.AddRange(dataPoints);
            }
            else
            {
                var lastMjd = AllanCollection.Last().X;
                var indexInReceivedCollection = dataPoints.IndexOf(dataPoints.Last(dp => dp.X == lastMjd));
                
                if (indexInReceivedCollection == -1)
                {
                    AllanCollection.AddRange(dataPoints);
                }
                else
                {
                    AllanCollection.AddRange(dataPoints.GetRange(indexInReceivedCollection, dataPoints.Count - indexInReceivedCollection));
                }
                
            }
        }
        public async ValueTask DisposeAsync()
        {
            await _cancellationTokenSource?.CancelAsync();
        }
    }
}

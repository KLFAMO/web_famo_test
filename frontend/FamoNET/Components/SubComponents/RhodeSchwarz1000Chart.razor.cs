using FamoNET.Model;
using FamoNET.Model.Interfaces;
using FamoNET.Services;
using Microsoft.AspNetCore.Components;
using System.Net;

namespace FamoNET.Components.SubComponents
{
    public partial class RhodeSchwarz1000Chart : ComponentBase, IAsyncDisposable
    {
        [Inject]
        private ChartManagerService _chartManagerService { get; set; }
        [Inject]
        private IFC1000Controller _fcController { get; set; }

        private ViewportParams<double> _viewportParams { get; set; }

        protected readonly Guid ChartGuid = Guid.NewGuid();
        public string IP { get; set; }        
        public int Port { get; set; }                
        public bool FailedToConnect { get; set; }
        public bool IsInitialized { get; set; }      
        private SAModel Model { get; set; } = new SAModel();

        public int SelectedRBWUnit { get; set; } = 1;
        public int SelectedCenterFrequencyUnit { get; set; } = 1;
        public int SelectedSpanUnit { get; set; } = 1;
        public double NewRBW { get; set; } = -1;
        public double NewCenterFrequency { get; set; } = -1;
        public double NewSpan { get; set; } = -1;

        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public async Task SetDeviceParameters()
        {
            await _fcController.SetFrequencySpan(NewSpan*SelectedSpanUnit);
            await _fcController.SetCenterFrequency(NewCenterFrequency * SelectedCenterFrequencyUnit);
            await _fcController.SetBandwidth(NewRBW*SelectedRBWUnit);
        }

        private async Task Initialize()
        {
            if (!IsInitialized)
            {
                await _chartManagerService.InitializeChart(ChartGuid, new ChartParameters<double>() { Title="", DisableXLabels = true, DisableEvents = true, InvertYAxis = false, AxisMode = AxisMode.Mjd });
            }

            await _chartManagerService.SetChartParameters(ChartGuid, new ChartParameters<double>()
            {
                Title = $"Spectrum Analyzer [{IP}]"
            });

            if (!IPAddress.TryParse(IP, out var ip))
            {
                FailedToConnect = true;                
                await _chartManagerService.SetChartParameters(ChartGuid, new ChartParameters<double> { Title = "Wrong IP" });
                await InvokeAsync(StateHasChanged);
                return;
            }

            IsInitialized = true;            
            _fcController.Initialize(IP, Port);
            _ = RefreshData();            
        }

        private async Task RefreshData()
        {
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                try
                {
                    Model.Data.Clear();
                    var (frequencies, rbw, center, span) = await _fcController.GetData().ConfigureAwait(false);
                    var points = new List<DataPoint<double>>();
                    for (int i = 0; i < frequencies.Count; i++)
                    {
                        points.Add(new DataPoint<double>((center-span)+(i*rbw), frequencies[i]));
                    }

                    Model.RBW = rbw;
                    Model.Span = span;
                    Model.CenterFrequency = center;

                    if (NewRBW == -1)
                        NewRBW = rbw;

                    if (NewSpan == -1)
                        NewSpan = span;

                    if (NewCenterFrequency == -1)
                        NewCenterFrequency = center;

                    if (_viewportParams == null || frequencies.Min() < _viewportParams.MinY || frequencies.Max() > _viewportParams.MaxY)
                    {
                        CalculateView(frequencies);
                    }

                    await _chartManagerService.ClearDataSets(ChartGuid, false).ConfigureAwait(false);
                    await _chartManagerService.AddDataSet(ChartGuid, points, false).ConfigureAwait(false);
                    await _chartManagerService.SetViewportParameters(ChartGuid, _viewportParams).ConfigureAwait(false);
                    FailedToConnect = false;
                    await InvokeAsync(StateHasChanged).ConfigureAwait(false);
                }
                catch(Exception ex)
                {
                    FailedToConnect = true;
                    await _chartManagerService.ClearDataSets(ChartGuid).ConfigureAwait(false);
                    await _chartManagerService.SetChartParameters(ChartGuid, new ChartParameters<double> { Title = "Failed to connect" }).ConfigureAwait(false);
                    _viewportParams = null;
                    await InvokeAsync(StateHasChanged).ConfigureAwait(false);
                }

                await Task.Delay(1000).ConfigureAwait(false);
            }
        }
        
        private void CalculateView(List<double> frequencies)
        {
            var offset = (frequencies.Max() - frequencies.Min()) * 0.2;

            _viewportParams = new ViewportParams<double>();
            _viewportParams.MinX = Model.CenterFrequency-Model.Span;
            _viewportParams.MaxX = Model.CenterFrequency+Model.Span;

            _viewportParams.MinY = frequencies.Min() - offset;
            _viewportParams.MaxY = frequencies.Max() + offset;
        }

        public async ValueTask DisposeAsync()
        {
            await _cancellationTokenSource.CancelAsync();
            await _chartManagerService.DisposeChart(ChartGuid);            
        }        
    }
}

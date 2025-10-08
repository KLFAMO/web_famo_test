using FamoNET.Model;
using FamoNET.Model.Args;
using FamoNET.Model.Interfaces;
using FamoNET.Services;
using Microsoft.AspNetCore.Components;
using System.Net;
using System.Transactions;

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
                
        public bool IsInitialized { get; set; }      
        private SAModel Model { get; set; } = new SAModel();
        private bool IsLoading { get; set; } = false;

        public int SelectedCenterFrequencyUnit { get; set; } = 1;
        public int SelectedSpanUnit { get; set; } = 1;

        public double NewRBW { get; set; } = -1;
        public double NewCenterFrequency { get; set; } = -1;
        public double NewSpan { get; set; } = -1;
        public double NewVBW { get; set; } = -1;

        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public async Task SetDeviceParameters()
        {
            await _fcController.SetParameters(new SpectrumAnalyzerParameters()
            {
                CenterFrequency = NewCenterFrequency * SelectedCenterFrequencyUnit,
                RBW = NewRBW,
                VBW = NewVBW,
                Span = NewSpan * SelectedSpanUnit
            });            
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
                await _chartManagerService.SetChartParameters(ChartGuid, new ChartParameters<double> { Title = "Wrong IP" });
                await InvokeAsync(StateHasChanged);
                return;
            }

            IsLoading = true;
            IsInitialized = true;
            _fcController.DataReceived += OnDataReceived;
            _fcController.Initialize(IP, Port);
        }

        private async void OnDataReceived(object sender, SpectrumAnalyzerEventArgs spectrumAnalyzerArgs)
        {
            IsLoading = false;
            var spectrumAnalyzerParameters = spectrumAnalyzerArgs.Parameters;

            try
            {
                await _chartManagerService.SetChartParameters(ChartGuid, new ChartParameters<double> { Title = $"Spectrum Analyzer [{IP}]" }).ConfigureAwait(false);
                Model.Data.Clear();

                var points = new List<DataPoint<double>>();
                for (int i = 0; i < spectrumAnalyzerParameters.Frequencies.Count; i++)
                {
                    points.Add(new DataPoint<double>((spectrumAnalyzerParameters.CenterFrequency - spectrumAnalyzerParameters.Span) + (i * spectrumAnalyzerParameters.RBW), spectrumAnalyzerParameters.Frequencies[i]));
                }

                Model.RBW = spectrumAnalyzerParameters.RBW;
                Model.Span = spectrumAnalyzerParameters.Span;
                Model.CenterFrequency = spectrumAnalyzerParameters.CenterFrequency;
                Model.VBW = spectrumAnalyzerParameters.VBW;

                if (NewRBW == -1)
                    NewRBW = spectrumAnalyzerParameters.RBW;

                if (NewVBW == -1)
                    NewVBW = spectrumAnalyzerParameters.VBW;

                if (NewSpan == -1)
                    NewSpan = spectrumAnalyzerParameters.Span;

                if (NewCenterFrequency == -1)
                    NewCenterFrequency = spectrumAnalyzerParameters.CenterFrequency;

                if (_viewportParams == null || spectrumAnalyzerParameters.Frequencies.Min() < _viewportParams.MinY || spectrumAnalyzerParameters.Frequencies.Max() > _viewportParams.MaxY)
                {
                    CalculateView(spectrumAnalyzerParameters.Frequencies);
                }

                await _chartManagerService.ClearDataSets(ChartGuid, false).ConfigureAwait(false);
                await _chartManagerService.AddDataSet(ChartGuid, points, false).ConfigureAwait(false);
                await _chartManagerService.SetViewportParameters(ChartGuid, _viewportParams).ConfigureAwait(false);

                IsLoading = false;
                await InvokeAsync(StateHasChanged).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await _chartManagerService.ClearDataSets(ChartGuid).ConfigureAwait(false);
                await _chartManagerService.SetChartParameters(ChartGuid, new ChartParameters<double> { Title = "Failed to parse data" }).ConfigureAwait(false);
                _viewportParams = null;
                await InvokeAsync(StateHasChanged).ConfigureAwait(false);
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

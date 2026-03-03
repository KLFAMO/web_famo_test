using FamoNET.Components.SubComponents.Wizards;
using FamoNET.Model;
using FamoNET.Model.Args;
using FamoNET.Model.Interfaces;
using FamoNET.Services;
using FamoNET.Services.DataServices;
using Microsoft.AspNetCore.Components;
using NLog;
using System.Net;

namespace FamoNET.Components.SubComponents
{
    public partial class SpectrumAnalyzerChart : ComponentBase, IAsyncDisposable
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        
        [Parameter]
        public EventCallback<Guid> ChartCloseRequested { get; set; }
        [Parameter]
        public Guid ComponentGuid { get; set; }
        [Inject]
        private ChartManagerService _chartManagerService { get; set; }
        [Inject]
        private RemoteDevicesDataService _remoteDevicesDataService { get; set; }
        [Inject]
        private RemoteChartsWriterService _writerService { get; set; }

        [Inject]
        private ISystemNotificationService _notificationService { get; set; }
        [Inject]
        private IDevicesDataService _devicesDataService { get; set; }
        [Parameter]
        public FileWizard FileWizard { get; set; }
        private FileManager FileManager;
        private ViewportParams<double> _viewportParams { get; set; }
        private List<Device> Devices { get; set; } = [];
        
        protected readonly Guid ChartGuid = Guid.NewGuid();
        private SpectrumAnalyzerParameters _lastData;
        private List<DataPoint<double>> _reference;
        public string DataFolder { get; set; }
        public string IP { get; set; }        
        public int? Port { get; set; }
        
        public bool IsFrozen { get; set; }        
        public bool IsInitialized { get; set; }      
        private SAModel Model { get; set; } = new SAModel();
        private bool IsLoading { get; set; } = true;

        public int SelectedCenterFrequencyUnit { get; set; } = 1;
        public int SelectedSpanUnit { get; set; } = 1;

        public double NewRBW { get; set; } = -1;
        public double NewCenterFrequency { get; set; } = -1;
        public double NewSpan { get; set; } = -1;
        public double NewVBW { get; set; } = -1;
        public int RefreshRate
        {
            get => _subscriptionInfo.Rate;
            set
            {
                _subscriptionInfo.Rate = value;
                _ = _remoteDevicesDataService.SetRefreshRate(_subscriptionInfo);
            }
        }

        public bool IsSettingParameters { get; private set; }
        public bool RefreshParametersUI { get; private set; }

        private RemoteChartsSubscriptionInfo _subscriptionInfo = new RemoteChartsSubscriptionInfo();
        private CancellationTokenSource _dataFetcherCancellationTokenSource;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            if (firstRender)
            {               
                await LoadData();
            }
        }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            _writerService.FileAdded += WriterService_FileAdded;
            
        }

        private async void WriterService_FileAdded(object sender, GenericEventArgs<string> e)
        {
            if (string.IsNullOrWhiteSpace(IP))
                return;

            try
            {
                if (e.Value.Contains(IP.Replace(".", "")))
                    await FileManager.Reload();
            }
            catch(Exception ex)
            {
                _logger.Error(ex);
                _notificationService.SendSystemMessage(this, new SystemMessage("Failed to reload file browser", SystemMessageType.Error));
            }
        }
        
        private async Task LoadData()
        {
            Devices = await _devicesDataService.GetDevicesAsync();
            IsLoading = false;
            await InvokeAsync(() => StateHasChanged());
        }       

        public async Task SetDeviceParameters()
        {
            try
            {
                if (NewCenterFrequency == double.NaN || NewRBW == double.NaN || NewVBW == double.NaN || NewSpan == double.NaN)
                {
                    _notificationService.SendSystemMessage(this, new SystemMessage("Invalid new values", SystemMessageType.Error));
                    return;
                }

                IsSettingParameters = true;
                StateHasChanged();

                await _remoteDevicesDataService.SetParameters(_subscriptionInfo, new SpectrumAnalyzerParameters()
                {
                    CenterFrequency = NewCenterFrequency * SelectedCenterFrequencyUnit,
                    RBW = NewRBW,
                    VBW = NewVBW,
                    Span = NewSpan * SelectedSpanUnit
                });
                
                IsSettingParameters = false;
                StateHasChanged();

                RefreshParametersUI = true;
            }
            catch(Exception ex)
            {
                _notificationService.SendSystemMessage(this, new SystemMessage("Failed to set parameters", SystemMessageType.Error));
                _logger.Error(ex);
            }
        }

        private void SetReference()
        {            
            _reference = SpectrumAnalyzerParametersToPoints(_lastData);
            StateHasChanged();
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
                _notificationService.SendSystemMessage(this, new SystemMessage("Wrong IP format", SystemMessageType.Error));                
                await InvokeAsync(StateHasChanged);
                return;
            }

            IsLoading = true;
            IsInitialized = true;

            _dataFetcherCancellationTokenSource = new CancellationTokenSource();            
            _subscriptionInfo.IP = IP;
            _subscriptionInfo.Port = Port.HasValue ? Port.Value : 5555;
            _subscriptionInfo.Rate = 2000;

            try
            {
                await _remoteDevicesDataService.Subscribe(_subscriptionInfo).ConfigureAwait(false);
                _ = Task.Run(DataFetcher);
            }
            catch(Exception ex)
            {
                if (ex is NotSupportedException)
                {
                    _notificationService.SendSystemMessage(this, new SystemMessage(ex.Message, SystemMessageType.Error));
                }
                else
                {
                    _logger.Error(ex);
                }

                _notificationService.SendSystemMessage(this, new SystemMessage("Unknown error", SystemMessageType.Error));
                IsLoading = false;
                IsInitialized = false;
            }
        }        

        private async Task DataFetcher()
        {
            _logger.Debug("Data fetcher started");
            try
            {                
                while(!_dataFetcherCancellationTokenSource.IsCancellationRequested)
                {
                    if (!IsFrozen)
                    {
                        _lastData = await _remoteDevicesDataService.GetData(_subscriptionInfo).ConfigureAwait(false);
                        await HandleData(_lastData).ConfigureAwait(false);
                    }
                    
                    _remoteDevicesDataService.Ping(_subscriptionInfo);
                    await Task.Delay(RefreshRate).ConfigureAwait(false);                                                            
                }
            }
            catch(Exception ex)
            {
                if (ex is TaskCanceledException)
                    return;

                _logger.Error(ex);
            }            
        }

        private void SaveData()
        {
            if (string.IsNullOrWhiteSpace(IP))
                return;

            _writerService.RaiseRequestWizard(Path.Combine(Directory.GetCurrentDirectory(), "Data", "RemoteCharts", IP.Replace(".", "")), SpectrumAnalyzerParametersToPoints(_lastData));
        }

        private List<DataPoint<double>> SpectrumAnalyzerParametersToPoints(SpectrumAnalyzerParameters spectrumAnalyzerParameters)
        {
            var points = new List<DataPoint<double>>();
            var step = 2 * spectrumAnalyzerParameters.Span / (spectrumAnalyzerParameters.Frequencies.Count - 1);
            var minFreq = spectrumAnalyzerParameters.CenterFrequency - spectrumAnalyzerParameters.Span;

            for (int i = 0; i < spectrumAnalyzerParameters.Frequencies.Count; i++)
            {
                points.Add(new DataPoint<double>((minFreq) + (i * step), spectrumAnalyzerParameters.Frequencies[i]));
            }

            return points;
        }

        private async Task HandleData(SpectrumAnalyzerParameters spectrumAnalyzerParameters)
        {            
            try
            {
                if (spectrumAnalyzerParameters == null || spectrumAnalyzerParameters.Frequencies == null || spectrumAnalyzerParameters.Frequencies.Count < 1)
                    return;

                await _chartManagerService.SetChartParameters(ChartGuid, new ChartParameters<double> { Title = $"Spectrum Analyzer [{IP}]" }).ConfigureAwait(false);
                Model.Data.Clear();

                var points = SpectrumAnalyzerParametersToPoints(spectrumAnalyzerParameters);

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


                AdjustView(points);

                await _chartManagerService.ClearDataSets(ChartGuid, false);
                await _chartManagerService.AddDataSet(ChartGuid, points, instantRender: false);
                if (_reference != null)
                {                    
                    await _chartManagerService.AddDataSet(ChartGuid, _reference, instantRender:false);
                }
                await _chartManagerService.SetViewportParameters(ChartGuid, _viewportParams).ConfigureAwait(false);

                if (IsLoading)
                {
                    IsLoading = false;
                    await InvokeAsync(StateHasChanged).ConfigureAwait(false);
                    await FileManager.Initialize(Path.Combine(Directory.GetCurrentDirectory(), "Data", "RemoteCharts", IP.Replace(".", "")));
                }

                if (RefreshParametersUI)
                {
                    RefreshParametersUI = false;
                    await InvokeAsync(StateHasChanged).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                try
                {
                    await _chartManagerService.ClearDataSets(ChartGuid).ConfigureAwait(false);
                }
                catch //Component dispose
                {
                    _logger.Trace("Tried to interact with chart, but page is disposed");
                    return;
                }

                _notificationService.SendSystemMessage(this, new SystemMessage("Failed to parse data", SystemMessageType.Error));
                _viewportParams = null;
                await InvokeAsync(StateHasChanged).ConfigureAwait(false);
                _logger.Error(ex);
            }
        }

        private void AdjustView(List<DataPoint<double>> points)
        {
            var minX = points.Select(p => p.X).Min();
            var maxX = points.Select(p => p.X).Max();

            var minY = points.Select(p => p.Y).Min();
            var maxY = points.Select(p => p.Y).Max();

            var offsetY = (maxY - minY) * 0.2;

            if (_viewportParams == null ||
                minX < _viewportParams.MinX || 
                maxX > _viewportParams.MaxX ||
                minY < _viewportParams.MinY ||
                maxY > _viewportParams.MaxY ||
                _viewportParams.MaxX > maxX*1.1 ||
                _viewportParams.MinX < minX*0.9 ||
                _viewportParams.MinY < minY-(5*offsetY) ||
                _viewportParams.MaxY > maxY+(5*offsetY))
            {
                

                _viewportParams = new ViewportParams<double>();
                _viewportParams.MinX = Model.CenterFrequency - Model.Span;
                _viewportParams.MaxX = Model.CenterFrequency + Model.Span;

                _viewportParams.MinY = minY - offsetY;
                _viewportParams.MaxY = maxY + offsetY;
            }
            
        }

        public async ValueTask DisposeAsync()
        {
            _dataFetcherCancellationTokenSource?.Cancel();
            await _remoteDevicesDataService?.Unsubscribe(_subscriptionInfo);                                        
            if (_chartManagerService != null)
            {
                await _chartManagerService.DisposeChart(ChartGuid);
            }            
        }        
    }
}

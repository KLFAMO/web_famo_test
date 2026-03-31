using FamoNET.Model;
using FamoNET.Model.Args;
using FamoNET.Model.Interfaces;
using FamoNET.Services;
using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;

namespace FamoNET.Components.SubComponents.Chart
{
    public partial class LiveChart : ChartWithAllanComponentBase<double>
    {        
        [Inject] 
        private IAndaDataProvider AndaDataProvider { get; set; } = null!;
        [Inject]
        private ISystemNotificationService SystemNotificationService { get; set; }
        [Parameter] 
        public string TableNameParam { get; set; } = String.Empty;

        [Parameter]
        public int Points { get; set; } = 100;

        private string _tableName = String.Empty;
        private List<DataPoint<double>>? CurrentSeries;
        
        private Task? _liveTask;
        private CancellationTokenSource? _cancellationTokenSource;
        
        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();
            
            if (string.Equals(TableNameParam, _tableName))
                return;

            Model = new ChartParameters<double>();
            _tableName = TableNameParam;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            if (firstRender)
            {
                await Initialize(new ChartParameters<double>() { Title = "Data overview", DisableXLabels = false, DisableEvents = false, AxisMode = AxisMode.Mjd });
            }
        }

        public void StartLive()
        {
            if (_liveTask != null)
            {
                Logger.Debug("Live task is already running. Stop first.");
                return;
            }

            _cancellationTokenSource = new CancellationTokenSource();
            _liveTask = Task.Run(LiveLoop, _cancellationTokenSource.Token);            
        }

        public void StopLive()
        {
            if (_liveTask == null)
            {
                Logger.Debug("There is no active live task");
                return;
            }
            _cancellationTokenSource?.Cancel();
            _liveTask.Dispose();
            
            _liveTask = null;
            _cancellationTokenSource = null;
        }

        private async Task LiveLoop()
        {
            try
            {
                var interval = await CheckSampligRateForTable();
                Logger.Trace($"Interval found: {interval}");
                
                while (!_cancellationTokenSource!.IsCancellationRequested)
                {
                    var dateNow = DateTime.UtcNow;
                    var mjdEnd = TimeService.GetMJD(dateNow.AddSeconds(-interval*Points));

                    //CurrentSeries = await AndaDataProvider.GetData(mjdEnd, TimeService.GetMJD(dateNow), _tableName);
                    var rand = new Random();
                    CurrentSeries = new List<DataPoint<double>>() 
                    { 
                        new DataPoint<double>(0, rand.Next()), 
                        new DataPoint<double>(1, rand.Next()), 
                        new DataPoint<double>(2, rand.Next()),
                        new DataPoint<double>(3, rand.Next()),
                        new DataPoint<double>(4, rand.Next()),
                        new DataPoint<double>(5, rand.Next()),
                        new DataPoint<double>(6, rand.Next()),
                        new DataPoint<double>(7, rand.Next()),
                        new DataPoint<double>(8, rand.Next()),
                        new DataPoint<double>(9, rand.Next()),
                        new DataPoint<double>(10, rand.Next()),
                        new DataPoint<double>(11, rand.Next()),
                    };
                    
                    await ChartManagerService.ClearDataSets(ChartGuid, false);
                    await ChartManagerService.AddDataSet(ChartGuid, CurrentSeries);

                    Model.Viewport = await ChartManagerService.GetViewportParameters(ChartGuid);

                    await SendToAllan();                    
                    await InvokeAsync(StateHasChanged);
                    
                    await Task.Delay(2000, _cancellationTokenSource.Token);                                  
                }
            }
            catch (Exception ex)
            {
                if (ex is TaskCanceledException)
                    return;

                SystemNotificationService.SendSystemMessage(this, new SystemMessage(ex.Message, SystemMessageType.Error));
                Logger.Error(ex);
            }
        }

        private async Task<double> CheckSampligRateForTable()
        {
            bool intervalFound = false;
            double interval = -1;

            var nowDate = DateTime.UtcNow;          
            int offset = 7; //in seconds

            while (!intervalFound)
            {
                var mjdEnd = TimeService.GetMJD(nowDate.AddSeconds(-offset));
                var data = await AndaDataProvider.GetData(mjdEnd, TimeService.GetMJD(nowDate), _tableName);

                if (data.Count < 2)
                {
                    offset *= 10;
                    if (offset > 604800) //larger than week of seconds
                        throw new InvalidDataException("Interval for data not found. There is not data or interval is longer than a week.");

                    continue;
                }
                interval = data[1].X - data[0].X;
                intervalFound = true;                                
            }

            return interval;            
        }

        public override Task LoadData(List<DataPoint<double>> data, string title)
        {
            throw new NotImplementedException();
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

            Model.Viewport = eventArgs.Viewport;
            
            await SendToAllan();
            StateHasChanged();
        }

        public override async Task SendToAllan()
        {
            if (!EnableAllan || CurrentSeries == null)
                return;

            await AllanVariance.ClearChart();

            var allanData = new List<DataPoint<double>>();
            foreach (var dp in CurrentSeries.Where(dp => dp.X >= Model.Viewport.MinX && dp.X <= Model.Viewport.MaxX))
            {
                allanData.Add(new DataPoint<double>(dp.X, dp.Y));
            }

            await AllanVariance.LoadData(allanData, "Allan deviation");
        }
    }
}

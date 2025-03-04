using FamoNET.Model;
using FamoNET.Services;
using FamoNET.Services.DataServices;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using NLog;

namespace FamoNET.Components.SubComponents
{
    public partial class AllanVariance : ComponentBase
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private double _allanTime = 0;
        private bool _allanPlotting = false;
        private List<double> _times = new List<double>();
        private List<double> _frequency = new List<double>();

        public bool PlotAllan
        {
            get => _allanPlotting;
            set 
            {
                if (value == _allanPlotting) 
                    return;

                _allanPlotting = value;

                if (value)
                {
                    _allanTime = 0;
                    _times.Clear();
                    _frequency.Clear();
                    CounterDataService.ChannelsReceived += CounterDataService_ChannelsReceived;
                }
                    
                else
                    CounterDataService.ChannelsReceived -= CounterDataService_ChannelsReceived;
            }
        }

        [Parameter]
        public CounterDataService CounterDataService { get; set; }
        [Inject]
        private ChartManagerService _chartManagerService { get; set; }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await _chartManagerService.InitializeService<AllanVariance>(DotNetObjectReference.Create(this));
                await _chartManagerService.InitializeChart("Allan deviation", true);
            }

            await base.OnAfterRenderAsync(firstRender);
        }
        
        private void CounterDataService_ChannelsReceived(object sender, Model.Args.ChannelsReceivedEventArgs e)
        {            
            try
            {
                if (_allanPlotting)
                {
                    _times.Add(_allanTime);
                    _frequency.Add(e.Channels[0].Frequency);
                    _allanTime += 0.01;

                    Task.Run(CalculateAllan);
                }                                             
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        private async Task CalculateAllan()
        {
            if (_times.Count < 2 || _frequency.Count < 2)
                return;

            var dt = 0.01;
            var data = new List<DataPoint>();
            
            for(double tau = 0.1; tau < _frequency.Count/2; tau*=1.5)
            {
                var n = (int)(tau / dt);                
                var M = (int)(_frequency.Count / n);
                if (M < 2)
                    continue;

                
                var y = new List<double>();
                for (int i=0; i < M-1; ++i)
                {                    
                    //ySum += Math.Pow(frequency.Slice((int)(i * n), n).Average(), 2);                    
                    y.Add(_frequency.Slice((int)(i*n), n).Average());
                }

                var ySum = 0.0;
                for (int i=0; i<y.Count-1; ++i)
                {
                    ySum += Math.Pow((y[i + 1] - y[i]), 2);
                }
                
                data.Add(new DataPoint(tau, Math.Sqrt((1.0 / (2.0 * (M - 1.0))) * ySum)));
            }            
                                                

            await _chartManagerService.ClearDataSets(false);
            await _chartManagerService.AddDataSet(data);            
        }        
    }
}

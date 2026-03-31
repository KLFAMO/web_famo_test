using FamoNET.Components.SubComponents.Chart;
using FamoNET.Model;
using FamoNET.Utils;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using NLog;

namespace FamoNET.Components.SubComponents
{
    public partial class AllanVariance : ChartComponentBase<double>
    {        
        [Inject]
        private IJSRuntime _jSRuntime { get; set; }
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private List<DataPoint<double>> _data;
        private List<DataPoint<double>> _allanData;
        private int _tauMode = (int)AllanTauMode.Decade;
        private int _allanType = (int)FamoNET.Model.AllanType.Normal;

        public int TauMode 
        { 
            get => _tauMode; 
            set 
            {
                if (value == _tauMode) return;

                _tauMode = value;
                
                _ = LoadData(_data, "Allan deviation");
            }
        }
        public int AllanType 
        {
            get => _allanType;
            set
            {
                if (value ==  _allanType) return;
                
                _allanType = value;
                _ = LoadData(_data, "Allan deviation");
            }
        }

        public bool IsLoading { get; set; } = false;
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            if (firstRender)
            {
                _logger.Debug("Allan:" + ChartGuid);
                await Initialize(new ChartParameters<double>() { Title = "Allan deviation", Logarithmic = true, AxisMode=AxisMode.Mjd, DisableEvents = false });

                await InvokeAsync(StateHasChanged);
                await Task.Yield();
            }            
        }

        public override async Task LoadData(List<DataPoint<double>> data, string title)
        {
            if (data == null || data.Count < 2)
                return;

            _data = data;
            
            IsLoading = true;
            await InvokeAsync(StateHasChanged);
            await Task.Yield();

            List<DataPoint<double>> allanData = null;
            double tau0 = (FamoMath.Convert_MJDToDateTime(data[1].X) - FamoMath.Convert_MJDToDateTime(data[0].X)).TotalSeconds;
            switch(AllanType)
            {
                case (int)FamoNET.Model.AllanType.Modified:
                    allanData = FamoMath.ModifiedAllan(data.Select(d => d.Y).ToList(), tau0, (AllanTauMode)TauMode);
                    break;

                case (int)FamoNET.Model.AllanType.Overlapping:
                    allanData = FamoMath.OverlappingAllan(data.Select(d => d.Y).ToList(), tau0, (AllanTauMode)TauMode);
                    break;
                                
                default:
                    allanData = FamoMath.Allan(data.Select(d => d.Y).ToList(), tau0, (AllanTauMode)TauMode);
                    break;
            }
            
            try
            {
                await ChartManagerService.ClearDataSets(ChartGuid, false);
            }
            catch(Microsoft.JSInterop.JSException)
            {
                //expected on init
            }
            
            _allanData = allanData;
            if (_allanData.Count < 1)
            {
                IsLoading = false;
                await InvokeAsync(StateHasChanged);
                return;
            }
                
            await ChartManagerService.AddDataSet(ChartGuid, allanData);

            try
            {
                await ChartManagerService.AdjustToVisible(ChartGuid);
            }
            catch (Microsoft.JSInterop.JSException)
            {
                //expected on init
            }

            IsLoading = false;
            await InvokeAsync(StateHasChanged);
        }

        

        public override Task SetViewport(ViewportParams<double> viewport)
        {
            throw new NotSupportedException();
        }

        public async Task SaveToFile()
        {
            var ms = new MemoryStream();
            using var sw = new StreamWriter(ms);
            foreach(var data in _allanData)
            {
                sw.WriteLine($"{data.X} {data.Y}");
            }
            await sw.FlushAsync();
            ms.Position = 0;

            using var streamRef = new DotNetStreamReference(stream: ms);

            await _jSRuntime.InvokeVoidAsync("downloadFileFromStream", "allan.txt", streamRef);
        }

        protected override void ChartManagerService_OnViewportChanged(object sender, EventArgs e)
        {
            return;
        }
    }
}

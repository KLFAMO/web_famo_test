using FamoNET.Components.SubComponents.Chart;
using FamoNET.Model;
using FamoNET.Utils;
using NLog;

namespace FamoNET.Components.SubComponents
{
    public partial class AllanVariance : ChartComponentBase<double>
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private List<DataPoint<double>> _data;
        private int _tauMode = (int)AllanTauMode.Decade;
        private int _allanType = (int)FamoNET.Model.AllanType.Normal;

        public int TauMode 
        { 
            get => _tauMode; 
            set 
            {
                if (value == _tauMode) return;

                _tauMode = value;
                
                _ = LoadData(_data);
            }
        }
        public int AllanType 
        {
            get => _allanType;
            set
            {
                if (value ==  _allanType) return;
                
                _allanType = value;
                _ = LoadData(_data);
            }
        }

        public bool IsLoading { get; set; } = false;
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            if (firstRender)
            {
                _logger.Debug("Allan:" + ChartGuid);
                await Initialize(new ChartParameters<double>() { Title = "Allan deviation", Logarithmic = true, AxisMode=AxisMode.Mjd, DisableEvents = true });
            }            
        }

        public override async Task LoadData(List<DataPoint<double>> data)
        {
            if (data == null)
                return;

            _data = data;
            
            IsLoading = true;
            await InvokeAsync(StateHasChanged);

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

            await ChartManagerService.AddDataSet(ChartGuid, allanData);

            try
            {
                await ChartManagerService.ResetViewport(ChartGuid);
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

        protected override void ChartManagerService_OnViewportChanged(object sender, EventArgs e)
        {
            //Events for chart are disabled
            return;
        }
    }
}

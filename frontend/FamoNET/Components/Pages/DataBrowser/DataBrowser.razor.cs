using FamoNET.Model;
using FamoNET.Model.Enums;
using FamoNET.Model.Interfaces;
using FamoNET.Services;
using FamoNET.Services.DataServices;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using NLog;

namespace FamoNET.Components.Pages.DataBrowser
{
    public partial class DataBrowser : ComponentBase
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private bool _isFetchingData = false;
        private bool _isDataLoaded = false;                
        private AxisMode _axisMode = AxisMode.Mjd;

        #region Injections        
        [Inject]
        private ChartManagerService _chartManagerService { get; set; }
        [Inject]
        private AndaDataService _counterDataService { get; set; }
        [Inject]
        private ICSVDataProvider _csvDataProvider { get; set; }
        #endregion

        #region Components
        private ChartWizard ChartWizardComponent { get; set; }        
        private PythonConsole PythonConsoleComponent { get; set; }
        private DataFetchWizard DataFetchWizardComponent { get; set; }
        #endregion

        #region Properties
        public bool IsFetchingData 
        { 
            get => _isFetchingData;
            set
            {
                if (_isFetchingData == value)
                    return;
                
                _isFetchingData = value;
                DataFetchWizardComponent.ToggleUI();

                StateHasChanged();
            } 
        }

        public bool IsDataLoaded
        {
            get => _isDataLoaded;
            set
            {
                if (_isDataLoaded == value)
                    return;

                _isDataLoaded = value;
                StateHasChanged();
            }
        }
        #endregion

        #region Handling inner component events
        private async Task OnChartParametersUpdate(ChartParams chartParams)
        {
            await _chartManagerService.SetChartParameters(chartParams);
        }

        private async Task OnPythonConsoleExecute(string cmd)
        {
            IsFetchingData = true;
            await _counterDataService.SendPythonAsync(cmd);
            IsFetchingData = false;
        }

        private async Task OnFetchRequested((decimal startMjd, decimal endMjd, string tableName) fetchParams)
        {
            IsFetchingData = true;
            var data = await _counterDataService.GetDataAsync(fetchParams.startMjd, fetchParams.endMjd, fetchParams.tableName);

            if (data == null)
            {
                IsFetchingData = false;
                await _chartManagerService.ClearDataSets(false);
                await _chartManagerService.SetChartParameters(new ChartParams() { Title = "Failed to retrieve data"});
                _logger.Info("Failed to fetch data");
                return;
            }

            if (data.Count == 0)
            {
                await _chartManagerService.ClearDataSets(false);
                await _chartManagerService.SetChartParameters(new ChartParams() { Title = "No data" });                
                IsFetchingData= false;
                return;
            }

            await _chartManagerService.ClearDataSets();
            await _chartManagerService.AddDataSet(data);            

            var chartParams = await _chartManagerService.GetViewportParameters();
            chartParams.Title = DataFetchWizardComponent.SelectedTableName;
            ChartWizardComponent.SetParams(chartParams);
            await _chartManagerService.SetChartParameters(chartParams);
            
            IsFetchingData = false;
            IsDataLoaded = true;
        }
        #endregion

        #region Overrides
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await _chartManagerService.InitializeService<DataBrowser>(DotNetObjectReference.Create(this));
                await _chartManagerService.InitializeChart("No data");
            }

            await base.OnAfterRenderAsync(firstRender);
        }
        #endregion
        
        protected async Task ClearChart()
        {
            await _chartManagerService.ClearDataSets();
            IsDataLoaded = false;
        }

        [JSInvokable]
        public async Task Unzoom() => await _chartManagerService.Unzoom();           

        protected async Task ToggleXLabels()
        {
            if (_axisMode == AxisMode.Mjd)
            {
                _axisMode = AxisMode.Date;
                await _chartManagerService.ConvertToDate();
            }
            else
            {
                _axisMode = AxisMode.Mjd;
                await _chartManagerService.ConvertToMjd();
            }            
        }          

        public async Task AdjustToVisible() => await _chartManagerService.AdjustToVisible();        

        public async Task LoadCSVData()
        {
            IsFetchingData = true;
            var data = await _csvDataProvider.LoadCSV();

            if (data == null)
            {
                IsFetchingData = false;
                _logger.Info("Failed to load data from CSV");
                return;
            }

            await _chartManagerService.AddDataSet(data);            
            ChartWizardComponent.SetParams(await _chartManagerService.GetViewportParameters());

            IsFetchingData = false;
        }     
    }
}

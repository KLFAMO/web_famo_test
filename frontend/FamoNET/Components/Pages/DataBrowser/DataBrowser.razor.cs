using FamoNET.Model;
using FamoNET.Model.Interfaces;
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
        private IJSObjectReference _module;        

        #region Injections
        [Inject]
        private IJSRuntime _jsRuntime { get;set; }
        [Inject]
        private CounterDataService _counterDataService { get; set; }
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
                StateHasChanged();
            } 
        }        
        #endregion

        #region Handling inner component events
        private async Task OnChartParametersUpdate(ChartParams chartParams)
        {
            await _module.InvokeVoidAsync("SetChartParameters", chartParams);
            await _module.InvokeVoidAsync("RenderChart");
        }

        private async Task OnPythonConsoleExecute(string cmd)
        {
            IsFetchingData = true;
            await _counterDataService.SendPythonAsync(cmd);
            IsFetchingData = false;
        }

        private async Task OnDataReceived(DataSet dataSet)
        {
            await _module.InvokeVoidAsync("AddDataSet", dataSet.Data);
            await _module.InvokeVoidAsync("RenderChart");

            var limits = await _module.InvokeAsync<List<decimal>>("GetChartParameters");

            if (limits != null)
                ChartWizardComponent.SetParams(new ChartParams() { MinX = limits[0], MaxX = limits[1], MinY = limits[2], MaxY = limits[3] });           
        }
        #endregion


        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                _module = await _jsRuntime.InvokeAsync<IJSObjectReference>("import","./Components/Pages/DataBrowser/DataBrowser.razor.js");
                await _module.InvokeAsync<string>("CreateEmptyChart", "");
            }

            await base.OnAfterRenderAsync(firstRender);            
        }

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

            await _module.InvokeVoidAsync("AddDataSet", data);
            await _module.InvokeVoidAsync("RenderChart");

            var limits = await _module.InvokeAsync<List<decimal>>("GetChartParameters");
            
            if (limits != null)
                ChartWizardComponent.SetParams(new ChartParams() { MinX = limits[0], MaxX = limits[1], MinY = limits[2], MaxY = limits[3] });

            IsFetchingData = false;
        }               
    }
}

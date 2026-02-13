using FamoNET.Components.SubComponents;
using FamoNET.Components.SubComponents.Chart;
using FamoNET.Components.SubComponents.Wizards;
using FamoNET.Model;
using FamoNET.Model.Interfaces;
using Microsoft.AspNetCore.Components;
using NLog;

namespace FamoNET.Components.Pages
{
    public partial class Browser : ComponentBase
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();                
        
        public AxisMode CurrentMode { get; set; }
        private bool _isFetchingData = false;
        private bool _isDataLoaded = false;
        [Inject]
        private ISystemNotificationService _systemNotificationService { get; set; }

        #region Components
        private Chart_MJD Chart_MJD;
        private Chart_Date Chart_Date;
        private Chart_Offset Chart_Offset;
        //private PythonConsole PythonConsoleComponent;
        private DataWizard DataFetchWizardComponent;        
        #endregion

        #region Properties               
        public bool IsFetchingData 
        {
            get => _isFetchingData;                
            set
            {
                if (value == _isFetchingData)
                    return;
                _isFetchingData = value;
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

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            if (firstRender)
            {                
                DataFetchWizardComponent.DataAvailable += DataWizardComponent_DataAvailable;
                DataFetchWizardComponent.DataFetching += (s,e) => IsFetchingData = true;
            }
        }

        private async void DataWizardComponent_DataAvailable(object sender, List<DataPoint<double>> e)
        {            
            if (e == null || e.Count < 1)
            {
                _systemNotificationService.SendSystemMessage(this, new SystemMessage("Invalid data or empty data", SystemMessageType.Error));
                return;
            }
            else
            {                
                await Chart_MJD.LoadData(e, DataFetchWizardComponent.SelectedTableName);
                //await Chart_Date.LoadData(e, DataFetchWizardComponent.SelectedTableName);
                //await Chart_Offset.LoadData(e, DataFetchWizardComponent.SelectedTableName);
            }                
            
            IsFetchingData = false;
            IsDataLoaded = true;
            DataFetchWizardComponent.IsUIDisabled = false;
            StateHasChanged();
        }

        protected async Task SwitchMode(AxisMode mode)
        {
            CurrentMode = mode;
            StateHasChanged();

            switch (mode)
            {
                case AxisMode.Mjd:
                    await Chart_MJD.UpdateParameters();
                    break;

                case AxisMode.Date:
                    await Chart_Date.UpdateParameters();
                    break;

                case AxisMode.Offset:
                    await Chart_Offset.UpdateParameters();
                    break;
            }

        }
    }
}

using FamoNET.Components.SubComponents;
using FamoNET.Components.SubComponents.Chart;
using FamoNET.Model;
using Microsoft.AspNetCore.Components;
using NLog;

namespace FamoNET.Components.Pages
{
    public partial class Browser : ComponentBase
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private bool _isFetchingData = false;
        private bool _isDataLoaded = false;
        
        public AxisMode CurrentMode { get; set; }

        #region Components
        private Chart_MJD Chart_MJD { get; set; }
        private Chart_Date Chart_Date { get; set; }
        private Chart_Offset Chart_Offset { get; set; }        
        private PythonConsole PythonConsoleComponent { get; set; }
        private DataWizard DataFetchWizardComponent { get; set; }
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
        #endregion

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            if (firstRender)
            {                
                DataFetchWizardComponent.DataAvailable += DataWizardComponent_DataAvailable;
            }
        }

        private async void DataWizardComponent_DataAvailable(object sender, List<DataPoint<double>> e)
        {
            await Chart_MJD.LoadData(e);
            await Chart_Date.LoadData(e);
            await Chart_Offset.LoadData(e);
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

using FamoNET.Model;
using FamoNET.Model.Interfaces;
using FamoNET.Services;
using FamoNET.Services.DataServices;
using FamoNET.Utils;
using Microsoft.AspNetCore.Components;
using NLog;

namespace FamoNET.Components.SubComponents.Chart
{
    public partial class DataWizard : ComponentBase
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private List<string> _tableNames = new List<string>();
        private string _selectedTableName;

        [Inject]
        private ISystemNotificationService _notificationService { get; set; }
        [Inject]
        private AndaDataService _andaDataService { get; set; }                

        public event EventHandler<List<DataPoint<double>>> DataAvailable;
        public event EventHandler DataFetching;

        public double StartMjd { get; set; } = Math.Round(TimeService.GetMJD(DateTime.UtcNow.AddDays(-1)),4);
        public double EndMjd { get; set; } = Math.Round(TimeService.GetMJD(DateTime.UtcNow), 4);


        #region Properties
        public List<string> TableNames 
        {
            get => _tableNames;
            set
            {
                if (_tableNames == value)
                    return;
                _tableNames = value;
                StateHasChanged();
            }
        }
          
        public string SelectedTableName
        {
            get => _selectedTableName;
            set
            {
                if (value == _selectedTableName)
                    return;

                _selectedTableName = value;                
            }
        }
        #endregion
               
        protected async override Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            if (firstRender)
            {                                
                TableNames = await _andaDataService.GetTableNamesAsync();
            }
        }
        
        protected async Task FetchData()
        {
            if (_selectedTableName == null)
            {
                _notificationService.SendSystemMessage(this, new SystemMessage("Table not selected", SystemMessageType.Error));
                return;
            }
            try
            {
                DataFetching.Invoke(this, EventArgs.Empty);
                var data = await _andaDataService.GetDataAsync(StartMjd, EndMjd, _selectedTableName);
                if (data == null || data.Count < 1)
                {
                    _notificationService.SendSystemMessage(this, new SystemMessage("Empty dataset", SystemMessageType.Error));
                    return;
                }

                DataAvailable?.Invoke(this, data ?? new List<DataPoint<double>>());
            }   
            catch(Exception ex)
            {
                _logger.Error(ex);
                _notificationService.SendSystemMessage(this, new SystemMessage("Failed to fetch data.", SystemMessageType.Error));
            }            
        }
    }
}

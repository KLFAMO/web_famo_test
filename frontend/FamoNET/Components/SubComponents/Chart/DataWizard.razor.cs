using FamoNET.Model;
using FamoNET.Services;
using FamoNET.Services.DataServices;
using Microsoft.AspNetCore.Components;
using NLog;

namespace FamoNET.Components.SubComponents.Chart
{
    public partial class DataWizard : ComponentBase
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private List<string> _tableNames = new List<string>();
        private string _selectedTableName;
        private double _startMjd;
        private double _endMjd;        

        [Inject]
        private AndaDataService _andaDataService { get; set; }                

        public event EventHandler<List<DataPoint<double>>> DataAvailable;

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
        public double StartMjd
        {
            get => _startMjd;
            set
            {
                if ( _startMjd == value)
                    return;

                _startMjd = value;
                StateHasChanged();
            }
        }
        public double EndMjd
        {
            get => _endMjd;
            set
            {
                if (value == _endMjd)
                    return;

                _endMjd = value;
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
                var mjd_now = Math.Floor(TimeService.GetMJD(DateTime.UtcNow));
                _startMjd = mjd_now-1;
                _endMjd = mjd_now;
                StateHasChanged();

                TableNames = await _andaDataService.GetTableNamesAsync();
            }
        }
        
        protected async Task FetchData()
        {
            if (_selectedTableName == null)
                return;

            ToggleUI();
            var data = await _andaDataService.GetDataAsync(_startMjd, _endMjd, _selectedTableName);
            ToggleUI();
            
            DataAvailable?.Invoke(this, data);           
        }
    }
}

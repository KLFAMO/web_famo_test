using FamoNET.Services;
using FamoNET.Services.DataServices;
using Microsoft.AspNetCore.Components;

namespace FamoNET.Components.Pages.DataBrowser
{
    public partial class DataFetchWizard : ComponentBase
    {
        private List<string> _tableNames;
        private string _selectedTableName;
        private decimal _startMjd;
        private decimal _endMjd;

        [Inject]
        private CounterDataService _counterDataService { get; set; }
        [Inject]
        private TimeService _timeService { get; set; }
        [Parameter]
        public EventCallback<(decimal,decimal,string)> FetchRequested { get; set; }
        
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
        public decimal StartMjd
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
        public decimal EndMjd
        {
            get => _endMjd;
            set
            {
                if ( value == _endMjd )
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
                var jdNumerical = TimeService.GetJulianDate(DateTime.UtcNow);
                var mjd_now = Math.Floor(jdNumerical - 2400000.5);
                _startMjd = Convert.ToDecimal(mjd_now - 1);
                _endMjd = Convert.ToDecimal(mjd_now);
                StateHasChanged();                              
            }
        }
        protected async Task FetchData()
        {
            if (_selectedTableName == null)
                return;
            await FetchRequested.InvokeAsync((StartMjd, EndMjd, SelectedTableName));
        }
    }
}

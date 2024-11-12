using FamoNET.Model;
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
        [Parameter]
        public EventCallback<DataSet> DataReceived { get; set; }
        
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
                TableNames = await _counterDataService.GetTableNamesAsync();
            }
        }
        protected async Task FetchData()
        {
            var result = await _counterDataService.GetDataAsync(_startMjd, _endMjd, _selectedTableName);
            await DataReceived.InvokeAsync(new DataSet() { Data = result });
        }
    }
}

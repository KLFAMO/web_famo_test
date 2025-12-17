using FamoNET.Model.Interfaces;
using Microsoft.AspNetCore.Components;

namespace FamoNET.Components.Pages
{
    public partial class FreqMonitor : ComponentBase
    {
        private const double sr88 = 429228066418007;
        private const double n = 1716959;
        private const double f0 = 35000000;        
        private const double sr1 = 429228470332607;
        private const double aomSr1 = 84000000;
        private string _selectedCounter = "5";
        
        private List<double> _values;
        public List<double> Values 
        { 
            get => _values; 
            set
            {
                if (_values == value)
                    return;
                _values = value;
                StateHasChanged();
            }
        }

        private List<double> _results = new List<double>();
        public List<double> Results
        {
            get => _results;
            set
            {
                if (_results == value)
                    return;

                _results = value;
                StateHasChanged();
            }
        }

        private List<double> _recentValues = new List<double>();

        [Inject]
        private IFreqMonitorDataService FreqMonitorDataService { get; set; }
        public string SelectedCounter 
        { 
            get => _selectedCounter;
            set
            {
                if (value == _selectedCounter)
                    return;
                _selectedCounter = value;
                StateHasChanged();

                _recentValues.Clear();
            }
        }

        private readonly PeriodicTimer _periodicTimer = new(TimeSpan.FromSeconds(1));        

        protected override async Task OnInitializedAsync()
        {            
            await base.OnInitializedAsync();
            _ = RefreshData();
        }

        private async Task RefreshData()
        {
            while (await _periodicTimer.WaitForNextTickAsync())
            {
                _results.Clear();
                _values = await FreqMonitorDataService.GetValues();
                for (int i = 0; i < _values.Count; i++)
                {
                    if (i != Int32.Parse(_selectedCounter))
                    {
                        _results.Add(0);
                        continue;
                    }

                    var calculatedValue = Math.Floor((((2.0 * f0 + n * _values[8] + _values[i]) - aomSr1) - sr88) / 4.0);
                    
                    if (Int32.Parse(_selectedCounter) == i)
                    {
                        _recentValues.Add(calculatedValue);
                        if (_recentValues.Count > 10)
                        {
                            _recentValues.RemoveAt(0);
                        }
                        
                        _results.Add(_recentValues.Average());
                    }                                                
                    else
                        _results.Add(calculatedValue);
                }
                
                await InvokeAsync(StateHasChanged);
            }
        }        
    }
}

using FamoNET.Model;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace FamoNET.Components.SubComponents.Chart
{
    public partial class SeriesListComponent : ComponentBase
    {
        [Parameter]
        public DataSeries<double> SelectedSeries { get; set; }
        [Parameter]
        public EventCallback<DataSeries<double>> SelectedSeriesChanged { get; set; }
        [Parameter]
        public EventCallback SeriesDeleted { get; set; }
        [Parameter]
        public EventCallback SeriesAdded { get; set; }
        public List<DataSeries<double>> SeriesList = new List<DataSeries<double>>();
        

        public async Task AddSeries(string label, List<DataPoint<double>> data)
        {
            SeriesList.Add(new DataSeries<double>(data, label));
            if (SeriesList.Count == 1)
            {
                SelectedSeries = SeriesList.FirstOrDefault();
                await SelectedSeriesChanged.InvokeAsync(SelectedSeries);                
            }

            await SeriesAdded.InvokeAsync();
            await Task.Yield();
            await InvokeAsync(StateHasChanged);
        }

        public async Task RemoveSeries(Guid guid)
        {
            var seriesToRemove = SeriesList.FirstOrDefault(s => s.Guid == guid);
            SeriesList.Remove(seriesToRemove);
            if (SeriesList.Count>0)
            {
                SelectedSeries = SeriesList.First();
            }
            else
            {
                SelectedSeries = new DataSeries<double>(new List<DataPoint<double>>(), "empty");
            }
            await SelectedSeriesChanged.InvokeAsync(SelectedSeries);
            await SeriesDeleted.InvokeAsync();

            await Task.Yield();
            await InvokeAsync(StateHasChanged);
        }

        public async Task SelectSeries(DataSeries<double> selectedSeries)
        {
            SelectedSeries = selectedSeries;
            await SelectedSeriesChanged.InvokeAsync(SelectedSeries);
        }

        private async Task OnLabelChange(KeyboardEventArgs keyboardArgs)
        {
            if (keyboardArgs.Code == "Enter" || keyboardArgs.Code == "NumpadEnter")
            {
                await SelectedSeriesChanged.InvokeAsync(SelectedSeries);
            }
        }
    }
}

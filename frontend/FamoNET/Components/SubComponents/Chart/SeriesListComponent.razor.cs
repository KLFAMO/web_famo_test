using FamoNET.Model;
using Microsoft.AspNetCore.Components;

namespace FamoNET.Components.SubComponents.Chart
{
    public partial class SeriesListComponent : ComponentBase
    {
        [Parameter]
        public DataSeries<double> SelectedSeries { get; set; }
        [Parameter]
        public EventCallback<DataSeries<double>> SelectedSeriesChanged { get; set; }
        public List<DataSeries<double>> SeriesList = new List<DataSeries<double>>();
        

        public void AddSeries(string label, List<DataPoint<double>> data)
        {
            SeriesList.Add(new DataSeries<double>(data, label));
            if (SeriesList.Count == 1)
            {
                SelectedSeries = SeriesList.FirstOrDefault();
                SelectedSeriesChanged.InvokeAsync(SelectedSeries);
            }

            StateHasChanged();
        }

        public void RemoveSeries(Guid guid)
        {
            SeriesList.Remove(SeriesList.FirstOrDefault(s => s.Guid == guid));
            StateHasChanged();
        }
    }
}

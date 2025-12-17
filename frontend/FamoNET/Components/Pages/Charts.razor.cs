using FamoNET.Components.SubComponents;
using FamoNET.Components.SubComponents.Wizards;
using Microsoft.AspNetCore.Components;

namespace FamoNET.Components.Pages
{
    public partial class Charts : ComponentBase
    {
        public Dictionary<Guid, SpectrumAnalyzerChart> LoadedCharts = new Dictionary<Guid, SpectrumAnalyzerChart>();
        //public List<SpectrumAnalyzerChart> LoadedCharts = new List<SpectrumAnalyzerChart>() { };
        private SystemMessageComponent SystemMessageComponent;
        private FileWizard FileWizard;
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            if (firstRender)
            {
                AddChart();
                StateHasChanged();
            }
        }      

        public void AddChart()
        {            
            LoadedCharts.Add(Guid.NewGuid(), new SpectrumAnalyzerChart());
            StateHasChanged();
        }

        private async Task OnChartClose(Guid senderGuid)
        {            
            LoadedCharts.TryGetValue(senderGuid, out var chart);
            
            if (chart != null)
            {
                LoadedCharts.Remove(senderGuid);
                await chart.DisposeAsync();
            }
            

            StateHasChanged();
        }
    }
}

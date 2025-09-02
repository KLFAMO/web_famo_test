using FamoNET.Components.SubComponents;
using Microsoft.AspNetCore.Components;

namespace FamoNET.Components.Pages
{
    public partial class Charts : ComponentBase
    {
        public List<RhodeSchwarz1000Chart> LoadedCharts = new List<RhodeSchwarz1000Chart>() { };

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
            LoadedCharts.Add(new RhodeSchwarz1000Chart());
            StateHasChanged();
        }
    }
}

using FamoNET.Components.SubComponents;
using FamoNET.Components.SubComponents.Chart;
using FamoNET.Model;
using FamoNET.Services;
using FamoNET.Services.DataServices;
using Microsoft.AspNetCore.Components;
using NLog;

namespace FamoNET.Components.Pages
{
    public partial class LiveBrowser : ComponentBase
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        [Inject]
        private AndaDataService AndaDataService { get; set; } = default!;
        protected LiveChart LiveChart { get; set; } = default!;
        

        public List<string> TableNames { get; set; } = new List<string>();

        private int _points = 300;
        private string _selectedTableName = String.Empty;

        protected int Points { get; set; }
        public string SelectedTableName { get; set; } = String.Empty;
        

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            if (firstRender)
            {
                TableNames = await AndaDataService.GetTableNamesAsync();
                StateHasChanged();
            }
        }    
        
        protected void StartLive()
        {
            Points = _points;
            SelectedTableName = _selectedTableName;
            LiveChart.StartLive();
        }
                       
    }
}

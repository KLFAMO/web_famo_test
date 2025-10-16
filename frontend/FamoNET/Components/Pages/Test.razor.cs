using FamoNET.Model;
using FamoNET.Model.Interfaces;
using Microsoft.AspNetCore.Components;

namespace FamoNET.Components.Pages
{
    public partial class Test : ComponentBase
    {
        [Inject]
        private IDevicesDataService DevicesDataService { get; set; }
        public List<Device> Devices { get; set; } = [];
        public List<Device> DevicesFiltered { get; set; } = [];
        public string SelectedIP { get; set; }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            if (firstRender)
            {
                await LoadData();
            }
        }

        private async Task LoadData()
        {
            Devices = await DevicesDataService.GetDevicesAsync();
            DevicesFiltered = await DevicesDataService.GetDevicesAsync(new List<string> { "dds_kam" });
            await InvokeAsync(() => StateHasChanged());
        }
    }
}

using FamoNET.Model;
using Microsoft.AspNetCore.Components;

namespace FamoNET.Components.Pages
{
    public partial class DDSConfiguration : ComponentBase
    {
        private int DeviceId { get; set; } = -1;
        public void OnDeviceSelected(Device device)
        {
            DeviceId = device.Id;
            StateHasChanged();
        }
    }
}

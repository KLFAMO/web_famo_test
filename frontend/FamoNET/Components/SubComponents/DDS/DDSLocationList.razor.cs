using FamoNET.Model;
using FamoNET.Model.Interfaces;
using Microsoft.AspNetCore.Components;
using NLog;

namespace FamoNET.Components.SubComponents.DDS
{
    public partial class DDSLocationList : ComponentBase
    {
        private bool _isLoading = false;
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        [Inject]
        private IDDSDataService _ddsDataService { get; set; }
        [Inject]
        private ISystemNotificationService _systemNotificationService { get; set; }

        private Dictionary<string, List<Device>> DevicesByLocation { get; set; } = new Dictionary<string, List<Device>>();


        protected override async Task OnInitializedAsync()        
        {
            await base.OnInitializedAsync();

            try
            {
                IsLoading = true;
                var devices = await _ddsDataService.GetDevices();

                if (devices == null)
                    return;

                foreach (var device in devices)
                {
                    if (DevicesByLocation.TryGetValue(device.Location, out var collection))
                    {
                        collection.Add(device);
                    }
                    else
                    {
                        DevicesByLocation.Add(device.Location, new List<Device>() { device });
                    }
                }

            }
            catch (Exception ex)
            {
                _systemNotificationService.SendSystemMessage(this, new SystemMessage("Failed to fetch dds devices", SystemMessageType.Error));
                _logger.Error(ex);
            }
            finally
            {
                IsLoading = false;
            }
            

        }
        [Parameter]
        public EventCallback<Device> DeviceSelected { get; set; }
        public bool IsLoading 
        { 
            get => _isLoading;
            set 
            { 
                if (value == _isLoading) return;
                _isLoading = value;
                StateHasChanged();
            } 
        }

        private void OnDeviceSelect(Device device)
        {
            DeviceSelected.InvokeAsync(device);
        }
    }
}

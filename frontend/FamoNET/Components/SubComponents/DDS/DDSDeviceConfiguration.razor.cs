using FamoNET.Model;
using FamoNET.Model.Interfaces;
using Microsoft.AspNetCore.Components;
using NLog;

namespace FamoNET.Components.SubComponents.DDS
{
    public partial class DDSDeviceConfiguration : ComponentBase
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        [Inject]
        private IDDSDataService _ddsDataService { get; set; }
        [Inject]
        private ISystemNotificationService _systemNotificationService { get; set; }
        public Model.DDSDevice Model { get; set; }

        [Parameter]
        public int DeviceId { get; set; } = -1;
        private int _lastDeviceId = -1;

        private bool _isLoading = false;
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

        private async Task SaveSettings()
        {
            try
            {
                foreach(var channel in Model.Channels)
                {
                    if (!(channel.Frequency.IsValid && channel.Phase.IsValid && channel.Amplitude.IsValid))
                    {
                        _systemNotificationService.SendSystemMessage(this, new SystemMessage("Provided value is out of range", SystemMessageType.Error));
                        return;
                    }
                }

                await _ddsDataService.SendDeviceConfiguration(DeviceId, Model.Channels);
                _systemNotificationService.SendSystemMessage(this, new SystemMessage("Default settings saved", SystemMessageType.Info));
            }
            catch(Exception ex)
            {
                _systemNotificationService.SendSystemMessage(this, new SystemMessage("Failed to save settigns", SystemMessageType.Error));
                _logger.Error(ex);
            }
        }

        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();

            if (DeviceId < 0 || DeviceId == _lastDeviceId)
            {
                return;
            }

            _lastDeviceId = DeviceId;

            try
            {
                IsLoading = true;
                Model = await _ddsDataService.GetById(DeviceId);
            }
            catch (Exception ex)
            {
                _systemNotificationService.SendSystemMessage(this, new SystemMessage("Failed to load dds", SystemMessageType.Error));
                _logger.Error(ex);
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}

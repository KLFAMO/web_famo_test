using FamoNET.Database.Model.Interfaces;
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
        private IDDSDevicesRepository _ddsDevicesRepository { get; set; }
        [Inject]
        private IDDSChannelsRepository _ddsChannelRepository { get; set; }
        [Inject]
        private ISystemNotificationService _systemNotificationService { get; set; }
        public DDSDevice Model { get; set; }

        [Parameter]
        public int ApiDeviceId { get; set; } = -1;
        private int _lastApiDeviceId = -1;

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
        
        private async void OnChannelLockChange(ChangeEventArgs e, DDSChannel channel)
        {
            channel.IsLocked = Boolean.Parse(e.Value.ToString());
            try
            {
                var fetchedChannel = await _ddsChannelRepository.GetByApiDeviceIdAndNameAsync(channel.Name, _lastApiDeviceId);
                if (fetchedChannel == null)
                {
                    await _ddsChannelRepository.InsertAsync(new Database.Model.Classes.DDSChannel() { Device = new () { ApiDeviceId = _lastApiDeviceId }, Name=channel.Name, IsLocked = channel.IsLocked });
                    fetchedChannel = await _ddsChannelRepository.GetByApiDeviceIdAndNameAsync(channel.Name, _lastApiDeviceId);
                }

                if (channel.IsLocked)
                {
                    await _ddsChannelRepository.LockChannel(fetchedChannel.Id);
                    _systemNotificationService.SendSystemMessage(this, new SystemMessage("Channel locked", SystemMessageType.Info));
                }
                else
                {
                    await _ddsChannelRepository.UnlockChannel(fetchedChannel.Id);
                    _systemNotificationService.SendSystemMessage(this, new SystemMessage("Channel unlocked", SystemMessageType.Info));
                }

            }
            catch(Exception ex)
            {
                _systemNotificationService.SendSystemMessage(this, new SystemMessage(ex.Message, SystemMessageType.Error));
                _logger.Error(ex);
            }
        }

        private async void OnDeviceLockChange(ChangeEventArgs e)
        {
            try
            {
                Model.IsLocked = Boolean.Parse(e.Value.ToString());

                var device = await _ddsDevicesRepository.GetByApiDeviceIdAsync(ApiDeviceId);                            

                if (Model.IsLocked)
                {
                    await _ddsDevicesRepository.LockDevice(device.Id);
                    _systemNotificationService.SendSystemMessage(this, new SystemMessage("Device locked", SystemMessageType.Info));
                }
                else
                {
                    await _ddsDevicesRepository.UnlockDevice(device.Id);
                    _systemNotificationService.SendSystemMessage(this, new SystemMessage("Device unlocked", SystemMessageType.Info));
                }
            }
            catch(Exception ex)
            {
                _systemNotificationService.SendSystemMessage(this, new SystemMessage(ex.Message, SystemMessageType.Error));
                _logger.Error(ex);
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

                await _ddsDataService.SendDeviceConfiguration(ApiDeviceId, Model.Channels);
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

            if (ApiDeviceId < 0 || ApiDeviceId == _lastApiDeviceId)
            {
                return;
            }

            _lastApiDeviceId = ApiDeviceId;
            var rep_device = await _ddsDevicesRepository.GetByApiDeviceIdAsync(ApiDeviceId);            
            
            if (rep_device == null)
            {
                await _ddsDevicesRepository.InsertAsync(new Database.Model.Classes.DDSDevice { ApiDeviceId = ApiDeviceId, IsLocked = false });                
            }

            var rep_channels = await _ddsChannelRepository.GetByApiDeviceIdAsync(ApiDeviceId);

            try
            {
                IsLoading = true;
                Model = await _ddsDataService.GetById(ApiDeviceId);
                Model.IsLocked = rep_device?.IsLocked ?? false;
                
                if (Model.Channels?.Count > 0 && rep_channels?.Count > 0)
                {
                    foreach (var channel in Model.Channels)
                    {
                        var rep_channel = rep_channels.FirstOrDefault(c => c.Name == channel.Name);
                        channel.Name = rep_channel?.Name ?? channel.Name; 
                        channel.IsLocked = rep_channel?.IsLocked ?? false;
                    }
                }
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

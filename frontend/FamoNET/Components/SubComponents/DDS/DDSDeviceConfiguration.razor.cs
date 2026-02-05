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
        
        private async void OnChannelLockChange(ChangeEventArgs e, DDSChannel channel)
        {
            channel.IsLocked = Boolean.Parse(e.Value.ToString());
            try
            {
                var fetchedChannel = await _ddsChannelRepository.GetSingleAsync(channel.Name, _lastDeviceId);
                if (fetchedChannel == null)
                {
                    await _ddsChannelRepository.InsertAsync(new Database.Model.Classes.DDSChannel() { Device = new () { Id = _lastDeviceId }, Name=channel.Name, IsLocked = channel.IsLocked });
                }

                if (channel.IsLocked)
                {
                    await _ddsChannelRepository.LockChannel(channel.Id);
                    _systemNotificationService.SendSystemMessage(this, new SystemMessage("Channel locked", SystemMessageType.Info));
                }
                else
                {
                    await _ddsChannelRepository.UnlockChannel(channel.Id);
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

                var device = await _ddsDevicesRepository.GetByApiDeviceIdAsync(DeviceId);                            

                if (Model.IsLocked)
                {
                    await _ddsDevicesRepository.LockDevice(DeviceId);
                    _systemNotificationService.SendSystemMessage(this, new SystemMessage("Device locked", SystemMessageType.Info));
                }
                else
                {
                    await _ddsDevicesRepository.UnlockDevice(DeviceId);
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
            var rep_device = await _ddsDevicesRepository.GetByApiDeviceIdAsync(DeviceId);            
            
            if (rep_device == null)
            {
                await _ddsDevicesRepository.InsertAsync(new Database.Model.Classes.DDSDevice { ApiDeviceId = Model.Id, IsLocked = false });                
            }

            var rep_channels = await _ddsChannelRepository.GetByDeviceIdAsync(DeviceId);



            try
            {
                IsLoading = true;
                Model = await _ddsDataService.GetById(DeviceId);
                Model.IsLocked = rep_device?.IsLocked ?? false;
                
                if (Model.Channels?.Count > 0 && rep_channels?.Count > 0)
                {
                    foreach (var channel in Model.Channels)
                    {
                        channel
                        channel.IsLocked = rep_channels.FirstOrDefault(c => c.Name == channel.Name)?.IsLocked ?? false;
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

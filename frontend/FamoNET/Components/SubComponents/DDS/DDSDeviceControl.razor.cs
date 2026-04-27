using FamoNET.Database.Model.Interfaces;
using FamoNET.Database.Repositories.Implementations;
using FamoNET.Model;
using FamoNET.Model.Args;
using FamoNET.Model.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using NLog;
using System.Reflection.Metadata.Ecma335;

namespace FamoNET.Components.SubComponents.DDS
{
    public partial class DDSDeviceControl : ComponentBase
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        [Inject]
        private IDDSDataService _ddsDataService { get; set; } = default!;
        [Inject]
        private IDDSDevicesRepository _ddsDevicesRepository { get; set; } = default!;
        [Inject]
        private IDDSChannelsRepository _ddsChannelsRepository { get; set; } = default!;
        [Inject]
        private ITelnetService _telnetService { get; set; } = default!;
        [Inject]
        private ISystemNotificationService _systemNotificationService { get; set; } = default!;
        public Model.DDSDevice Model { get; set; } 

        [Parameter]
        public int DeviceId { get; set; } = -1;
        private int _lastDeviceId { get; set; } = -1;

        private bool _isLoading = false;
        public bool IsLoading
        {
            get => _isLoading; 
            set
            {
                if (value ==  _isLoading) return;
                _isLoading = value;
                StateHasChanged();
            }
                
        }
        public void OnModalConfirmed(ModalConfirmationEventArgs e)
        {
            if (e.Operation != Operation.Update)
                return;
        }

        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();

            if (DeviceId < 0 || _lastDeviceId == DeviceId)
            {
                return;
            }

            _lastDeviceId = DeviceId;
            var rep_device = await _ddsDevicesRepository.GetByApiDeviceIdAsync(DeviceId);

            try
            {
                IsLoading = true;
                Model = await _ddsDataService.GetById(DeviceId);
                Model.IsLocked = rep_device?.IsLocked ?? false;
                var rep_channels = await _ddsChannelsRepository.GetByApiDeviceIdAsync(DeviceId);

                if (Model.Channels?.Count > 0)
                {
                    for (int i = 0; i < Model.Channels.Count; ++i)
                    {
                        if (Model.IsLocked)
                        {
                            Model.Channels[i].IsLocked = true;
                            continue;
                        }

                        var rep_channel = rep_channels?.FirstOrDefault(c => c.Name == Model.Channels[i].Name);
                        if (rep_channel == null)
                            continue;

                        Model.Channels[i].IsLocked = rep_channel.IsLocked;
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
        
        private async Task SendCommand(DDSValue ddsValue)
        {
            if (!ddsValue.IsValid)
            {
                _systemNotificationService.SendSystemMessage(this, new SystemMessage("Invalid value. Check format and range.", SystemMessageType.Error));
                return;
            }

            await _telnetService.Send(Model.IP, Model.Port, string.Concat(ddsValue.Command, $" {ddsValue.Value}"), false);
            _systemNotificationService.SendSystemMessage(this, new SystemMessage("Value has been sent", SystemMessageType.Info));
        }

        private async Task ReadCommand(DDSValue ddsValue)
        {
            var telnetResponse = await _telnetService.Send(Model.IP, Model.Port, string.Concat(ddsValue.Command, $" ?"), false);            
            if (Double.TryParse(new string(telnetResponse.Response.Where(c => char.IsDigit(c) || c == '.' || c== ',').ToArray()), out var fetchedValue))
            {
                ddsValue.Value = fetchedValue;
                _systemNotificationService.SendSystemMessage(this, new SystemMessage("Value fetched", SystemMessageType.Info));
            }
            else
            {
                _systemNotificationService.SendSystemMessage(this, new SystemMessage("Failed to parse data.", SystemMessageType.Error));
                _logger.Error($"Failed to parse data from telnet response. {telnetResponse.Response}");
            }

            StateHasChanged();
        }
    }
}

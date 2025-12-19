using FamoNET.Model;
using FamoNET.Model.Args;
using FamoNET.Model.Interfaces;
using Microsoft.AspNetCore.Components;
using NLog;

namespace FamoNET.Components.SubComponents.DDS
{
    public partial class DDSDeviceControl : ComponentBase
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        [Inject]
        private IDDSDataService _ddsDataService { get; set; }
        [Inject]
        private ITelnetService _telnetService { get; set; }
        [Inject]
        private ISystemNotificationService _systemNotificationService { get; set; }
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
        
        private async Task SendCommand(DDSValue ddsValue)
        {
            await _telnetService.Send(Model.IP, Model.Port, string.Concat(ddsValue.Command, $" {ddsValue.Value}"), false);
            _systemNotificationService.SendSystemMessage(this, new SystemMessage("Value has been sent", SystemMessageType.Info));
        }

        private async Task ReadCommand(DDSValue ddsValue)
        {
            var telnetResponse = await _telnetService.Send(Model.IP, Model.Port, string.Concat(ddsValue.Command, $" ?"), false);
            if (Double.TryParse(telnetResponse.Response, out var fetchedValue))
            {
                ddsValue.Value = fetchedValue;
            }
            else
            {
                _systemNotificationService.SendSystemMessage(this, new SystemMessage("Failed to parse data.", SystemMessageType.Info));
                _logger.Error($"Failed to parse data from telnet response. {telnetResponse.Response}");
            }

            StateHasChanged();
        }
    }
}

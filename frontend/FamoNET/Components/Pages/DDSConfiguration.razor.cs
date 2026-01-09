using FamoNET.Model;
using FamoNET.Model.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Options;

namespace FamoNET.Components.Pages
{
    public partial class DDSConfiguration : ComponentBase
    {
        [Inject]
        private ISystemNotificationService _systemNotificationService { get; set; }
        [Inject]
        private IOptions<CredentialsOptions> Credentials { get; set; }

        private bool Authorized = false;
        protected string Password = String.Empty; //simple password
        private int DeviceId { get; set; } = -1;
        public void OnDeviceSelected(Device device)
        {
            DeviceId = device.Id;
            StateHasChanged();
        }

        public void Authorize()
        {
            if (Credentials.Value.DDSPassword == Password)
            {
                Authorized = true;
                StateHasChanged();
            }
            else
            {
                _systemNotificationService.SendSystemMessage(this, new SystemMessage("Invalid password", SystemMessageType.Error));
            }
        }

        public void OnKeyEnter(KeyboardEventArgs e)
        {
            if (e.Code == "Enter" || e.Code == "NumpadEnter")
            {
                Authorize();
            }
        }
    }
}

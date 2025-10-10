using FamoNET.Model;
using FamoNET.Model.Args;
using FamoNET.Model.Interfaces;

namespace FamoNET.Services
{
    public class SystemNotificationService : ISystemNotificationService
    {
        public event EventHandler<GenericEventArgs<SystemMessage>> SystemMessageReceived;

        public void SendSystemMessage(object sender, SystemMessage message)
        {
            SystemMessageReceived?.Invoke(this, new GenericEventArgs<SystemMessage>(message));
        }
    }
}

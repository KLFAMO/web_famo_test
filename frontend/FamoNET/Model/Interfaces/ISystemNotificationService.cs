using FamoNET.Model.Args;

namespace FamoNET.Model.Interfaces
{
    public interface ISystemNotificationService
    {
        event EventHandler<GenericEventArgs<SystemMessage>> SystemMessageReceived;
        void SendSystemMessage(object sender, SystemMessage message);
    }
}

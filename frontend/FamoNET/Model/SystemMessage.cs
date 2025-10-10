namespace FamoNET.Model
{
    public class SystemMessage
    {
        public SystemMessageType Type { get; }
        public string Message { get; }
        public SystemMessage(string message, SystemMessageType type)
        {
            Message = message;
            Type = type;
        }
        public SystemMessage(SystemMessage message)
        {
            if (message == null)
                return;

            Type = message.Type;
            Message = message.Message;
        }
    }

    public enum SystemMessageType
    {
        Info = 0,
        Error = 1
    }
}

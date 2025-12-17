namespace FamoNET.Model
{
    public class TerminalMessage
    {
        public string Request { get; set; }
        public string Response { get; set; }
        public TerminalMessageType Type { get; set; }
    }
}

namespace FamoNET.Model.Interfaces
{
    public interface ITelnetService
    {
        Task Send(string ip, string message);
    }
}

using FamoNET.Database.Model.Classes;
using FamoNET.Model.Dto;

namespace FamoNET.Model.Interfaces
{
    public interface ITelnetService
    {
        Task<TelnetResponseDto> Send(string ip, int port, string message);
        IEnumerable<TerminalCommand> GetHistoryForDevice(string ip, int port);
    }
}

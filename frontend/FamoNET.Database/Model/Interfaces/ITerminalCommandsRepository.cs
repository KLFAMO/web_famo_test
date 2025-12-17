using FamoNET.Database.Model.Classes;

namespace FamoNET.Database.Model.Interfaces
{
    public interface ITerminalCommandsRepository
    {
        Task AddAsync(TerminalCommand terminalCommand);
        IEnumerable<TerminalCommand> GetForDevice(string ip, int port);
    }
}

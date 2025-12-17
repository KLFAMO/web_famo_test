using FamoNET.Database.Model.Classes;
using FamoNET.Database.Model.Interfaces;

namespace FamoNET.Database.Repositories.Implementations
{
    public class TerminalCommandsRepository : RepositoryBase, ITerminalCommandsRepository
    {
        public TerminalCommandsRepository(MainDbContext context) : base(context)
        {
            
        }

        public async Task AddAsync(TerminalCommand terminalCommand)
        {
            if (terminalCommand == null || string.IsNullOrWhiteSpace(terminalCommand.IP))
                throw new InvalidDataException("Invalid terminal command");

            terminalCommand.CreatedOn = DateTime.Now;
            Context.TerminalCommands.Add(terminalCommand);
            await Context.SaveChangesAsync();
        }

        public IEnumerable<TerminalCommand> GetForDevice(string ip, int port)
        {
            return Context.TerminalCommands.Where(t => t.IP == string.Concat(ip, ":", port)).Take(10);
        }
    }
}

using FamoNET.Database.Model.Classes;
using FamoNET.Database.Model.Interfaces;
using FamoNET.Model;
using FamoNET.Model.Dto;
using FamoNET.Model.Interfaces;

namespace FamoNET.Services.Mock
{
    public class MockTelnetService : ITelnetService
    {
        private readonly ITerminalCommandsRepository _terminalCommandsRepository;

        public MockTelnetService(ITerminalCommandsRepository terminalCommandsRepository)
        {
            _terminalCommandsRepository = terminalCommandsRepository;
        }
        public IEnumerable<TerminalCommand> GetHistoryForDevice(string ip, int port)
        {
            return _terminalCommandsRepository.GetForDevice(ip, port); 
        }        

        public async Task<TelnetResponseDto> Send(string ip, int port, string message, bool insertHistory = true)
        {
            await Task.Delay(1000);

            //return new TelnetResponseDto()
            //{
            //    Response = "Error!11!",
            //    Status = "error"
            //};
            var objToAdd = new TerminalCommand()
            {
                Request = message,
                Response = "Ok",
                ResponseType = (int)TerminalMessageType.Ok,
                IP = string.Concat(ip, ":", port)
            };

            if (insertHistory)
                await _terminalCommandsRepository.AddAsync(objToAdd);

            return new TelnetResponseDto()
            {
                Response = "Succcess!11!",
                Status = "ok"
            };
        }
    }
}

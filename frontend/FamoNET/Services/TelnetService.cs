using FamoNET.Database.Model.Classes;
using FamoNET.Database.Model.Interfaces;
using FamoNET.Model;
using FamoNET.Model.Dto;
using FamoNET.Model.Interfaces;
using NLog;
using System.Text;
using System.Text.Json;

namespace FamoNET.Services
{
    public class TelnetService : ITelnetService
    {
        protected readonly static Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly ITerminalCommandsRepository _terminalCommandsRepository;
        private HttpClient _httpClient;

        public TelnetService(string uri, ITerminalCommandsRepository terminalCommandsRepository)
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(uri);

            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36");
            _terminalCommandsRepository = terminalCommandsRepository;
        }

        public IEnumerable<TerminalCommand> GetHistoryForDevice(string ip, int port)
        {
            return _terminalCommandsRepository.GetForDevice(ip, port);
        }

        public async Task<TelnetResponseDto> Send(string ip, int port, string message)
        {            
            dynamic dto = new { ip = ip, port=port, command = message };
            var content = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");

            var messageToSave = new TerminalCommand();
            messageToSave.Request = message;
            messageToSave.IP = string.Concat(ip, ":", port);

            HttpResponseMessage response = null;
            try
            {
                response = await _httpClient.PostAsync("", content);                
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                throw;
            }

            try
            {
                var telnetResponse = JsonSerializer.Deserialize<TelnetResponseDto>(await response.Content.ReadAsStringAsync());

                messageToSave.Response = telnetResponse.Response;
                messageToSave.ResponseType = (int)TerminalMessageType.Ok;

                await _terminalCommandsRepository.AddAsync(messageToSave);
                return telnetResponse;
            }
            catch(Exception ex)
            {
                messageToSave.ResponseType = (int)TerminalMessageType.Error;
                await _terminalCommandsRepository.AddAsync(messageToSave);

                Logger.Error($"Failed to read response", ex);
                return new TelnetResponseDto()
                {
                    Status = "Error",
                    Response = "Unknown error while reading response from API"
                };
            }
        }
    }
}

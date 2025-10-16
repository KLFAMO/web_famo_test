using FamoNET.Model;
using FamoNET.Model.Interfaces;
using NLog;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace FamoNET.Services
{
    public class TelnetService : ITelnetService
    {
        protected readonly static Logger Logger = LogManager.GetCurrentClassLogger();
        private HttpClient _httpClient;

        public TelnetService(string uri)
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(uri);

            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36");
        }

        public async Task Send(string ip, string message)
        {
            dynamic dto = new { ip = ip, message = message };
            var content = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");

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

            if (!response.IsSuccessStatusCode)
            {
                Logger.Error($"Wrong status code: {response.StatusCode}. Address: {_httpClient.BaseAddress}");
                return;
            }
        }
    }
}

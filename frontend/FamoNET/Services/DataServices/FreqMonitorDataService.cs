using FamoNET.Model.Interfaces;
using NLog;
using System.Text.Json;

namespace FamoNET.Services.DataServices
{
    public class FreqMonitorDataService : IFreqMonitorDataService
    {
        private readonly static Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly HttpClient _httpClient;

        public FreqMonitorDataService(string freqMonitorUri)
        {
            _httpClient = new HttpClient()
            {
                BaseAddress = new Uri(freqMonitorUri)
            };

            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36");
        }

        public async Task<List<double>> GetValues()
        {
            HttpResponseMessage response = null;
            try
            {
                response = await _httpClient.GetAsync("get_data");
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            var content = JsonSerializer.Deserialize<List<double>>(json.Substring(18).TrimEnd('}'));
            return content;
        }
    }
}

using FamoNET.Model;
using FamoNET.Model.Interfaces;
using System.Text;
using System.Text.Json;

namespace FamoNET.Services.DataServices
{
    public class DDSDataService : DDSDataServiceBase, IDDSDataService
    {
        private readonly string _endpoint;
        public DDSDataService(string endpoint, IDevicesDataService devicesDataService) : base(endpoint, devicesDataService)
        {
            _endpoint = endpoint;
        }

        public async Task<List<Device>> GetDevices()
        {
            HttpResponseMessage response = null;
            try
            {
                response = await HttpClient.GetAsync($"{_endpoint.TrimEnd('/')}?tag=dds", CancellationTokenSource.Token);
                if (!response.IsSuccessStatusCode)
                {
                    Logger.Error($"Wrong status code: {response.StatusCode}. Address: {HttpClient.BaseAddress}");
                    return null;
                }

                var result = JsonSerializer.Deserialize<List<Device>>(await response.Content.ReadAsStringAsync());
                if (result == null)
                    throw new Exception("Failed to parse data from API");


                return result;

            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                throw;
            }
        }

        public async Task SendDeviceConfiguration(int deviceId, List<DDSChannel> channels)
        {
            var updateJson = GetJsonRequest(deviceId, channels);
            var content = new StringContent(updateJson.ToString(), Encoding.UTF8, "application/json");

            HttpResponseMessage response = null;

            HttpClient client = new HttpClient() //temp solution, backend needs to change endpoint name
            {
                BaseAddress = new Uri(_endpoint.Substring(0, _endpoint.Length - 2))
            };
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36");

            try
            {                
                response = await client.PatchAsync($"{deviceId}/update", content);
                if (!response.IsSuccessStatusCode)
                {
                    Logger.Error($"Wrong status code: {response.StatusCode}. Address: {client.BaseAddress}/{deviceId}/update");
                    return;
                }

                var result = JsonSerializer.Deserialize<List<Device>>(await response.Content.ReadAsStringAsync());
                if (result == null)
                    throw new Exception("Failed to parse data from API");

                if (!response.IsSuccessStatusCode)
                {
                    Logger.Error(await response.Content.ReadAsStringAsync());
                    throw new InvalidOperationException("Failed to update data.");
                }                                    
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                throw;
            }            
        }
    }
}

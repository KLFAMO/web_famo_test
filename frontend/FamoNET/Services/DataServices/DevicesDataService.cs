using FamoNET.Model;
using FamoNET.Model.Interfaces;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using System.Text.Json;

namespace FamoNET.Services.DataServices
{
    public class DevicesDataService : DataServiceBase, IDevicesDataService
    {        
        public DevicesDataService(string endpoint) : base(endpoint)
        {
        }

        public async Task<List<Device>> GetDevicesAsync()
        {
            HttpResponseMessage response = null;
            try
            {
                response = await HttpClient.GetAsync("", CancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                throw;
            }

            if (!response.IsSuccessStatusCode)
            {
                Logger.Error($"Wrong status code: {response.StatusCode}. Address: {HttpClient.BaseAddress}");
                return null;
            }

            try
            {
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

        public async Task<List<Device>> GetDevicesAsync(List<string> types)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("?");
            for (int i=0; i<types.Count; ++i)
            {
                sb.Append("device_type=");
                sb.Append(types[i]);
                if (i < types.Count-1)
                {
                    sb.Append("&");
                }                
            }
            

            HttpResponseMessage response = null;
            try
            {
                response = await HttpClient.GetAsync(sb.ToString(), CancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                throw;
            }

            if (!response.IsSuccessStatusCode)
            {
                Logger.Error($"Wrong status code: {response.StatusCode}. Address: {HttpClient.BaseAddress}");
                return null;
            }

            try
            {
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
    }
}

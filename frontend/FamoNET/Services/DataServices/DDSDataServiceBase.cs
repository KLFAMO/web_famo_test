using FamoNET.Model;
using FamoNET.Model.Interfaces;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace FamoNET.Services.DataServices
{
    public abstract class DDSDataServiceBase : DataServiceBase
    {
        private IDevicesDataService _devicesDataService;

        protected DDSDataServiceBase(string endpoint, IDevicesDataService devicesDataService) : base(endpoint)
        {
            _devicesDataService = devicesDataService;
        }

        public virtual async Task<DDSDevice> GetById(int id)
        {
            var baseDevice = await _devicesDataService.GetById(id);

            HttpResponseMessage response = null;
            try
            {
                response = await HttpClient.GetAsync("", CancellationTokenSource.Token);
                if (!response.IsSuccessStatusCode)
                {
                    Logger.Error($"Wrong status code: {response.StatusCode}. Address: {HttpClient.BaseAddress}");
                    return null;
                }

                var result = JsonSerializer.Deserialize<List<DDSDevice>>(await response.Content.ReadAsStringAsync());
                if (result == null)
                    throw new Exception("Failed to parse data from API");


                return result.OrderBy(r => r.Name).ToList();

            }                                    
            catch (Exception ex)
            {
                Logger.Error(ex);
                throw;
            }           
        }

        protected async Task<JsonObject> GetJsonRequest(int deviceId, List<DDSChannel> channels)
        {
            JsonObject root = new JsonObject();
            JsonObject parameters = new JsonObject();
            root["eth_communication"] = new JsonObject();
            root["eth_communication"]["parameters"] = parameters;

            var oldDdsDevice = await GetById(deviceId);

            for (int i = 0; i < channels.Count; i++)
            {
                AddUpdate(parameters, channels[i].Frequency, oldDdsDevice.Channels[i].Frequency);
                AddUpdate(parameters, channels[i].Phase, oldDdsDevice.Channels[i].Phase);
                AddUpdate(parameters, channels[i].Amplitude, oldDdsDevice.Channels[i].Amplitude);
            }

            Logger.Debug(root);
        }
        private static void AddUpdate(JsonObject root, DDSValue newDdsValue, DDSValue oldDdsValue)
        {
            if (newDdsValue.Value == oldDdsValue.Value &&
                newDdsValue.Min == oldDdsValue.Min &&
                newDdsValue.Max == oldDdsValue.Max &&
                string.Equals(newDdsValue.Description, oldDdsValue.Description))
            {
                return;
            }

            var parts = newDdsValue.Command.Split(':');

            JsonObject current = root;

            // Iterate until the second-to-last part (building structure)
            for (int i = 0; i < parts.Length - 1; i++)
            {
                string propertyName = parts[i];

                // Create a new child object, attach it, and move 'current' deeper
                var next = new JsonObject();


                if (current[propertyName] == null)
                {
                    current[propertyName] = next;
                    current = next;
                }
                else
                {
                    current = current[propertyName].AsObject();
                }
            }

            // Handle different value types safely (assigning values)
            if (newDdsValue.Value != oldDdsValue.Value)
            {
                current["val"] = ConvertToJsonValue(newDdsValue.Value);
            }

            if (newDdsValue.Min != oldDdsValue.Min)
            {
                current["min"] = ConvertToJsonValue(newDdsValue.Min);
            }

            if (newDdsValue.Max != oldDdsValue.Max)
            {
                current["max"] = ConvertToJsonValue(newDdsValue.Max);
            }

            if (!string.Equals(oldDdsValue.Description, newDdsValue.Description))
            {
                current["label"] = ConvertToJsonValue(newDdsValue.Description);
            }
        }        

        protected static JsonNode ConvertToJsonValue(object value)
        {
            return value switch
            {
                int i => JsonValue.Create(i),
                double d => JsonValue.Create(d),
                string s => JsonValue.Create(s),
                bool b => JsonValue.Create(b),
                _ => JsonValue.Create(value?.ToString())
            };
        }
    }
}

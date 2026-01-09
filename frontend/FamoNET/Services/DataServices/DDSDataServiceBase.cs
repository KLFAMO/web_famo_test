using FamoNET.Model;
using FamoNET.Model.Interfaces;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace FamoNET.Services.DataServices
{
    public abstract class DDSDataServiceBase : DataServiceBase
    {
        protected IDevicesDataService _devicesDataService;
        private readonly string _endpoint;

        protected DDSDataServiceBase(string endpoint, IDevicesDataService devicesDataService) : base(endpoint)
        {
            _devicesDataService = devicesDataService;
            _endpoint = endpoint;
        }

        public virtual async Task<DDSDevice> GetById(int id)
        {
            var baseDevice = (await _devicesDataService.GetDevicesByTagsAsync(new() { "dds" })).FirstOrDefault(d => d.Id == id);

            if (baseDevice == null)
                throw new InvalidDataException("Device with such ID not found. Make sure device has assigned 'dds' tag.");

            HttpResponseMessage response = null;
            
            HttpClient client = new HttpClient() //temp solution, backend needs to change endpoint name
            {
                BaseAddress = new Uri(_endpoint.Substring(0, _endpoint.Length - 2) + '/')
            };
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36");

            try
            {                
                response = await client.GetAsync($"{id}/properties", CancellationTokenSource.Token);
                if (!response.IsSuccessStatusCode)
                {
                    Logger.Error($"Wrong status code: {response.StatusCode}. Address: {client.BaseAddress}{id}/properties");
                    throw new Exception("Device not found");
                }

                var result = JsonSerializer.Deserialize<DDSDevice>(await response.Content.ReadAsStringAsync());
                if (result == null)
                    throw new Exception("Failed to parse data from API");


                return new DDSDevice(baseDevice) { Channels = result.Channels, Port = result.Port };

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
            
            return root;            
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

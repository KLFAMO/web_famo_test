using FamoNET.Converters;
using FamoNET.Model;
using FamoNET.Model.Interfaces;
using NLog;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace FamoNET.Services.DataServices.Mock
{
    public class MockDDSDataService : IDDSDataService
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private IDevicesDataService _devicesDataService;
        public List<DDSDevice> DDSs { get; set; } = new List<DDSDevice>()
        {
            new DDSDevice()
            {
                Id = 10,
                Description = "Jakiś DDS 1",
                Name = "DDS1",
                IP = "192.168.3.1",
                Location = "Room1",
                Channels = new List<DDSChannel>
                {
                    new DDSChannel()
                    {
                        Id = 1,
                        Name = "Kanał 1",
                        Description = "Jakiś kanał 1",
                        Amplitude = new DDSValue()
                        { 
                            Value = 0,
                            Min = 0,
                            Max = 100
                        },
                        Phase = new DDSValue()
                        {
                            Value = 0,
                            Min = 0,
                            Max = 100
                        },
                        Frequency = new DDSValue()
                        {
                            Value = 0,
                            Min = 0,
                            Max = 100
                        },
                    }
                }
            },
            new DDSDevice()
            {
                Id = 1,
                Description = "Jakiś DDS 2",
                IP = "192.168.3.2",
                Name = "DDS2",
                Location = "Room1",
                Channels = new List<DDSChannel>
                {
                    new DDSChannel()
                    {
                        Id = 1,
                        Name = "Kanał 1",
                        Description = "Jakiś kanał 1",
                        Amplitude = new DDSValue()
                        {
                            Value = 0,
                            Min = 0,
                            Max = 100
                        },
                        Phase = new DDSValue()
                        {
                            Value = 0,
                            Min = 0,
                            Max = 100
                        },
                        Frequency = new DDSValue()
                        {
                            Value = 0,
                            Min = 0,
                            Max = 100
                        },
                    }
                }
            },
            new DDSDevice()
            {
                Id = 2,
                Description = "Jakiś DDS 3",
                Name = "DDS3",
                IP = "192.168.3.3",
                Location = "Room2",
                Channels = new List<DDSChannel>
                {
                    new DDSChannel()
                    {
                        Id = 1,
                        Name = "Kanał 1",
                        Description = "Jakiś kanał 1",
                        Amplitude = new DDSValue()
                        {
                            Value = 0,
                            Min = 0,
                            Max = 100
                        },
                        Phase = new DDSValue()
                        {
                            Value = 0,
                            Min = 0,
                            Max = 100
                        },
                        Frequency = new DDSValue()
                        {
                            Value = 0,
                            Min = 0,
                            Max = 100
                        },
                    }
                }
            },
            new DDSDevice()
            {
                Id = 3,
                Description = "Jakiś DDS 4",
                Name = "DDS4",
                IP = "192.168.3.4",
                Location = "Room3",
                Channels = new List<DDSChannel>
                {
                    new DDSChannel()
                    {
                        Id = 1,
                        Name = "Kanał 1",
                        Description = "Jakiś kanał 1",
                        Amplitude = new DDSValue()
                        {
                            Value = 0,
                            Min = 0,
                            Max = 100
                        },
                        Phase = new DDSValue()
                        {
                            Value = 0,
                            Min = 0,
                            Max = 100
                        },
                        Frequency = new DDSValue()
                        {
                            Value = 0,
                            Min = 0,
                            Max = 100
                        },
                    }
                }
            },
            new DDSDevice()
            {
                Id = 4,
                Description = "Jakiś DDS 5",
                Name = "DDS5",
                IP = "192.168.3.5",
                Location = "Room3",
                Channels = new List<DDSChannel>
                {
                    new DDSChannel()
                    {
                        Id = 1,
                        Name = "Kanał 1",
                        Description = "Jakiś kanał 1",
                        Amplitude = new DDSValue()
                        {
                            Value = 0,
                            Min = 0,
                            Max = 100
                        },
                        Phase = new DDSValue()
                        {
                            Value = 0,
                            Min = 0,
                            Max = 100
                        },
                        Frequency = new DDSValue()
                        {
                            Value = 0,
                            Min = 0,
                            Max = 100
                        },
                    }
                }
            }
        };

        public async Task<DDSDevice> GetById(int id)
        {
            string json = @"{
  ""device_type"": ""dds_kam"",
  ""eth_communication"": {
    ""port"": 23,
    ""parameters"": {
      ""DDS"": {
        ""CH0"": {
          ""FREQ"": {
            ""val"": 0,
            ""min"": 0,
            ""max"": 180,
            ""label"": ""Frequency of channel 0 in MHz""
          },
          ""AMP"": {
            ""val"": 0,
            ""min"": 0,
            ""max"": 1,
            ""label"": ""Amplitude of channel 0 (from 0 to 1)""
          },
         ""PH"": {
            ""val"": 0,
            ""min"": 0,
            ""max"": 1,
            ""label"": ""Phase of channel 0 (from 0 to 1)""
          }
        },
        ""CH1"": {
          ""FREQ"": {
            ""val"": 0,
            ""min"": 0,
            ""max"": 180,
            ""label"": ""Frequency of channel 1 in MHz""
          },
          ""AMP"": {
            ""val"": 0,
            ""min"": 0,
            ""max"": 1
          },
          ""PH"": {
            ""val"": 0,
            ""min"": 0,
            ""max"": 1,
            ""label"": ""Phase of channel 1 (from 0 to 1)""
          }
        }
      }
    }
  }
}";
            var baseDevice = await _devicesDataService.GetById(id);
            var options = new JsonSerializerOptions();
            options.Converters.Add(new DDSDeviceConverter()); // Uncomment if you removed the attribute

            // 3. Run the deserialization
            var ddsDevice = JsonSerializer.Deserialize<DDSDevice>(json, options);

            return new DDSDevice(baseDevice) { Channels = ddsDevice.Channels };              
        }

        public async Task<List<Device>> GetDevices()
        {
            return await _devicesDataService.GetDevicesAsync();
        }

        public MockDDSDataService(IDevicesDataService devicesDataService)
        {
            _devicesDataService = devicesDataService;
        }

        public async Task SendDeviceConfiguration(int deviceId, List<DDSChannel> channels)
        {
            JsonObject root = new JsonObject();
            JsonObject parameters = new JsonObject();
            root["eth_communication"] = new JsonObject();
            root["eth_communication"]["parameters"] = parameters;

            var oldDdsDevice = await GetById(deviceId);

            for (int i=0; i<channels.Count; i++) 
            {
                AddUpdate(parameters, channels[i].Frequency, oldDdsDevice.Channels[i].Frequency);
                AddUpdate(parameters, channels[i].Phase, oldDdsDevice.Channels[i].Phase);
                AddUpdate(parameters, channels[i].Amplitude, oldDdsDevice.Channels[i].Amplitude);
            }

            _logger.Debug(root);
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
        private static JsonNode ConvertToJsonValue(object value)
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

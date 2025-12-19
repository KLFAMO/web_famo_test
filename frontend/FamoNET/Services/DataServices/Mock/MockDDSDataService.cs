using FamoNET.Converters;
using FamoNET.Model;
using FamoNET.Model.Interfaces;
using NLog;
using System.Text.Json;

namespace FamoNET.Services.DataServices.Mock
{
    public class MockDDSDataService : DDSDataServiceBase, IDDSDataService
    {             
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

        public override async Task<DDSDevice> GetById(int id)
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
            var baseDevice = (await _devicesDataService.GetDevicesAsync()).FirstOrDefault(d => d.Id == id);
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

        public Task SendDeviceConfiguration(int deviceId, List<DDSChannel> channels)
        {
            var json = GetJsonRequest(deviceId, channels);
            Logger.Debug(json);
            return Task.CompletedTask;
        }

        public MockDDSDataService(IDevicesDataService devicesDataService) : base("http://localhost", devicesDataService)
        {
            _devicesDataService = devicesDataService;
        }

        
        
    }
}

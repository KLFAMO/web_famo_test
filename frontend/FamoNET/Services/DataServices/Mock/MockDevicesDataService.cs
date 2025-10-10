using FamoNET.Model;
using FamoNET.Model.Interfaces;

namespace FamoNET.Services.DataServices.Mock
{
    public class MockDevicesDataService : IDevicesDataService
    {
        public Task<List<Device>> GetDevicesAsync()
        {
            return Task.FromResult(new List<Device>() 
            {
                new Device()
                {
                    IP = "192.168.3.1",
                    Description = "Jakiś tam opis 1",
                    Location = "SR1",
                    DeviceType = "SpecAn",
                    Name = "spectrum_analyzer_sr1"
                },
                new Device()
                {
                    IP = "192.168.3.2",
                    Description = "Jakiś tam opis 2",
                    Location = "SR2",
                    DeviceType = "SpecAn",
                    Name = "spectrum_analyzer_sr2"
                }
            });
        }
    }
}

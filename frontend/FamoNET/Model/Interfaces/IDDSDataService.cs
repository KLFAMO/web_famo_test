namespace FamoNET.Model.Interfaces
{
    public interface IDDSDataService
    {
        Task<DDSDevice> GetById(int id);
        Task<List<Device>> GetDevices();
        Task SendDeviceConfiguration(int deviceId, List<DDSChannel> channels);
    }
}

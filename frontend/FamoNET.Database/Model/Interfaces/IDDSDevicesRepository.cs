using FamoNET.Database.Model.Classes;

namespace FamoNET.Database.Model.Interfaces
{
    public interface IDDSDevicesRepository
    {
        Task<DDSDevice> GetByDeviceIdAsync(int deviceId);
        Task<int> InsertAsync(DDSDevice device);
        Task LockDevice(int deviceId);
        Task UnlockDevice(int deviceId);
    }
}

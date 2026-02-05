using FamoNET.Database.Model.Classes;

namespace FamoNET.Database.Model.Interfaces
{
    public interface IDDSChannelsRepository
    {
        Task<DDSChannel> GetSingleAsync(string name, int deviceId);
        Task<List<DDSChannel>> GetByDeviceIdAsync(int deviceId);
        Task LockChannel(int channelId);
        Task UnlockChannel(int channelId);
        Task<int> InsertAsync(DDSChannel channel);
    }
}

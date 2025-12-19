namespace FamoNET.Model.Interfaces
{
    public interface IDevicesDataService
    {
        Task<List<Device>> GetDevicesAsync();
        Task<List<Device>> GetDevicesByTypeAsync(List<string> types);
        Task<List<Device>> GetDevicesByTagsAsync(List<string> tags);
        Task<Device> GetById(int id);
    }
}

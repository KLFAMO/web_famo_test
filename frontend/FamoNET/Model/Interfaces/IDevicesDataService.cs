namespace FamoNET.Model.Interfaces
{
    public interface IDevicesDataService
    {
        Task<List<Device>> GetDevicesAsync();
        Task<List<Device>> GetDevicesAsync(List<string> types);
        Task<Device> GetById(int id);
    }
}

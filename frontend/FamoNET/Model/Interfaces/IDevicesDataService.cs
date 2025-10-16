namespace FamoNET.Model.Interfaces
{
    public interface IDevicesDataService
    {
        Task<List<Device>> GetDevicesAsync();
        Task<List<Device>> GetDevicesAsync(List<string> types);
    }
}

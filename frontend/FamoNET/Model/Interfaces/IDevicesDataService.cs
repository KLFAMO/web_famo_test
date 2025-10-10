namespace FamoNET.Model.Interfaces
{
    public interface IDevicesDataService
    {
        Task<List<Device>> GetDevicesAsync();
    }
}

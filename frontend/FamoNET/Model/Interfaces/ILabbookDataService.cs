namespace FamoNET.Model.Interfaces
{
    public interface ILabbookDataService
    {
        Task<LabbookEntry> GetByIdAsync(int id);
        Task<List<LabbookEntry>> GetEntriesAsync(double from = 0, double to = 0);
        Task<bool> PostAsync(LabbookEntry entry);
        Task<bool> UpdateAsync(LabbookEntry entry);
    }
}

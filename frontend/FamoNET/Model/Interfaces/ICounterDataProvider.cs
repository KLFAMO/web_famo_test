namespace FamoNET.Model.Interfaces
{
    public interface ICounterDataProvider
    {
        Task<List<DataPoint>> GetData(decimal startMjd, decimal endMjd, string tableName);
        Task<List<DataPoint>> GetData(string query);
        Task<List<string>> GetTableNames();
    }
}

namespace FamoNET.Model.Interfaces
{
    public interface IAndaDataProvider
    {
        Task<List<DataPoint<double>>> GetData(double startMjd, double endMjd, string tableName);
        Task<List<DataPoint<double>>> GetData(string query);
        Task<List<string>> GetTableNames();
    }
}

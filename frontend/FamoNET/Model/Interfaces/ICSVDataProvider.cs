namespace FamoNET.Model.Interfaces
{
    public interface ICSVDataProvider
    {
        Task<List<DataPoint>> LoadCSV();
    }
}

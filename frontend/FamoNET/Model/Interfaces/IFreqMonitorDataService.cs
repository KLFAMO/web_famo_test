namespace FamoNET.Model.Interfaces
{
    public interface IFreqMonitorDataService
    {
        Task<List<double>> GetValues();
    }
}

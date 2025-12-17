namespace FamoNET.Model.Interfaces
{
    public interface IRemoteChartsDeviceFactory : IDisposable
    {
        Task<ISpectrumAnalyzerController> Create(string ip, int port);
    }
}

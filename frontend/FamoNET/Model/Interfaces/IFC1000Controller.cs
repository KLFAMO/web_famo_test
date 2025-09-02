namespace FamoNET.Model.Interfaces
{
    public interface IFC1000Controller
    {
        Task<(List<double> data, double rbw, double center, double span)> GetData();        
        void Initialize(string ip, int port);
        Task SetCenterFrequency(double frequency);
        Task SetFrequencySpan(double span);
        Task SetBandwidth(double bandwidth);
    }
}

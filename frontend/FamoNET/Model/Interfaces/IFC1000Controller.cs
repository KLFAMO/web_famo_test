using FamoNET.Model.Args;

namespace FamoNET.Model.Interfaces
{
    public interface IFC1000Controller
    {        
        void Initialize(string ip, int port);
        Task SetParameters(SpectrumAnalyzerParameters spectrumAnalyzerParameters);
        event EventHandler<SpectrumAnalyzerEventArgs> DataReceived;
    }
}

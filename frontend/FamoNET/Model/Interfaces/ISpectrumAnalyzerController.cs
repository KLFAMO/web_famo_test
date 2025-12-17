using FamoNET.Model.Args;

namespace FamoNET.Model.Interfaces
{
    public interface ISpectrumAnalyzerController : IDisposable
    {
        double RefreshRate { get; }
        void SetRefreshRate(double rate);
        
        Task SetParameters(SpectrumAnalyzerParameters spectrumAnalyzerParameters);
        void RefreshParameters();

        SpectrumAnalyzerParameters Data { get; }

        void Connect(string ip, int port);
    }
}

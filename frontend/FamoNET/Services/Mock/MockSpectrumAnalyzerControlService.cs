using FamoNET.Controllers.Mock;
using FamoNET.Model;
using FamoNET.Model.Args;
using FamoNET.Model.Interfaces;

namespace FamoNET.Services.Mock
{
    public class MockSpectrumAnalyzerControlService : ISpectrumAnalyzerController
    {
        public event EventHandler<SpectrumAnalyzerEventArgs> DataReceived;
        private ISpectrumAnalyzerController _spectrumAnalyzerController = new MockSpectrumAnalyzerController();

        public double RefreshRate
        {
            get => _spectrumAnalyzerController.RefreshRate;
            set
            {
                if (value == _spectrumAnalyzerController.RefreshRate)
                    return;
                _spectrumAnalyzerController.SetRefreshRate(value);
            }
        }


        public SpectrumAnalyzerParameters Data => _spectrumAnalyzerController.Data;

        public void Connect(string ip, int port)
        {            
            _spectrumAnalyzerController.Connect(ip, port);
        }

        public async Task SetParameters(SpectrumAnalyzerParameters spectrumAnalyzerParameters)
        {
            await _spectrumAnalyzerController.SetParameters(spectrumAnalyzerParameters);            
        }

        public void Dispose()
        {
            _spectrumAnalyzerController.Dispose();
        }

        public void SetRefreshRate(double rate)
        {
            _spectrumAnalyzerController.SetRefreshRate(rate);
        }

        public void RefreshParameters()
        {
            _spectrumAnalyzerController.RefreshParameters();
        }
    }
}

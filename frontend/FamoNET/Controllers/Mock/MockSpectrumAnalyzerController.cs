using FamoNET.Model;
using FamoNET.Model.Interfaces;
using NLog;

namespace FamoNET.Controllers.Mock
{
    public class MockSpectrumAnalyzerController : ISpectrumAnalyzerController
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private double _span;
        private double _rbw;

        public double RefreshRate { get; private set; }
        private bool ParametersRefresh { get; set; } = true;
        public SpectrumAnalyzerParameters Data { get; } = new SpectrumAnalyzerParameters();
        

        public void SendData()
        {
            var rand = new Random();
            var result = new List<double>();
            for (int i = 0; i < 100; ++i)
            {
                result.Add(rand.NextDouble() * _span);
            }

            Data.Frequencies = result;
            if (ParametersRefresh)
            {
                Data.RBW = _rbw;
                Data.Span = _span;
                Data.VBW = _rbw;
                Data.CenterFrequency = _span + 100;
            }                        
        }

        private async Task SendLoop()
        {
            while(true)
            {
                SendData();
                await Task.Delay((int)RefreshRate).ConfigureAwait(false);
            }
        }

        public void Connect(string ip, int port)
        {
            _rbw = Convert.ToDouble(ip.Split('.')[0]);
            _span = port == 0 ? 5555 : port;

            _ = Task.Run(SendLoop);
        }

        public async Task SetParameters(SpectrumAnalyzerParameters spectrumAnalyzerParameters)
        {
            _logger.Debug(spectrumAnalyzerParameters.CenterFrequency + " " + spectrumAnalyzerParameters.Span + " " + spectrumAnalyzerParameters.VBW + " " + spectrumAnalyzerParameters.RBW);
            await Task.Delay(2000);            
        }

        public void Dispose()
        {
            
        }

        public void SetRefreshRate(double rate)
        {
            RefreshRate = rate;
        }

        public void RefreshParameters()
        {
            ParametersRefresh = true;
        }
    }
}

using FamoNET.Model;
using FamoNET.Model.Args;
using FamoNET.Model.Interfaces;
using NLog;

namespace FamoNET.Controllers.Mock
{
    public class MockFC1000Controller : IFC1000Controller
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private double _span;
        private double _rbw;

        public event EventHandler<SpectrumAnalyzerEventArgs> DataReceived;

        public Task<(List<double> data, double rbw, double center, double span, double vbw)> GetData()
        {
            var rand = new Random();
            var result = new List<double>();
            for (int i = 0; i < 1000; ++i)
            {
                result.Add(rand.NextDouble() * _span);
            }

            return Task.FromResult((result, _rbw, _span+100, _span, _rbw));
        }

        public void Initialize(string ip, int port)
        {
            _rbw = Convert.ToDouble(ip);
            _span = port;
        }

        public Task SetParameters(SpectrumAnalyzerParameters spectrumAnalyzerParameters)
        {
            _logger.Debug(spectrumAnalyzerParameters.CenterFrequency + " " + spectrumAnalyzerParameters.Span + " " + spectrumAnalyzerParameters.VBW + " " + spectrumAnalyzerParameters.RBW);
            return Task.CompletedTask;
        }
    }
}

using FamoNET.Model.Interfaces;
using NLog;
using System.Net.Sockets;

namespace FamoNET.Controllers.Mock
{
    public class MockFC1000Controller : IFC1000Controller
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private double _span;
        private double _rbw;
        public Task<(List<double> data, double rbw, double center, double span)> GetData()
        {
            var rand = new Random();
            var result = new List<double>();
            for (int i = 0; i < 1000; ++i)
            {
                result.Add(rand.NextDouble() * _span);
            }

            return Task.FromResult((result, _rbw, _span+100, _span));
        }

        public void Initialize(string ip, int port)
        {
            _rbw = Convert.ToDouble(ip);
            _span = port;
        }

        public Task SetBandwidth(double bandwidth)
        {
            _logger.Debug($"Set bandwidth: {bandwidth}");
            return Task.CompletedTask;
        }

        public Task SetCenterFrequency(double frequency)
        {
            _logger.Debug($"Set center frequency: {frequency}");
            return Task.CompletedTask;
        }

        public Task SetFrequencySpan(double span)
        {
            _logger.Debug($"Set span: {span}");
            return Task.CompletedTask;
        }
    }
}

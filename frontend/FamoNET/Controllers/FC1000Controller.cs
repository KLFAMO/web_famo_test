using FamoNET.Model;
using FamoNET.Model.Args;
using FamoNET.Model.Interfaces;
using NLog;
using System;
using System.Net.Sockets;
using System.Reflection.PortableExecutable;
using System.Text;

namespace FamoNET.Controllers
{
    public class FC1000Controller : IFC1000Controller
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private string _ip;
        private int _port;
        private CancellationTokenSource _cancellationTokenSource;        
        public event EventHandler<SpectrumAnalyzerEventArgs> DataReceived;
        
        public void Initialize(string ip, int port)
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
            }
            _cancellationTokenSource = new CancellationTokenSource();

            _ip = ip;
            _port = port;            
            
            _ = StartReading();
        }

        private async Task StartReading()
        {
            try
            {
                using TcpClient tcpClient = new TcpClient();
                tcpClient.Connect(_ip, _port);

                using NetworkStream stream = tcpClient.GetStream();
                using StreamReader streamReader = new StreamReader(stream);

                while (!_cancellationTokenSource.IsCancellationRequested)
                {
                    await SendCommand(stream, "TRAC:DATA?");                    
                    double[] frequencies = ParseData(await streamReader.ReadLineAsync());                    

                    await SendCommand(stream, "FREQuency:CENTer?");                    
                    double centerFreq = ParseData(await streamReader.ReadLineAsync())[0];

                    await SendCommand(stream, "FREQuency:SPAN?");                    
                    double span = ParseData(await streamReader.ReadLineAsync())[0];

                    await SendCommand(stream, "BANDwidth?");                    
                    double rbw = ParseData(await streamReader.ReadLineAsync())[0];

                    await SendCommand(stream, "BANDwidth:VIDeo?");                    
                    double vbw = ParseData(await streamReader.ReadLineAsync())[0];
                    
                    DataReceived?.Invoke(this, new SpectrumAnalyzerEventArgs(new SpectrumAnalyzerParameters
                    {
                        Frequencies = new List<double>(frequencies),
                        CenterFrequency = centerFreq,
                        RBW = rbw,
                        Span = span,
                        VBW = vbw
                    }));

                    await Task.Delay(1000).ConfigureAwait(false);
                }
            }
            catch(Exception ex)
            {
                if (ex is TaskCanceledException)
                {
                    _logger.Debug("SpectrumAnalyzer read task exit");
                }
                else
                {
                    _logger.Error(ex);
                }
            }
        }       

        public async Task SetParameters(SpectrumAnalyzerParameters spectrumAnalyzerParameters)
        {
            using TcpClient tcpClient = new TcpClient();            
            tcpClient.Connect(_ip, _port);

            using NetworkStream stream = tcpClient.GetStream();

            await SendCommand(stream, $"FREQuency:CENTer {spectrumAnalyzerParameters.CenterFrequency}HZ");            
            await SendCommand(stream, $"FREQuency:SPAN {spectrumAnalyzerParameters.Span}HZ");            
            await SendCommand(stream, $"BANDwidth {spectrumAnalyzerParameters.RBW}HZ");            
            await SendCommand(stream, $"BANDwidth:VIDeo {spectrumAnalyzerParameters.VBW}HZ");            
        }

             
        private async Task SendCommand(NetworkStream stream, string command)
        {
            byte[] cmdBytes = Encoding.ASCII.GetBytes(command + "\n");
            await stream.WriteAsync(cmdBytes, 0, cmdBytes.Length);
        }       

        private double[] ParseData(string response)
        {
            string[] parts = response.Split(',');
            double[] values = new double[parts.Length];

            for (int i = 0; i < parts.Length; i++)
            {
                if (!double.TryParse(parts[i], out values[i]))
                {
                    values[i] = double.NaN;
                }
            }

            return values;
        }        
    }
}

using FamoNET.Model;
using FamoNET.Model.Interfaces;
using System.Net.Sockets;

namespace FamoNET.Controllers
{
    public class FC1000Controller : SpectrumAnalyzerControllerBase, ISpectrumAnalyzerController
    {
        public double RefreshRate { get; set; } = 2000;
        private bool ParametersRefresh = true;
        public SpectrumAnalyzerParameters Data { get; private set; } = new SpectrumAnalyzerParameters();
        private SemaphoreSlim _communicationSemaphore = new SemaphoreSlim(1);


        public FC1000Controller()
        {
            _logger.Debug("Controller created");
        }

        public void Connect(string ip, int port)
        {
            if (CancellationTokenSource != null)
            {
                CancellationTokenSource.Cancel();
            }
            CancellationTokenSource = new CancellationTokenSource();

            IP = ip;
            Port = port;            
            
            _ = Task.Run(StartReading);
        }

        private async Task StartReading()
        {
            try
            {
                using TcpClient tcpClient = new TcpClient();
                tcpClient.Connect(IP, Port);

                using NetworkStream stream = tcpClient.GetStream();
                using StreamReader streamReader = new StreamReader(stream);

                //await SendCommand(stream, "INIT:CONT OFF");
                while (!CancellationTokenSource.IsCancellationRequested)
                {                    
                    try
                    {
                        await _communicationSemaphore.WaitAsync();
                        //_logger.Trace("Read start, sending OPC");
                        //await SendCommand(stream, "*OPC?");                        
                        //var opcResult = await streamReader.ReadLineAsync();
                        //_logger.Warn("OPC received");
                        //if (!opcResult.Contains("1"))
                        //    _logger.Warn("Wrong OPC response from device");

                        await SendCommand(stream, "TRAC:DATA?");
                        double[] frequencies = ParseData(await streamReader.ReadLineAsync());

                        if (ParametersRefresh)
                        {
                            await SendCommand(stream, "FREQuency:CENTer?");
                            double centerFreq = ParseData(await streamReader.ReadLineAsync())[0];

                            await SendCommand(stream, "FREQuency:SPAN?");
                            double span = ParseData(await streamReader.ReadLineAsync())[0];

                            await SendCommand(stream, "BANDwidth?");
                            double rbw = ParseData(await streamReader.ReadLineAsync())[0];

                            await SendCommand(stream, "BANDwidth:VIDeo?");
                            double vbw = ParseData(await streamReader.ReadLineAsync())[0];

                            Data.RBW = rbw;
                            Data.VBW = vbw;
                            Data.Span = span;
                            Data.CenterFrequency = centerFreq;

                            ParametersRefresh = false;
                        }

                        //Data.Frequencies = new List<double>() { 1,2,3,4 };
                        if (!frequencies.Contains(double.NaN) && frequencies.Count() > 0)
                        {
                            Data.Frequencies = frequencies.ToList();
                        }

                    }
                    catch(Exception ex)
                    {
                        _logger.Error(ex);
                    }
                    finally
                    {
                        _logger.Trace("Read end");
                        if (_communicationSemaphore.CurrentCount == 0)
                            _communicationSemaphore.Release();

                        await Task.Delay((int)RefreshRate).ConfigureAwait(false);
                    }                                                                                                                       
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

        public void RefreshParameters()
        {
            ParametersRefresh = true;
        }

        public async Task SetParameters(SpectrumAnalyzerParameters spectrumAnalyzerParameters)
        {
            try
            {
                await _communicationSemaphore.WaitAsync();
                _logger.Trace("Send start");
                using TcpClient tcpClient = new TcpClient();
                tcpClient.Connect(IP, Port);
                tcpClient.ReceiveTimeout = 5000;

                using NetworkStream stream = tcpClient.GetStream();
                using StreamReader streamReader = new StreamReader(stream);

                string scpiCommand = $@"FREQuency:CENTer {spectrumAnalyzerParameters.CenterFrequency}HZ;" +
                    $"SPAN {spectrumAnalyzerParameters.Span}HZ;" +
                    $":BANDwidth {spectrumAnalyzerParameters.RBW}HZ;" +
                    $":BANDwidth:VIDeo {spectrumAnalyzerParameters.VBW}HZ;*OPC?"; 
                
                await SendCommand(stream, scpiCommand);
                var opcResult = await streamReader.ReadLineAsync();
                

                if (!opcResult.Contains("1"))
                    _logger.Warn("Wrong OPC response from device (send)");

                ParametersRefresh = true;
            }
            catch(Exception ex)
            {
                _logger.Error(ex);
            }
            finally
            {
                _logger.Trace("Send end");
                if (_communicationSemaphore.CurrentCount == 0)
                    _communicationSemaphore.Release();
            }            
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

        public void SetRefreshRate(double rate)
        {
            RefreshRate = rate;
        }

        public override void Dispose()
        {
            base.Dispose();

            using TcpClient tcpClient = new TcpClient();
            tcpClient.Connect(IP, Port);

            using NetworkStream stream = tcpClient.GetStream();
            using StreamReader streamReader = new StreamReader(stream);

            SendCommand(stream, "INIT:CONT ON").Wait();
        }
    }
}

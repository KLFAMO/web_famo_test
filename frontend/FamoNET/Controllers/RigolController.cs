using FamoNET.Model;
using FamoNET.Model.Args;
using FamoNET.Model.Interfaces;
using System.Net.Sockets;

namespace FamoNET.Controllers
{
    public class RigolController : SpectrumAnalyzerControllerBase, ISpectrumAnalyzerController
    {
        public double RefreshRate { get; set; } = 2000;
        private bool ParametersRefresh = true;
        public SpectrumAnalyzerParameters Data { get; private set; } = new SpectrumAnalyzerParameters();
        private SemaphoreSlim _communicationSemaphore = new SemaphoreSlim(1);

        public void Connect(string ip, int port)
        {
            if (CancellationTokenSource != null)
            {
                CancellationTokenSource.Cancel();
            }
            CancellationTokenSource = new CancellationTokenSource();

            IP = ip;
            Port = port;

            _ = StartReading();
        }

        private async Task StartReading()
        {
            try
            {
                using TcpClient tcpClient = new TcpClient();
                tcpClient.Connect(IP, Port);

                using NetworkStream stream = tcpClient.GetStream();
                using StreamReader streamReader = new StreamReader(stream);

                while (!CancellationTokenSource.IsCancellationRequested)
                {
                    try
                    {
                        await _communicationSemaphore.WaitAsync();
                        
                        await SendCommand(stream, "TRACe:DATA? TRACE1");
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
                        _communicationSemaphore.Release();
                        await Task.Delay((int)RefreshRate).ConfigureAwait(false);
                    }                                                                                
                }
            }
            catch (Exception ex)
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
            try
            {
                await _communicationSemaphore.WaitAsync();

                using TcpClient tcpClient = new TcpClient();
                tcpClient.Connect(IP, Port);

                using NetworkStream stream = tcpClient.GetStream();

                await SendCommand(stream, $"FREQuency:CENTer {spectrumAnalyzerParameters.CenterFrequency}");
                await stream.FlushAsync();

                await SendCommand(stream, $"FREQuency:SPAN {spectrumAnalyzerParameters.Span}");
                await stream.FlushAsync();

                await SendCommand(stream, $"BANDwidth {spectrumAnalyzerParameters.RBW}");
                await stream.FlushAsync();

                await SendCommand(stream, $"BANDwidth:VIDeo {spectrumAnalyzerParameters.VBW}");
                await stream.FlushAsync();
            }
            catch(Exception ex)
            {
                _logger.Error(ex);
            }
            finally
            {
                RefreshParameters();
                _communicationSemaphore.Release();
            }            
        }

        public void SetRefreshRate(double rate)
        {
            RefreshRate = rate;
        }

        public void RefreshParameters()
        {
            ParametersRefresh = true;
        }

        private double[] ParseData(string response)
        {
            string[] parts = response.Split(',');

            if (parts.Length > 1) //that means it's data
            {
                parts = parts.Skip(1).ToArray(); //there is a trash value at the beginning
            }

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

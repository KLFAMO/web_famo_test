using FamoNET.Controllers;
using FamoNET.Model;
using FamoNET.Model.Interfaces;
using System.Net.Sockets;
using System.Text;

namespace FamoNET.Factories
{
    public class RemoteChartsDeviceFactory : IRemoteChartsDeviceFactory
    {
        private ISpectrumAnalyzerController _spectrumAnalyzerController;        

        public async Task<ISpectrumAnalyzerController> Create(string ip, int port)
        {
            var identifier = await GetIdentifier(ip, port);

            if (identifier.Contains("Rigol"))
            {
                _spectrumAnalyzerController = new RigolController();
            }
            else if (identifier.Contains("Rohde"))
            {
                _spectrumAnalyzerController = new FC1000Controller();
            }
            else
            {
                throw new NotSupportedException("Unknown device");
            }
            
            _spectrumAnalyzerController.Connect(ip, port);

            return _spectrumAnalyzerController;
        }       

        public void Dispose()
        {                
            _spectrumAnalyzerController?.Dispose();
        }
        
        private async Task<string> GetIdentifier(string ip, int port)
        {
            using TcpClient tcpClient = new TcpClient();
            tcpClient.Connect(ip, port);

            using NetworkStream stream = tcpClient.GetStream();
            using StreamReader streamReader = new StreamReader(stream);

            byte[] cmdBytes = Encoding.ASCII.GetBytes("*IDN?\n");
            await stream.WriteAsync(cmdBytes, 0, cmdBytes.Length);
            return await streamReader.ReadLineAsync();
        }
    }
}

using FamoNET.Model.Interfaces;
using System.Net.Sockets;
using System.Text;

namespace FamoNET.Controllers
{
    public class FC1000Controller : IFC1000Controller
    {
        private string _ip;
        private int _port;

        public void Initialize(string ip, int port)
        {
            _ip = ip;
            _port = port;
        }

        public async Task<(List<double> data, double rbw, double center, double span)> GetData()
        {
            using (TcpClient client = new TcpClient())
            {
                client.Connect(_ip, _port);

                if (!client.Connected)
                {
                    throw new Exception("Failed to connect to the instrument");
                }

                NetworkStream stream = client.GetStream();

                await SendCommand(stream, "TRAC:DATA?");
                string xResponse = ReadResponse(stream);
                double[] frequencies = ParseData(xResponse);

                await SendCommand(stream, "FREQuency:CENTer?");
                xResponse = ReadResponse(stream);
                double centerFreq = ParseData(xResponse)[0];

                await SendCommand(stream, "FREQuency:SPAN?");
                xResponse = ReadResponse(stream);
                double span = ParseData(xResponse)[0];

                await SendCommand(stream, "BANDwidth?");
                xResponse = ReadResponse(stream);
                double rbw = ParseData(xResponse)[0];

                return (new List<double>(frequencies), rbw, centerFreq, span);
            }
        }

        public async Task SetCenterFrequency(double frequency)
        {
            using (TcpClient client = new TcpClient())
            {
                client.Connect(_ip, _port);

                if (!client.Connected)
                {
                    throw new Exception("Failed to connect to the instrument");
                }

                NetworkStream stream = client.GetStream();

                await SendCommand(stream, $"FREQuency:CENTer {frequency}HZ");
            }
        }

        public async Task SetFrequencySpan(double span)
        {
            using (TcpClient client = new TcpClient())
            {
                client.Connect(_ip, _port);

                if (!client.Connected)
                {
                    throw new Exception("Failed to connect to the instrument");
                }

                NetworkStream stream = client.GetStream();

                await SendCommand(stream, $"FREQuency:SPAN {span}HZ");
            }
        }

        public async Task SetBandwidth(double bandwidth)
        {
            using (TcpClient client = new TcpClient())
            {
                client.Connect(_ip, _port);

                if (!client.Connected)
                {
                    throw new Exception("Failed to connect to the instrument");
                }

                NetworkStream stream = client.GetStream();

                await SendCommand(stream, $"BANDwidth {bandwidth}HZ");
            }
        }

        private async Task SendCommand(NetworkStream stream, string command)
        {
            byte[] cmdBytes = Encoding.ASCII.GetBytes(command + "\n");
            await stream.WriteAsync(cmdBytes, 0, cmdBytes.Length);
        }

        private string ReadResponse(NetworkStream stream)
        {
            byte[] buffer = new byte[16384]; // Larger buffer for potentially big data
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            return Encoding.ASCII.GetString(buffer, 0, bytesRead).Trim();
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

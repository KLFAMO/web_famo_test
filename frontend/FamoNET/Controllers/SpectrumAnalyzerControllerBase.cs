using FamoNET.Model.Args;
using NLog;
using System.Net.Sockets;
using System.Text;

namespace FamoNET.Controllers
{
    public abstract class SpectrumAnalyzerControllerBase : IDisposable
    {
        protected static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        protected CancellationTokenSource CancellationTokenSource;
        protected string IP;
        protected int Port;

        public virtual void Dispose()
        {
            CancellationTokenSource?.Cancel();
        }

        protected async Task SendCommand(NetworkStream stream, string command)
        {
            byte[] cmdBytes = Encoding.ASCII.GetBytes(command + "\n");
            await stream.WriteAsync(cmdBytes, 0, cmdBytes.Length);
        }
    }
}

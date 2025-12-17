using FamoNET.Model;
using FamoNET.Model.Args;
using NLog;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace FamoNET.Services.DataServices
{
    public class CounterDataService
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        
        private string _counterUri;
        private bool _isConnectingRequested = false;
                
        private TcpClient _client = new TcpClient();
        
        private CancellationTokenSource CTS = new CancellationTokenSource(); //to do later beacuse I'm not cancelling it anywhere and I should
        
        private CounterWriterService _writerService;   
        
        public bool IsSaving => _writerService != null;

        public CounterDataService(string counterUri)
        {            
            _counterUri = counterUri;
        }

        public int ReceiveBufferSize { get; set; } = 8192;

        public async Task ConnectAsync()
        {
            if (_isConnectingRequested)
                return;
            
            _isConnectingRequested = true;

            try
            {                
                _logger.Info("Connecting to Counter...");
                var splitUri = _counterUri.Split(':');
                await _client.ConnectAsync(IPAddress.Parse(splitUri[0]), Int32.Parse(splitUri[1]));                
                               
                await Task.Factory.StartNew(ReceiveLoop, CTS.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            }
            catch(Exception ex)
            {                
                _client?.Dispose();
                _client = new TcpClient();

                _logger.Info($"{ex} Failed to connect. Retrying in 5 seconds.");
                await Task.Delay(5000);

                _isConnectingRequested = false;
                await ConnectAsync();
            }
            
        }

        private async Task ReceiveLoop()
        {
            await using NetworkStream stream = _client.GetStream();            
            var dataBuffer = new byte[ReceiveBufferSize];
            string messageBuffer = String.Empty;
            List<CounterChannel> _channels = new List<CounterChannel>();            

            try
            {
                await stream.FlushAsync();
                
                while (!CTS.Token.IsCancellationRequested)
                {                    
                    int received = await stream.ReadAsync(dataBuffer);
                    string bufferedMessage = String.Empty;
                    
                    var message = bufferedMessage + Encoding.UTF8.GetString(dataBuffer, 0, received);

                    int i = 0;
                    while(i < message.Length)
                    {
                        if (message[i] == '|')
                        {
                            if (message[i + 1] == '0' && _channels.Count > 0)
                            {
                                ChannelsReceived?.Invoke(this, new ChannelsReceivedEventArgs(_channels));
                                if (_writerService != null)
                                    _writerService.Write(_channels);

                                _channels = new List<CounterChannel>();
                            }

                            int dataEndIndex = -1;
                            try
                            {
                                dataEndIndex = message.IndexOf('|', i + 3);
                            }
                            catch (ArgumentOutOfRangeException)
                            {
                                dataEndIndex = -1;
                            }

                            if (dataEndIndex > -1)
                            {
                                _channels.Add(new CounterChannel((int)Char.GetNumericValue(message[i + 1]), Convert.ToDouble(message.Substring(i + 3, dataEndIndex - (i + 3)))));
                                i = dataEndIndex + 1;
                            }
                            else //end of message
                            {
                                bufferedMessage = message.Substring(i);
                                break;
                            }
                        }
                        else if (bufferedMessage == String.Empty) //start of communication, not full data received
                        { 
                            i = message.IndexOf('|') + 1;
                            _logger.Warn("Skipping value. Doesn't matter if that's on communication start");
                        }
                        else
                        {
                            _logger.Warn($"Unexpected message: {message}");
                        }
                            
                    }                                                                                            
                }
            }
            catch (TaskCanceledException)
            {
                _logger.Info("Cancelled");
            }
            catch (Exception ex)
            {
                _logger.Error($"Unexpected: {ex}");
            }
            finally
            {
                _isConnectingRequested = false;
                _logger.Info("Disposed");                
            }
        }
        
        public void StartSaving(CounterWriterService writerService)
        {
            _writerService = writerService;
        }

        public void StopSaving()
        {
            _writerService.StopSave();
            _writerService = null;
        }

        public event EventHandler<ChannelsReceivedEventArgs> ChannelsReceived;

    }
}

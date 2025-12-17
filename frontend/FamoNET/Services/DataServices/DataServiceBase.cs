using NLog;

namespace FamoNET.Services.DataServices
{
    public abstract class DataServiceBase : IDisposable
    {
        protected readonly static Logger Logger = LogManager.GetCurrentClassLogger();
        protected CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();
        protected readonly HttpClient HttpClient;
        protected DataServiceBase(string endpoint)
        {
            HttpClient = new HttpClient()
            {
                BaseAddress = new Uri(endpoint)
            };

            HttpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36");
        }

        public void Dispose()
        {
            CancellationTokenSource.Cancel();
        }
    }
}

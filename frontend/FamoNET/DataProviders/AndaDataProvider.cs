using FamoNET.Model;
using FamoNET.Model.Interfaces;
using NLog;
using System.Collections.Specialized;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Web;

namespace FamoNET.DataProviders
{
    public class AndaDataProvider : IAndaDataProvider
    {
        private readonly static Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly HttpClient _httpClient;        

        public AndaDataProvider(string counterUri)
        {
            _httpClient = new HttpClient()
            {
                BaseAddress = new Uri(counterUri)
            };

            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36");
        }

        public async Task<List<DataPoint>> GetData(decimal startMjd, decimal endMjd, string tableName)
        {            
            NameValueCollection queryParameters = HttpUtility.ParseQueryString(string.Empty);
            
            queryParameters.Add($"from_mjd", startMjd.ToString());
            queryParameters.Add($"to_mjd", endMjd.ToString());
            queryParameters.Add($"table_name", tableName.ToString());
            
            HttpResponseMessage response = null;
            try
            {
                response = await _httpClient.GetAsync("table_data/?"+queryParameters.ToString(), _cancellationTokenSource.Token);
            }
            catch(Exception ex)
            {
                _logger.Error(ex);
                return null;
            }
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.Error($"Wrong status code: {response.StatusCode}. Address: {_httpClient.BaseAddress}{queryParameters}");
                return null;
            }
            
            try
            {
                var result = new List<DataPoint>();

                JsonNode root = JsonSerializer.Deserialize<JsonNode>(await response.Content.ReadAsStringAsync());
                var mjds = root["data"].AsObject()["mjd"].AsArray();
                var vals = root["data"].AsObject()["val"].AsArray();

                for (int i = 0; i < mjds.Count; ++i)
                {
                    result.Add(new DataPoint(mjds[i].GetValue<double>(), vals[i].GetValue<double>()));
                }

                return result;
            }
            catch(Exception ex)
            {
                _logger.Error(ex);
                throw;
            }
        }

        public async Task<List<DataPoint>> GetData(string query)
        {
            throw new NotImplementedException();
        }

        public async Task<List<string>> GetTableNames()
        {            
            HttpResponseMessage response = null;
            try
            {
                response = await _httpClient.GetAsync("tables_names/", _cancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.Error($"Wrong status code: {response.StatusCode}. Address: {_httpClient.BaseAddress}");
                return null;
            }
            
            try
            {
                var result = JsonSerializer.Deserialize<List<string>>(await response.Content.ReadAsStringAsync());
                if (result == null)
                    throw new Exception("Failed to parse data from API");                

                return result;
            }
            catch(Exception ex)
            {
                _logger.Error(ex);
                throw;
            }
        }        
    }
}

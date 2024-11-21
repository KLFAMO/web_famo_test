using FamoNET.Model;
using FamoNET.Model.Interfaces;
using NLog;
using System.Collections.Specialized;
using System.Text;
using System.Web;

namespace FamoNET.DataProviders
{
    public class CounterDataProvider : ICounterDataProvider
    {
        private readonly static Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly HttpClient _httpClient;        

        public CounterDataProvider(string counterUri)
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
                response = await _httpClient.GetAsync("?"+queryParameters.ToString(), _cancellationTokenSource.Token);
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

            var content = await response.Content.ReadAsStringAsync();
            return ParseHTMLPage(content);
        }

        public async Task<List<DataPoint>> GetData(string query)
        {
            HttpResponseMessage response = null;
            try
            {
                response = await _httpClient.PostAsync("", new StringContent(query, Encoding.UTF8, "text/plain"));
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }


            var content = await response.Content.ReadAsStringAsync();
            return ParseHTMLPage(content);
        }

        public Task<List<string>> GetTableNames()
        {
            throw new NotImplementedException();
        }

        private List<DataPoint> ParseHTMLPage(string content)
        {
            try
            {                
                if (content == null || content.Length < 1)
                    return null;

                var arrayIndex = content.IndexOf('[') + 1; //find opening bracket and skip ' sign

                if (arrayIndex == -1)
                    return null;

                bool isArrayContent = true;
                List<string> x_values = new List<string>();

                string x_temp = String.Empty;
                while (isArrayContent)
                {
                    if (content[arrayIndex] == ']') //json table close
                    {
                        isArrayContent = false;
                        x_values.Add(x_temp);
                    }
                    else if (content[arrayIndex] == ' ')
                    {

                    }
                    else if (content[arrayIndex] == ',')
                    {
                        x_values.Add(x_temp);
                        x_temp = String.Empty;
                    }
                    else
                        x_temp += content[arrayIndex];

                    ++arrayIndex;
                }

                arrayIndex = content.IndexOf('[', arrayIndex) + 1; //find opening bracket and skip ' sign
                isArrayContent = true;
                List<string> y_values = new List<string>();

                string y_temp = String.Empty;

                while (isArrayContent)
                {
                    if (content[arrayIndex] == ']') //json table close
                    {
                        isArrayContent = false;
                        y_values.Add(y_temp);
                    }
                    else if (content[arrayIndex] == ' ')
                    {

                    }
                    else if (content[arrayIndex] == ',')
                    {
                        y_values.Add(y_temp);
                        y_temp = String.Empty;
                    }
                    else
                        y_temp += content[arrayIndex];

                    ++arrayIndex;
                }

                var result = new List<DataPoint>();
                for (int i = 0; i < x_values.Count; i++)
                    result.Add(new DataPoint(Convert.ToDecimal(x_values[i]), Convert.ToDecimal(y_values[i])));

                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return null;
            }
        }
    }
}

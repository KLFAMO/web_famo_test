using FamoNET.Model;
using FamoNET.Model.Dto;
using FamoNET.Model.Interfaces;
using Microsoft.AspNetCore.Http.Extensions;
using System.Text;
using System.Text.Json;

namespace FamoNET.Services.DataServices
{
    public class LabbookDataService : DataServiceBase, ILabbookDataService
    {
        public LabbookDataService(string endpoint) : base(endpoint)
        {
        }

        public async Task<LabbookEntry> GetByIdAsync(int id)
        {
            HttpResponseMessage response = null;
            try
            {
                response = await HttpClient.GetAsync($"?id={id}");
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                throw;
            }

            if (!response.IsSuccessStatusCode)
            {
                Logger.Error($"Wrong status code: {response.StatusCode}. Address: {HttpClient.BaseAddress}");
                return null;
            }

            try
            {
                var result = JsonSerializer.Deserialize<LabbookEntryDTO>(await response.Content.ReadAsStringAsync());
                if (result == null)
                    throw new Exception("Failed to parse data from API");

                return new LabbookEntry(result);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                throw;
            }
        }

        public async Task<List<LabbookEntry>> GetEntriesAsync(double from, double to)
        {
            QueryBuilder keyValuePairs = new QueryBuilder();
            if (from > 0 && to > 0)
            {
                keyValuePairs.Add("from_mjd", from.ToString());
                keyValuePairs.Add("to_mjd", to.ToString());
            }
            HttpResponseMessage response = null;
            try
            {                
                response = await HttpClient.GetAsync(keyValuePairs.ToString(), CancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                throw;
            }

            if (!response.IsSuccessStatusCode)
            {
                Logger.Error($"Wrong status code: {response.StatusCode}. Address: {HttpClient.BaseAddress}");
                return null;
            }

            try
            {
                var result = JsonSerializer.Deserialize<List<LabbookEntryDTO>>(await response.Content.ReadAsStringAsync());
                if (result == null)
                    throw new Exception("Failed to parse data from API");

                return result.Select(l => new LabbookEntry(l)).ToList();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                throw;
            }
        }

        public async Task<bool> PostAsync(LabbookEntry entry)
        {
            var content = new StringContent(JsonSerializer.Serialize(new LabbookEntryDTO(entry)), Encoding.UTF8, "application/json");

            HttpResponseMessage response = null;
            try
            {
                response = await HttpClient.PostAsync("", content);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    Logger.Error(string.Concat(response.StatusCode.ToString(), " ", responseContent));
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                throw;
            }            
        }

        public async Task<bool> UpdateAsync(LabbookEntry entry)
        {
            var content = new StringContent(JsonSerializer.Serialize(entry), Encoding.UTF8, "application/json");

            HttpResponseMessage response = null;
            try
            {
                response = await HttpClient.PutAsync("", content);
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                throw;
            }            
        }
    }
}

using FamoNET.Model.Interfaces;
using System.Text.Json;

namespace FamoNET.Services.DataServices.Mock
{
    public class MockFreqMonitorDataService : IFreqMonitorDataService
    {
        private const string _json = "{\"counters_comb2\": [19973650.9019852, 35000000.032465, 34999999.9683828, 48450250.7233181, 46959760.0201073, 44411122.3736553, 65055498.5762348, 33188701.450922, 249993412.7254963, 35000000.032465]}";
        private int index = 0;
        public Task<List<double>> GetValues()
        {
            var values = JsonSerializer.Deserialize<List<double>>(_json.Substring(18).TrimEnd('}'));
            for (int i = 0; i < values.Count; ++i)
                values[i] += index;
            ++index;

            return Task.FromResult(values);
        }
    }
}

using FamoNET.Model;
using FamoNET.Model.Interfaces;

namespace FamoNET.Services.DataServices
{
    public class AndaDataService
    {
        private IAndaDataProvider _dataProvider;

        public AndaDataService(IAndaDataProvider dataProvider)
        {
            _dataProvider = dataProvider;
        }        

        public async Task<List<DataPoint<double>>> SendPythonAsync(string cmd)
        {                  
            return await _dataProvider.GetData(cmd);
        }

        public async Task<List<string>> GetTableNamesAsync()
        {
            return await _dataProvider.GetTableNames();
        }

        public async Task<List<DataPoint<double>>> GetDataAsync(double startMjd, double endMjd, string tableName)
        {
            return await _dataProvider.GetData(startMjd, endMjd, tableName);
        }
    }
}

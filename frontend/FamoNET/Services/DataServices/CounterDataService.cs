﻿using FamoNET.Model;
using FamoNET.Model.Interfaces;

namespace FamoNET.Services.DataServices
{
    public class CounterDataService
    {
        private ICounterDataProvider _dataProvider;

        public CounterDataService(ICounterDataProvider dataProvider)
        {
            _dataProvider = dataProvider;
        }        

        public async Task<List<DataPoint>> SendPythonAsync(string cmd)
        {                  
            return await _dataProvider.GetData(cmd);
        }

        public async Task<List<string>> GetTableNamesAsync()
        {
            return await _dataProvider.GetTableNames();
        }

        public async Task<List<DataPoint>> GetDataAsync(decimal startMjd, decimal endMjd, string tableName)
        {
            return await _dataProvider.GetData(startMjd, endMjd, tableName);
        }
    }
}

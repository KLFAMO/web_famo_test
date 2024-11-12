using FamoNET.Model;
using FamoNET.Model.Interfaces;
using Microsoft.VisualBasic.FileIO;
using NLog;

namespace FamoNET.DataProviders
{
    public class MockDataProvider : ICounterDataProvider, ICSVDataProvider
    {
        private readonly string _csvPath;
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        
        public MockDataProvider(string csvPath)
        {
            _csvPath = csvPath;
        }

        public async Task<List<DataPoint>> LoadCSV()
        {
            await Task.Delay(2000);
            var resultData = new List<DataPoint>();

            using (TextFieldParser parser = new TextFieldParser(_csvPath))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");

                //skip first row
                parser.ReadFields();

                while (!parser.EndOfData)
                {
                    //Processing row
                    string[] fields = parser.ReadFields();
                    try
                    {
                        resultData.Add(new DataPoint(Convert.ToDecimal(fields[0]), Convert.ToDecimal(fields[1])));                        
                    }
                    catch (Exception ex) 
                    {
                        _logger.Error(ex);
                    }
                }
            }

            return resultData;
        }     

        public async Task<List<DataPoint>> GetData(string query)
        {
            await Task.Delay(2000);
            var resultData = new List<DataPoint>();

            using (TextFieldParser parser = new TextFieldParser(_csvPath))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");

                //skip first row
                parser.ReadFields();

                while (!parser.EndOfData)
                {
                    //Processing row
                    string[] fields = parser.ReadFields();
                    try
                    {
                        resultData.Add(new DataPoint(Convert.ToDecimal(fields[0]), Convert.ToDecimal(fields[1])));
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex);
                    }
                }
            }

            return resultData;
        }

        public async Task<List<DataPoint>> GetData(decimal startMjd, decimal endMjd, string tableName)
        {
            await Task.Delay(2000);
            var resultData = new List<DataPoint>();

            using (TextFieldParser parser = new TextFieldParser(_csvPath))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");

                //skip first row
                parser.ReadFields();

                while (!parser.EndOfData)
                {
                    //Processing row
                    string[] fields = parser.ReadFields();
                    try
                    {
                        var mjd = Convert.ToDecimal(fields[0]);
                        if (mjd < startMjd || mjd > endMjd)
                            continue;

                        resultData.Add(new DataPoint(mjd, Convert.ToDecimal(fields[1])));
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex);
                    }
                }
            }

            return resultData;
        }

        public async Task<List<string>> GetTableNames()
        {
            await Task.Delay(2000);
            return new List<string>() { "Table1", "Table2", "Table3", "Table4" };
        }
    }
}

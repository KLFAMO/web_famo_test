﻿using FamoNET.Model;
using FamoNET.Model.Interfaces;
using Microsoft.VisualBasic.FileIO;
using NLog;

namespace FamoNET.DataProviders
{
    public class MockDataProvider : IAndaDataProvider, ICSVDataProvider
    {
        private readonly string _csvPath;
        private readonly string _htmlPage = @"<!DOCTYPE html>
<html lang=""en"">
    <head>
        <meta charset=""UTF-8"">
        <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
        <title>Document</title>
        <script src=""https://cdn.plot.ly/plotly-2.20.0.min.js""></script>
        <script>
            function update_data() {
                plot = document.getElementById('data_plot');
                var plot_data = [{
                    x: x_tab,
                    y: y_tab,
                    type: 'scatter'
                }]";
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

        public Task<List<DataPoint>> GetData(decimal startMjd, decimal endMjd, string tableName)
        {
            return Task.FromResult(ParseHTMLPage());
        }

        public async Task<List<string>> GetTableNames()
        {
            await Task.Delay(2000);
            return new List<string>() { "Table1", "Table2", "Table3", "Table4" };
        }


        private List<DataPoint> ParseHTMLPage()
        {
            try
            {
                string content = _htmlPage;

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
            catch(Exception ex)
            {
                _logger.Error(ex);
                return null;
            }            
        }
    }
}
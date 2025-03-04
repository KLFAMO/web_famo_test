using FamoNET.Components.SubComponents;
using FamoNET.Model;
using Microsoft.JSInterop;

namespace FamoNET.Services
{
    public class ChartManagerService : IDisposable
    {
        private bool _isInitialized = false;

        private IJSObjectReference _module;
        private DotNetObjectReference<AllanVariance> objRef;
        private IJSRuntime _jsRuntime;

        public ChartManagerService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task InitializeService<T>(DotNetObjectReference<T> dotNetObjectReference) where T : class
        {            
            _module = await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "./canvas/interop.js");
            await _module.InvokeVoidAsync("SetDotNetReference", new object[1] { dotNetObjectReference });            
            _isInitialized = true;
        }

        public async Task InitializeChart(string title = "New chart", bool logarithmic = false)
        {
            if (!_isInitialized)
                throw new Exception("Service is not initialized!");

            await _module.InvokeAsync<string>("InitializeChart", title, logarithmic);
        }

        public async Task AddDataSet(List<DataPoint> dataPoints, bool instantRender = true)
        {
            if (!_isInitialized)
                throw new Exception("Service is not initialized!");

            await _module.InvokeVoidAsync("AddDataSet", dataPoints);
            if (instantRender)
                await Render();
        }

        public async Task Unzoom(bool instantRender = true)
        {
            if (!_isInitialized)
                throw new Exception("Service is not initialized!");

            await _module.InvokeVoidAsync("Unzoom");
            if (instantRender)            
                await Render();                
            
        }

        public async Task ClearDataSets(bool instantRender = true)
        {
            if (!_isInitialized)
                throw new Exception("Service is not initialized!");

            await _module.InvokeVoidAsync("ClearDataSets", CancellationToken.None);
            if (instantRender)
                await Render();
        }

        public async Task<ChartParams> GetViewportParameters()
        {
            var limits = await _module.InvokeAsync<List<decimal>>("GetViewportParameters");
            if (limits != null)
                return new ChartParams() { MinX = limits[0], MaxX = limits[1], MinY = limits[2], MaxY = limits[3] };
            else
                throw new Exception("Failed to read chart parameters");                            
        }

        public async Task SetChartParameters(ChartParams chartParams, bool instantRender = true)
        {
            if (!_isInitialized)
                throw new Exception("Service is not initialized!");

            await _module.InvokeVoidAsync("SetChartParameters", chartParams);
            if (instantRender) 
                await Render();
        }

        public async Task Render()
        {
            if (!_isInitialized)
                throw new Exception("Service is not initialized!");

            await _module.InvokeVoidAsync("RenderChart", CancellationToken.None);
        }

        public async Task AdjustToVisible(bool instantRender = true)
        {
            if (!_isInitialized)
                throw new Exception("Service is not initialized!");
            
            await _module.InvokeVoidAsync("AdjustToVisibleData");
            
            if (instantRender)
                await Render();
        }

        public async Task ConvertToMjd(bool instantRender = true)
        {
            if (!_isInitialized)
                throw new Exception("Service is not initialized!");

            await _module.InvokeVoidAsync("ConvertToMjd");

            if (instantRender) 
                await Render();
        }

        public async Task ConvertToDate(bool instantRender = true)
        {
            if (!_isInitialized)
                throw new Exception("Service is not initialized!");

            await _module.InvokeVoidAsync("ConvertToDate");
            
            if (instantRender)
                await Render();
        }

        public async Task InvokeAsync(string commandName, bool instantRender = true)
        {
            if (!_isInitialized)
                throw new Exception("Service is not initialized!");

            await _module.InvokeVoidAsync(commandName);
            if (instantRender)
                await Render();
        }

        public void Dispose()
        {
            objRef?.Dispose();
        }
    }
}

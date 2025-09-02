using FamoNET.Model;
using FamoNET.Model.Args;
using FamoNET.Utils;
using Microsoft.JSInterop;
using NLog;
using System.ComponentModel;
using System.Text.Json;

namespace FamoNET.Services
{
    public class ChartManagerService
    {   
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private IJSObjectReference _module;        
        private IJSRuntime _jsRuntime;

        public ChartManagerService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;         
        }                

        public async Task InitializeChart<T>(Guid containerGuid, ChartParameters<T> chartParameters)
        {
            _module = await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "./canvas/interop.js");
            await _module.InvokeVoidAsync("SetDotNetReference", containerGuid.ToString(), DotNetObjectReference.Create(this) );
            await _module.InvokeAsync<string>("InitializeChart", containerGuid.ToString(), chartParameters);
        }

        public async Task AddDataSet<T>(Guid containerGuid, List<DataPoint<T>> dataPoints, bool instantRender = true)
        {                        
            await _module.InvokeVoidAsync("AddDataSet", containerGuid.ToString(), dataPoints);
            if (instantRender)
                await Render(containerGuid);
        }       

        public async Task ClearDataSets(Guid containerGuid, bool instantRender = true)
        {            
            await _module.InvokeVoidAsync("ClearDataSets", CancellationToken.None, containerGuid.ToString());
            if (instantRender)
                await Render(containerGuid);
        }        

        public async Task SetChartParameters<T>(Guid containerGuid, ChartParameters<T> chartParams, bool instantRender = true)
        {            
            await _module.InvokeVoidAsync("SetChartParameters", containerGuid.ToString(), chartParams);
            if (instantRender) 
                await Render(containerGuid);
        }

        public async Task<ViewportParams<double>> GetViewportParameters(Guid containerGuid)
        {
            var values = await _module.InvokeAsync<List<JsonElement>>("GetViewportParameters", containerGuid.ToString());

            if ((AxisMode)values[4].GetInt32() == AxisMode.Date)
            {
                //return new ViewportParams<double>() { MinX = DateTime.Parse(values[0].GetString()).Ticks, MaxX = DateTime.Parse(values[1].GetString()).Ticks, MinY = values[2].GetDouble(), MaxY = values[3].GetDouble(), AxisMode = (AxisMode)values[4].GetInt32() };
                return new ViewportParams<double>() 
                { 
                    MinX = FamoMath.Convert_DateTimeToMjd(DateTimeOffset.FromUnixTimeMilliseconds(values[0].GetInt64()).UtcDateTime), 
                    MaxX = FamoMath.Convert_DateTimeToMjd(DateTimeOffset.FromUnixTimeMilliseconds(values[1].GetInt64()).UtcDateTime),
                    MinY = values[2].GetDouble(), 
                    MaxY = values[3].GetDouble(), 
                    AxisMode = (AxisMode)values[4].GetInt32() };
            }
            else
            {
                return new ViewportParams<double>() { MinX = values[0].GetDouble(), MaxX = values[1].GetDouble(), MinY = values[2].GetDouble(), MaxY = values[3].GetDouble(), AxisMode = (AxisMode)values[4].GetInt32() };
            }
            
        }

        public async Task SetViewportParameters<T>(Guid containerGuid, ViewportParams<T> viewportParams)
        {            
            var interopVP = new ViewportParams<double>();
            if (viewportParams.AxisMode == AxisMode.Date)
            {
                var source = viewportParams as ViewportParams<DateTime>;
                interopVP.MinX = new DateTimeOffset(source.MinX).ToUnixTimeMilliseconds();
                interopVP.MaxX = new DateTimeOffset(source.MaxX).ToUnixTimeMilliseconds();
                interopVP.MinY = source.MinY;
                interopVP.MaxY = source.MaxY;
                interopVP.AxisMode = AxisMode.Date;
            }
            else
            {
                interopVP = viewportParams as ViewportParams<double>;
            }
            await _module.InvokeVoidAsync("SetViewportParameters", containerGuid.ToString(), interopVP);
            //Render is implemented in interop
        }

        public async Task Render(Guid containerGuid)
        {            
            await _module.InvokeVoidAsync("RenderChart", CancellationToken.None, containerGuid.ToString());
        }

        public async Task AdjustToVisible(Guid containerGuid, bool instantRender = true)
        {            
            await _module.InvokeVoidAsync("AdjustToVisibleData", containerGuid.ToString());
            
            if (instantRender)
                await Render(containerGuid);
        }       
        
        public async Task InvokeAsync(Guid containerGuid, string commandName, bool instantRender = true)
        {            
            await _module.InvokeVoidAsync(commandName, containerGuid.ToString());
            if (instantRender)
                await Render(containerGuid);
        }

        public async Task ResetViewport(Guid containerGuid, bool instantRender = true)
        {            
            await _module.InvokeVoidAsync("ResetViewport", CancellationToken.None, containerGuid.ToString());
            if (instantRender)
                await Render(containerGuid);
        }

        public async Task<ViewportParams<double>> PopPreviousViewport(Guid containerGuid, bool instantRender = true)
        {            
            try
            {
                var limits = await _module.InvokeAsync<List<double>>("PopPreviousViewport", containerGuid.ToString());
                return new ViewportParams<double>() { MinX = limits[0], MaxX = limits[1], MinY = Convert.ToDouble(limits[2]), MaxY = Convert.ToDouble(limits[3]), AxisMode = (AxisMode)limits[4] };                                
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task DisposeChart(Guid containerGuid)
        {
            try
            {
                if (_module == null)
                {
                    return;
                }

                await _module.InvokeVoidAsync("Dispose", containerGuid.ToString());
            }
            catch (JSDisconnectedException)
            {
                // JS runtime is gone — skip cleanup
            }
            catch (ObjectDisposedException)
            {
                // .NET object is already gone — ignore
                _logger.Debug("JS Module already disposed. Ignoring interop call");
            }

        }

        public event EventHandler<EventArgs> OnViewportChanged;
        

        [JSInvokable]
        public void ViewportChanged(string guid, JsonElement minX, JsonElement maxX, JsonElement minY, JsonElement maxY, int axisMode)
        {            
            if (axisMode == (int)AxisMode.Mjd || axisMode == (int)AxisMode.Offset)
            {
                OnViewportChanged?.Invoke(this, new MjdViewportEventArgs(
                    Guid.Parse(guid), 
                    new ViewportParams<double>() { MinX = minX.GetDouble(), MaxX = maxX.GetDouble(), MinY = minY.GetDouble(), MaxY = maxY.GetDouble(), AxisMode = AxisMode.Mjd }));
            }
            else if (axisMode == (int)AxisMode.Date)
            {
                OnViewportChanged?.Invoke(this, new DateViewportEventArgs(
                    Guid.Parse(guid),
                    new ViewportParams<DateTime>() { MinX = DateTime.Parse(minX.GetString()), MaxX = DateTime.Parse(maxX.GetString()), MinY = minY.GetDouble(), MaxY = maxY.GetDouble(), AxisMode = AxisMode.Date }));
            }            
        }        
    }
}

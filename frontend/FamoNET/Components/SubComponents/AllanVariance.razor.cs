using FamoNET.Components.SubComponents.Chart;
using FamoNET.Model;
using NLog;

namespace FamoNET.Components.SubComponents
{
    public partial class AllanVariance : ChartComponentBase<double>
    {                
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            if (firstRender)
            {
                Logger.Debug($"Allan guid: {ChartGuid}");
                await Initialize(new ChartParameters<double>() { Title = "Allan deviation", Logarithmic = true, AxisMode=AxisMode.Mjd, DisableEvents = true });
            }            
        }

        public override async Task LoadData(List<DataPoint<double>> data)
        {
            var allanData = CalculateAllan(data);

            await ChartManagerService.AddDataSet(ChartGuid, allanData);
            await ChartManagerService.ResetViewport(ChartGuid);           
        }

        private List<DataPoint<double>> CalculateAllan(List<DataPoint<double>> dataPoints)
        {            
            if (dataPoints.Count < 2)
                return null;            

            var frequencies = dataPoints.Select(dp => dp.Y).ToList();

            var dt = 0.01;
            var data = new List<DataPoint<double>>();
            
            for(double tau = 0.1; tau < frequencies.Count/2; tau*=1.5)
            {
                var n = (int)(tau / dt);                
                var M = (int)(frequencies.Count / n);
                if (M < 2)
                    continue;

                
                var y = new List<double>();
                for (int i=0; i < M-1; ++i)
                {                    
                    //ySum += Math.Pow(frequency.Slice((int)(i * n), n).Average(), 2);                    
                    y.Add(frequencies.Slice((int)(i*n), n).Average());
                }

                var ySum = 0.0;
                for (int i=0; i<y.Count-1; ++i)
                {
                    ySum += Math.Pow((y[i + 1] - y[i]), 2);
                }
                
                data.Add(new DataPoint<double>(tau, Math.Sqrt((1.0 / (2.0 * (M - 1.0))) * ySum)));
            }

            return data;
        }

        public override Task SetViewport(ViewportParams<double> viewport)
        {
            throw new NotSupportedException();
        }

        protected override void ChartManagerService_OnViewportChanged(object sender, EventArgs e)
        {
            //Events for chart are disabled
            return;
        }
    }
}

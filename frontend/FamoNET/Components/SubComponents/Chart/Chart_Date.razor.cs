using FamoNET.Model;
using FamoNET.Model.Args;
using FamoNET.Services;
using FamoNET.Utils;

namespace FamoNET.Components.SubComponents.Chart
{
    public partial class Chart_Date : ChartWithAllanComponentBase<DateTime>
    {
        public Chart_Date()
        {
            Model = new ChartParameters<DateTime>();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            if (firstRender)
            {
                await Initialize(new ChartParameters<DateTime>() { Title = "No data", DisableXLabels = false, DisableEvents = false, AxisMode = AxisMode.Date });
            }
        }
        public override async Task LoadData(List<DataPoint<double>> data)
        {            
            var convertedData = new List<DataPoint<DateTime>>();
            foreach (var point in data) 
            {
                convertedData.Add(new DataPoint<DateTime>(FamoMath.Convert_MJDToDateTime(point.X), point.Y));
            }

            OriginalCollection = convertedData;
            await ChartManagerService.AddDataSet<DateTime>(base.ChartGuid, convertedData);
            
            var mjdVP = await ChartManagerService.GetViewportParameters(ChartGuid);
            
            Model.Viewport.MinX = FamoMath.Convert_MJDToDateTime(mjdVP.MinX);
            Model.Viewport.MaxX = FamoMath.Convert_MJDToDateTime(mjdVP.MaxX);
            Model.Viewport.MinY = mjdVP.MinY;
            Model.Viewport.MaxY = mjdVP.MaxY;
            Model.Viewport.AxisMode = AxisMode.Date;

            StateHasChanged();
        }        

        public override async Task SetViewport(ViewportParams<double> viewport)
        {
            var convertedViewport = new ViewportParams<DateTime>();
            convertedViewport.MinX = FamoMath.Convert_MJDToDateTime((double)viewport.MinX);
            convertedViewport.MaxX = FamoMath.Convert_MJDToDateTime((double)viewport.MaxX);
            convertedViewport.MinY = viewport.MinY;
            convertedViewport.MaxY = viewport.MaxY;
            convertedViewport.AxisMode = AxisMode.Date;

            await ChartManagerService.SetViewportParameters<DateTime>(base.ChartGuid, convertedViewport);
        }

        protected override void ChartManagerService_OnViewportChanged(object sender, EventArgs e)
        {
            var eventArgs = e as DateViewportEventArgs;
            if (eventArgs == null)
            {
                return;
            }

            if (eventArgs.ChartGuid != base.ChartGuid)
            {
                return;
            }

            base.Model.Viewport = eventArgs.Viewport;
            base.StateHasChanged();
        }

        public override async Task SendToAllan()
        {
            //await AllanVariance.ClearChart();

            var allanData = new List<DataPoint<double>>();
            foreach (var dp in OriginalCollection.Where(dp => dp.X >= Model.Viewport.MinX && dp.X <= Model.Viewport.MaxX))
            {
                allanData.Add(new DataPoint<double>(FamoMath.Convert_DateTimeToMjd(dp.X), dp.Y));
            }

            await AllanVariance.LoadData(allanData);
        }
    }
}

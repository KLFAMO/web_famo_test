using FamoNET.Model;
using FamoNET.Model.Args;
using FamoNET.Services;

namespace FamoNET.Components.SubComponents.Chart
{
    public partial class Chart_MJD : ChartWithAllanComponentBase<double>
    {
        public Chart_MJD()
        {
            Model = new ChartParameters<double>();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            if (firstRender)
            {
                await Initialize(new ChartParameters<double>() { Title = "No data", DisableXLabels = false, DisableEvents = false, AxisMode = AxisMode.Mjd });
            }
        }

        public override async Task LoadData(List<DataPoint<double>> data)
        {
            OriginalCollection = new List<DataPoint<double>>();
            data.ForEach(dp => OriginalCollection.Add(new DataPoint<double>(dp)));

            await ChartManagerService.AddDataSet(ChartGuid, data);
            Model.Viewport = await ChartManagerService.GetViewportParameters(ChartGuid);
            StateHasChanged();
        }

        public override async Task SetViewport(ViewportParams<double> viewport)
        {            
            await ChartManagerService.SetViewportParameters(ChartGuid, viewport);
        }

        protected override void ChartManagerService_OnViewportChanged(object sender, EventArgs e)
        {
            var eventArgs = e as MjdViewportEventArgs;
            if (eventArgs == null)
            {
                return;
            }

            if (eventArgs.ChartGuid != ChartGuid)
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
                allanData.Add(new DataPoint<double>(dp.X, dp.Y));
            }

            await AllanVariance.LoadData(allanData);
        }
    }
}

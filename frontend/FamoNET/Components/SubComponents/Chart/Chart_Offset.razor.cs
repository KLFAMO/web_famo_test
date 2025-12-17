using FamoNET.Model;
using FamoNET.Model.Args;

namespace FamoNET.Components.SubComponents.Chart
{
    public partial class Chart_Offset : ChartWithAllanComponentBase<double>
    {               
        private double _lastOffset { get; set; }

        public Chart_Offset()
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
            foreach (var dp in data)
            {
                OriginalCollection.Add(new DataPoint<double>(dp));
            }

            await ChartManagerService.AddDataSet(ChartGuid, data);
            Model.Viewport = await ChartManagerService.GetViewportParameters(ChartGuid);
            StateHasChanged();
        }

        public override async Task UpdateParameters()
        {
            if (_lastOffset != Model.Offset)
            {
                await ClearChart();

                var newSet = new List<DataPoint<double>>();
                OriginalCollection.ForEach(dp =>
                {
                    var newDp = new DataPoint<double>(dp);
                    newDp.X -= Model.Offset;
                    newSet.Add(newDp);
                });

                await ChartManagerService.AddDataSet(ChartGuid, newSet);

                Model.Viewport.MinX = (Model.Viewport.MinX + _lastOffset) - Model.Offset;
                Model.Viewport.MaxX = (Model.Viewport.MaxX + _lastOffset) - Model.Offset;
                _lastOffset = Model.Offset;                
            }
            
            await base.UpdateParameters();
            //Model.Viewport = await ChartManagerService.GetViewportParameters(ChartGuid);
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
            // AllanVariance.ClearChart();

            var allanData = new List<DataPoint<double>>();
            foreach (var dp in OriginalCollection.Where(dp => dp.X - Model.Offset >= Model.Viewport.MinX && dp.X - Model.Offset <= Model.Viewport.MaxX))
            {
                allanData.Add(new DataPoint<double>(dp.X - Model.Offset, dp.Y));
            }

            await AllanVariance.LoadData(allanData);
        }
    }
}

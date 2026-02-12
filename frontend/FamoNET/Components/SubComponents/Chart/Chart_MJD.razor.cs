using FamoNET.Model;
using FamoNET.Model.Args;
using FamoNET.Model.Interfaces;
using FamoNET.Services;
using Microsoft.AspNetCore.Components;
using org.mariuszgromada.math.mxparser;

namespace FamoNET.Components.SubComponents.Chart
{
    public partial class Chart_MJD : ChartWithAllanComponentBase<double>
    {
        [Inject]
        private ISystemNotificationService _notificationService { get; set; }
        public string MathExpression { get; set; }
        public Chart_MJD()
        {
            Model = new ChartParameters<double>();
        }

        private async Task Calculate()
        {
            Argument x = new Argument("x");            
            Expression e = new Expression(MathExpression, x);

            var results = new List<DataPoint<double>>();
            
            if (!e.checkSyntax())
            {
                string errorMessage = e.getErrorMessage();
                _notificationService.SendSystemMessage(this, new SystemMessage($"Invalid formula: {errorMessage}", SystemMessageType.Error));
                return;
            }
            
            foreach (var point in OriginalCollection)
            {                
                x.setArgumentValue(point.Y);
                
                results.Add(new DataPoint<double>() { X = point.X, Y = e.calculate() });
            }

            await LoadData(results, null);
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            if (firstRender)
            {
                await Initialize(new ChartParameters<double>() { Title = "No data", DisableXLabels = false, DisableEvents = false, AxisMode = AxisMode.Mjd });
            }
        }

        public override async Task LoadData(List<DataPoint<double>> data, string title)
        {
            OriginalCollection = new List<DataPoint<double>>();
            data.ForEach(dp => OriginalCollection.Add(new DataPoint<double>(dp)));

            if (title != null)
            {
                await ChartManagerService.SetChartParameters(ChartGuid, new ChartParameters<double>() { Title = title }, false);
            }
            
            await ChartManagerService.AddDataSet(ChartGuid, data);
            Model.Viewport = await ChartManagerService.GetViewportParameters(ChartGuid);
            await SendToAllan();
            StateHasChanged();
        }

        public override async Task SetViewport(ViewportParams<double> viewport)
        {            
            await ChartManagerService.SetViewportParameters(ChartGuid, viewport);
        }

        protected override async void ChartManagerService_OnViewportChanged(object sender, EventArgs e)
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
            await SendToAllan();
            base.StateHasChanged();
        }

        public override async Task SendToAllan()
        {
            await AllanVariance.ClearChart();

            var allanData = new List<DataPoint<double>>();
            foreach (var dp in OriginalCollection.Where(dp => dp.X >= Model.Viewport.MinX && dp.X <= Model.Viewport.MaxX))
            {
                allanData.Add(new DataPoint<double>(dp.X, dp.Y));
            }

            await AllanVariance.LoadData(allanData, "Allan deviation");
        }
    }
}

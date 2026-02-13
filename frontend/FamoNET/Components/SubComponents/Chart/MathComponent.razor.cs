using FamoNET.Model;
using FamoNET.Model.Interfaces;
using Microsoft.AspNetCore.Components;
using org.mariuszgromada.math.mxparser;

namespace FamoNET.Components.SubComponents.Chart
{
    public partial class MathComponent : ComponentBase
    {
        [Inject]
        private ISystemNotificationService _notificationService { get; set; }

        [Parameter]
        public DataSeries<double> Series { get; set; }
        [Parameter]
        public EventCallback<DataSeries<double>> SeriesChanged { get; set; }
        private DataSeries<double> _lastSeries = null;
        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();

            if (Series == null || _lastSeries?.Guid == Series.Guid)
                return;

            _lastSeries = new DataSeries<double>(Series);
            StateHasChanged();
        }

        protected void ApplyMath()
        {
            Argument x = new Argument("x");
            Argument y = new Argument("y");
            Expression xExpression = null;
            Expression yExpression = null;

            var newData = new List<DataPoint<double>>();

            if (!string.IsNullOrWhiteSpace(_lastSeries.MathExpressionX))
            {                
                xExpression = new Expression(_lastSeries.MathExpressionX, x);

                if (!xExpression.checkSyntax())
                {
                    string errorMessage = xExpression.getErrorMessage();
                    _notificationService.SendSystemMessage(this, new SystemMessage($"Invalid formula for x: {errorMessage}", SystemMessageType.Error));
                    return;
                }
            }

            if (!string.IsNullOrWhiteSpace(_lastSeries.MathExpressionY))
            {                
                yExpression = new Expression(_lastSeries.MathExpressionY, y);

                if (!yExpression.checkSyntax())
                {
                    string errorMessage = yExpression.getErrorMessage();
                    _notificationService.SendSystemMessage(this, new SystemMessage($"Invalid formula for y: {errorMessage}", SystemMessageType.Error));
                    return;
                }
            }

            foreach (var point in _lastSeries.OriginalData)
            {
                x.setArgumentValue(point.X);
                y.setArgumentValue(point.Y);

                newData.Add(new DataPoint<double>() 
                { 
                    X = xExpression != null ? xExpression.calculate() : point.X, 
                    Y = yExpression != null ? yExpression.calculate() : point.Y 
                });
            }

            Series.ModifiedData = new List<DataPoint<double>>(newData);
            Series.MathExpressionX = _lastSeries.MathExpressionX;
            Series.MathExpressionY = _lastSeries.MathExpressionY;
            SeriesChanged.InvokeAsync(Series);
        }        
    }
}

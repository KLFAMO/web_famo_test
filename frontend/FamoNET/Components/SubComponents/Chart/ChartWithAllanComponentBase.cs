using Microsoft.AspNetCore.Components;

namespace FamoNET.Components.SubComponents.Chart
{
    public abstract class ChartWithAllanComponentBase<T> : ChartComponentBase<T> 
    {
        [Parameter]
        public bool EnableAllan { get; set; } = true;
        protected AllanVariance AllanVariance { get; set; }        

        public abstract Task SendToAllan();
        public virtual async Task ClearAllanChart()
        {
            await AllanVariance.ClearChart();
        }
    }
}

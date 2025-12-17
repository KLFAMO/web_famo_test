using FamoNET.Utils;
using Microsoft.AspNetCore.Components;
using NLog;

namespace FamoNET.Components.SubComponents
{
    public partial class MjdPickerComponent : ComponentBase
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        public MjdPickerComponent()
        {
            _logger.Debug("MjdPickerComponent created");
        }

        [Parameter]
        public EventCallback<double> StartMjdChanged { get; set; }
        [Parameter]
        public EventCallback<double> EndMjdChanged { get; set; }
        [Parameter]
        public EventCallback<bool> IsUIDisabledChanged { get; set; }

        [Parameter]
        public double EndMjd { get; set; } 

        [Parameter]
        public double StartMjd { get; set; }

        [Parameter]
        public bool IsUIDisabled { get; set; }
        public bool IsInMJDMode { get; set; } = true;
        
        public DateTime StartDate
        {
            get => FamoMath.Convert_MJDToDateTime(StartMjd).ToLocalTime();
            set => _ = OnStartMjdSet(new ChangeEventArgs() { Value = FamoMath.Convert_DateTimeToMjd(value) });
        }

        public DateTime EndDate
        {
            get => FamoMath.Convert_MJDToDateTime(EndMjd).ToLocalTime();
            set => _ = OnEndMjdSet(new ChangeEventArgs() { Value = FamoMath.Convert_DateTimeToMjd(value) });
        }      
        
        private async Task OnStartMjdSet(ChangeEventArgs startMjdArgs)
        {
            var startMjd = Convert.ToDouble(startMjdArgs?.Value.ToString());
            
            StartMjd = startMjd;
            await StartMjdChanged.InvokeAsync(startMjd);
        }

        private async Task OnEndMjdSet(ChangeEventArgs endMjdArgs)
        {
            var endMjd = Convert.ToDouble(endMjdArgs?.Value.ToString());
            
            EndMjd = endMjd;
            await EndMjdChanged.InvokeAsync(endMjd);
        }
    }
}

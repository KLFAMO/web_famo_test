using FamoNET.Model;
using FamoNET.Model.Args;
using Microsoft.AspNetCore.Components;
using NLog;

namespace FamoNET.Components.Pages.DataBrowser
{
    public partial class ChartWizard : ComponentBase
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        [Parameter]
        public EventCallback<ChartParams> ChartParametersUpdated { get; set; }

        [SupplyParameterFromForm]
        private ChartParams Model { get; set; }        



        protected override void OnInitialized()
        {
            base.OnInitialized();
            Model = new ChartParams();
        }
        
        protected async Task HandleValidSubmit()
        {
            if (Model == null)
                return;

            await ChartParametersUpdated.InvokeAsync(new ChartParams(Model));
        }


        public void SetParams(ChartParams model)
        {
            Model = model;
            StateHasChanged();
        }        
    }
}

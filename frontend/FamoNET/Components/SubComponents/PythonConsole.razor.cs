using Microsoft.AspNetCore.Components;

namespace FamoNET.Components.SubComponents
{
    public partial class PythonConsole : ComponentBase
    {
        public string Cmd {  get; set; }
        [Parameter]
        public EventCallback<string> QueryExecuted { get; set; }

        private async Task HandleValidSubmit()
        {
            await QueryExecuted.InvokeAsync(Cmd);
        }        
    }
}

using FamoNET.Model;
using FamoNET.Model.Args;
using Microsoft.AspNetCore.Components;

namespace FamoNET.Components.SubComponents
{
    public partial class ConfirmModal : ComponentBase
    {
        private Operation _operation { get; set; }
        private bool _isVisible { get; set; } = false;
        [Parameter]
        public EventCallback<ModalConfirmationEventArgs> ChoiceMade { get; set; }

        public async Task HandleConfirmation(bool choice)
        {
            await ChoiceMade.InvokeAsync(new ModalConfirmationEventArgs(choice, _operation));
            _isVisible = false;
        }

        public void ShowModal(Operation operation)
        {
            _operation = operation;
            _isVisible = true;
            StateHasChanged();
        }
    }
}

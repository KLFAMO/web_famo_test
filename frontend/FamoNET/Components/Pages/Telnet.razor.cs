using FamoNET.Model;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace FamoNET.Components.Pages
{
    public partial class Telnet : ComponentBase
    {
        [Inject]
        private IJSRuntime _jsRuntime { get; set; }
        private IJSObjectReference _module;

        private string _currentMessage = String.Empty;
        public List<TerminalMessage> Messages { get; set; } = new List<TerminalMessage>();


        public int CurrentHistoryIndex = 0;
        public string CurrentMessage 
        { 
            get => _currentMessage; 
            set
            {
                if (_currentMessage == value)
                    return;

                _currentMessage = value;
                StateHasChanged();
            } 
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            if (firstRender)
            {
                _module = await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "./Components/Pages/Telnet.razor.js");                
            }

            await _module.InvokeVoidAsync("Terminal_ScrollToBottom");
        }

        public async Task SendMessage()
        {
            //dummy
            AddMessageToView(new TerminalMessage { Message = CurrentMessage, Type = TerminalMessageType.Ok });            
        }

        public void AddMessageToView(TerminalMessage terminalMessage)
        {
            if (CurrentMessage.Length < 1)
            {
                return;
            }

            Messages.Add(terminalMessage);
            CurrentHistoryIndex = Messages.Count;
        }

        public void OnKeyPress(KeyboardEventArgs keyboardEventArgs)
        {
            if (keyboardEventArgs.Key == "ArrowUp")
            {
                CurrentHistoryIndex -= 1;

                if (CurrentHistoryIndex < 1)
                {
                    CurrentHistoryIndex = 0;                                        
                }
            }
            else if (keyboardEventArgs.Key == "ArrowDown")
            {
                CurrentHistoryIndex += 1;

                if (CurrentHistoryIndex >= Messages.Count)
                {
                    CurrentHistoryIndex = Messages.Count;
                    CurrentMessage = String.Empty;
                    return;
                }
            }
            else if (keyboardEventArgs.Key == "Enter")
            {
                SendMessage();
                CurrentMessage = String.Empty;
                return;
            }
            else
            {
                return;
            }
                        
            CurrentMessage = Messages[CurrentHistoryIndex].Message;            
        }

        public void OnArrowDown()
        {
            CurrentHistoryIndex -= 1;

            if (CurrentHistoryIndex < 0 || CurrentHistoryIndex >= Messages.Count)
            {
                return;
            }

            CurrentMessage = Messages[CurrentHistoryIndex].Message;
        }
    }
}

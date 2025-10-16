using FamoNET.Model;
using FamoNET.Model.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using NLog;

namespace FamoNET.Components.Pages
{
    public partial class Telnet : ComponentBase
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        [Inject]
        private IJSRuntime _jsRuntime { get; set; }
        [Inject]
        private IDevicesDataService _devicesDataService { get; set; }
        [Inject]
        private ITelnetService _telnetService { get; set; }

        private IJSObjectReference _module;
        private ElementReference InputBox;

        private string _currentMessage = String.Empty;
        public List<TerminalMessage> Messages { get; set; } = new List<TerminalMessage>();

        public List<Device> Devices { get; set; } = [];
        
        private string _selectedIP = String.Empty;
        private int? Port { get; set; }
        public string SelectedIP 
        { 
            get => _selectedIP; 
            set
            {
                if (value == _selectedIP) 
                    return;
                
                _selectedIP = value;
                if (!string.IsNullOrWhiteSpace(_selectedIP))
                    IsReady = true;
                                                
                StateHasChanged();
            }
        }

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

        public bool IsReady { get; set; } = false;
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            if (firstRender)
            {
                _module = await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "./Components/Pages/Telnet.razor.js");
                await LoadData();
            }

            await _module.InvokeVoidAsync("Terminal_ScrollToBottom");
        }

        private async Task LoadData()
        {
            Devices = await _devicesDataService.GetDevicesAsync();
            await InvokeAsync(() => StateHasChanged());
        }

        public async Task SendMessage()
        {
            if (string.IsNullOrWhiteSpace(_selectedIP) || string.IsNullOrWhiteSpace(CurrentMessage))
                return;

            IsReady = false;
            
            try
            {
                int port = Port.HasValue ? Port.Value : 23;
                await _telnetService.Send(SelectedIP + $":{port}", CurrentMessage);
                AddMessageToView(new TerminalMessage { Message = CurrentMessage, Type = TerminalMessageType.Ok });                
            }
            catch(Exception ex)
            {
                AddMessageToView(new TerminalMessage { Message = CurrentMessage, Type = TerminalMessageType.Error });
                _logger.Error(ex);
            }

            IsReady = true;
            await InvokeAsync(() => StateHasChanged());
            await InputBox.FocusAsync();
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

        public async void OnKeyPress(KeyboardEventArgs keyboardEventArgs)
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
                await SendMessage();
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

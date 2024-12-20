using FamoNET.Model;
using FamoNET.Model.Args;
using FamoNET.Services;
using FamoNET.Services.DataServices;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using NLog;

namespace FamoNET.Components.Pages.Counter
{
    public partial class Counter : ComponentBase, IDisposable
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        [Inject]
        private CounterWriterService WriterService { get; set; }
        [Inject]
        private CounterDataService CounterDataService { get; set; }
        [Inject]
        private IJSRuntime _jsRuntime { get; set; }
        private IJSObjectReference _module;

        private int _uiRefreshDelay = 50;
        private int _uiRefreshCounter = 0;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                _module = await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "./Components/Pages/Counter/Counter.razor.js");
                await CounterDataService.ConnectAsync().ConfigureAwait(false);
            }                            

            await base.OnAfterRenderAsync(firstRender);
        }

        protected override async Task OnInitializedAsync()
        {
            CounterDataService.ChannelsReceived += CounterDataService_ChannelsReceived;            
            await base.OnInitializedAsync();
        }                

        private async void CounterDataService_ChannelsReceived(object sender, ChannelsReceivedEventArgs e)
        {
            if (_module == null)
                return;            

            try
            {
                if (_uiRefreshDelay > _uiRefreshCounter)
                {
                    _uiRefreshCounter += 1;
                    return;
                }
                    

                _uiRefreshCounter = 0;
                
                string displayText = string.Empty;
                foreach (var channel in e.Channels) 
                {
                    displayText += $"Channel {channel.Id}: {channel.Frequency.ToString("0.#")} <br>";
                }
                await _module.InvokeAsync<string>("UpdateFrequency", displayText);
                                
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }                                    
        }
        
        public string Filename { get; set; }
        
        private bool _isSaving = false;        
        public bool IsSaving 
        { 
            get => _isSaving; 
            set
            {
                if (_isSaving == value) 
                    return;

                _isSaving = value;                
                StateHasChanged();
            }
        }

        public Task StartSaving()
        {
            if (Filename == null || Filename.Length < 1)
                return Task.CompletedTask;

            WriterService.ConfigureAndStartSave(Filename, [0]); //todo later, bad way of managing those classes
            CounterDataService.StartSaving(WriterService);
            IsSaving = true;            
            
            return Task.CompletedTask;
        }

        public Task StopSaving()
        {
            CounterDataService.StopSaving();
            IsSaving = false;                        

            return Task.CompletedTask;
        }        

        public void Dispose()
        {
            CounterDataService.ChannelsReceived -= CounterDataService_ChannelsReceived;
        }
    }
}

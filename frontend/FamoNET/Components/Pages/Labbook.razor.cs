using FamoNET.Components.SubComponents.Labbook;
using FamoNET.Model;
using FamoNET.Model.Args;
using FamoNET.Model.Interfaces;
using Microsoft.AspNetCore.Components;
using NLog;

namespace FamoNET.Components.Pages
{           
    public partial class Labbook : ComponentBase
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        [Inject]
        private ILabbookDataService _labbookDataService { get; set; }
        [Inject]
        private ISystemNotificationService _systemNotificationService { get; set; }
        
        private LabbookEntryWizardModel LabbookEntryWizardModel;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            if (firstRender)
            {                
                LabbookEntryWizardModel.EntrySubmitted += EntrySubmitted;
                await LoadDataAsync();
            }
        }

        private bool _isLoading = false;        
        protected bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (value == _isLoading) return;
                _isLoading = value;
                StateHasChanged();
            }
        }

        public List<LabbookEntry> Entries { get; set; } = new List<LabbookEntry>();   
        
        private async void EntrySubmitted(object sender, GenericEventArgs<LabbookEntry> args)
        {
            if (args.Value.Id > 0)
            {
                await UpdateEntryAsync(args.Value);
            }
            else
            {
                await AddEntryAsync(args.Value);
            }
        }
        protected async Task LoadDataAsync()
        {
            IsLoading = true;
            Entries = await _labbookDataService.GetEntriesAsync();
            foreach (var entry in new List<LabbookEntry>(Entries))
            {
                if (entry.ParentId > 0)
                {
                    //look for parent in fetched list
                    var parent = Entries.FirstOrDefault(e => e.Id == entry.ParentId);
                    if (parent == null)
                    {
                        //fetch parent if not found
                        parent = await _labbookDataService.GetByIdAsync(entry.ParentId);
                        if (parent != null)
                        {
                            Entries.Add(parent);
                        }
                        else
                        {
                            _systemNotificationService.SendSystemMessage(this, new SystemMessage("Failed to fetch full history", SystemMessageType.Error));
                            _logger.Warn("Failed to retrieve parent entry");   
                        }
                    }
                    parent.Child = entry; 
                }
            }
            Entries.OrderByDescending(e => e.Id);
            IsLoading = false;            
        }

        protected async Task AddEntryAsync(LabbookEntry entry)
        {
            if (await _labbookDataService.PostAsync(entry))
            {
                _systemNotificationService.SendSystemMessage(this, new SystemMessage("Entry added", SystemMessageType.Info));
                await LoadDataAsync();                
            }
            else
            {
                _systemNotificationService.SendSystemMessage(this, new SystemMessage("Failed to add entry", SystemMessageType.Error));
            }
        }

        protected async Task UpdateEntryAsync(LabbookEntry entry)
        {
            if (await _labbookDataService.UpdateAsync(entry))
            {
                _systemNotificationService.SendSystemMessage(this, new SystemMessage("Entry updated", SystemMessageType.Info));
                await LoadDataAsync();                
            }
            else
            {
                _systemNotificationService.SendSystemMessage(this, new SystemMessage("Failed to update entry", SystemMessageType.Info));
            }
        }
    }
}

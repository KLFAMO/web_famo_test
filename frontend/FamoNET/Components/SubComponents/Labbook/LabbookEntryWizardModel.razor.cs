using FamoNET.Model;
using FamoNET.Model.Args;
using FamoNET.Utils;
using Microsoft.AspNetCore.Components;

namespace FamoNET.Components.SubComponents.Labbook
{
    public partial class LabbookEntryWizardModel : ComponentBase
    {
        public event EventHandler<GenericEventArgs<LabbookEntry>> EntrySubmitted;    
        public LabbookEntry LabbookEntry { get; set; }
        public bool IsValid => LabbookEntry != null && !string.IsNullOrWhiteSpace(LabbookEntry.Message) && LabbookEntry.StartMjd <= LabbookEntry.EndMjd;
        public bool IsVisible { get; set; }
        private Guid MjdPickerComponentKey;
        public void Load(LabbookEntry existingEntry = null)
        {
            LabbookEntry = existingEntry ?? new LabbookEntry()
            {
                StartMjd = Math.Round(FamoMath.Convert_DateTimeToMjd(DateTime.UtcNow), 4),
                EndMjd = Math.Round(FamoMath.Convert_DateTimeToMjd(DateTime.UtcNow), 4)
            };
            ShowModal();
        }

        protected override void OnAfterRender(bool firstRender)
        {
            base.OnAfterRender(firstRender);

            if (firstRender)
            {
                MjdPickerComponentKey = Guid.NewGuid();
                
            }
        }

        private void Submit()
        {
            if (!IsValid)
                return;

            EntrySubmitted?.Invoke(this, new GenericEventArgs<LabbookEntry>(LabbookEntry));
            CloseModal();
        }

        private void ShowModal()
        {
            IsVisible = true;
            StateHasChanged();
        }

        private void CloseModal()
        {
            IsVisible = false;
            StateHasChanged();
        }
    }
}

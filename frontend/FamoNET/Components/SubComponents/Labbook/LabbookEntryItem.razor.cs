using FamoNET.Model;
using FamoNET.Utils;
using Microsoft.AspNetCore.Components;

namespace FamoNET.Components.SubComponents.Labbook
{
    public partial class LabbookEntryItem : ComponentBase
    {
        [Parameter]
        public LabbookEntryWizardModel LabbookEntryWizardModel { get; set; }
        [Parameter]
        public LabbookEntry Entry { get; set; }
        [Parameter]
        public bool IsCollapsed { get; set; } = true;
        public bool IsChild => Entry.ParentId > 0;
        private LabbookEntryItem ChildEntryItem { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        private void ToggleChildVisibility()
        {
            if (ChildEntryItem == null)
                return;

            ChildEntryItem.IsCollapsed = !ChildEntryItem.IsCollapsed;
            StateHasChanged();
        }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            if (Entry != null)
            {
                StartDate = FamoMath.Convert_MJDToDateTime(Entry.StartMjd).ToLocalTime();
                EndDate = FamoMath.Convert_MJDToDateTime(Entry.EndMjd).ToLocalTime();
            }
            
        }
    }
}

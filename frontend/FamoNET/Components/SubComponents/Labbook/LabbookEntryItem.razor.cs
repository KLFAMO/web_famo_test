using FamoNET.Model;
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
        private void ToggleChildVisibility()
        {
            if (ChildEntryItem == null)
                return;

            ChildEntryItem.IsCollapsed = !ChildEntryItem.IsCollapsed;
            StateHasChanged();
        }
    }
}

using FamoNET.Model;
using FamoNET.Model.Args;
using Microsoft.AspNetCore.Components;

namespace FamoNET.Components.SubComponents
{
    public partial class DDSControlPanel : ComponentBase
    {
        [Parameter]
        public Model.DDS DDS_Model { get; set; }
        public List<DDSChannel> Channels { get; set; }

        private ConfirmModal ConfirmModalComponent;

        public void OnModalConfirmed(ModalConfirmationEventArgs e)
        {
            if (e.Operation != Operation.Update)
                return;
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            Channels = DDS_Model.Channels;
        }
    }
}

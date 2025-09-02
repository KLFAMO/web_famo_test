using FamoNET.Model;
using FamoNET.Model.Args;
using Microsoft.AspNetCore.Components;

namespace FamoNET.Components.SubComponents
{
    public partial class DDSConfigurationPanel : ComponentBase
    {
        private Operation _operation { get; set; }
        
        private Model.DDS DDS_Model { get; set; }
        
        private ConfirmModal ConfirmModalComponent;

        public void OnModalConfirmed(ModalConfirmationEventArgs e)
        {
            if (e.Operation == Operation.Create)
            {
                //send create request
            }
            else if (e.Operation == Operation.Delete)
            {
                //send delete request
            }
            else if (e.Operation == Operation.Update)
            {
                //send update request
            }
        }

        public void LoadModel(DDS dds = null)
        {
            if (dds == null)
            {
                dds = new DDS();
                _operation = Operation.Create;
            }
            else
            {
                DDS_Model = dds;
                _operation = Operation.Update;
            }
            
            StateHasChanged();
        }      
        
        public void OnSendClicked()
        {
            ConfirmModalComponent.ShowModal(_operation);
        }

        public void AddChannel()
        {
            DDS_Model.Channels.Add(new DDSChannel());
        }

        public void OnDeleteClicked()
        {
            ConfirmModalComponent.ShowModal(Operation.Delete);
        }
    }
}

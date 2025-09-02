namespace FamoNET.Model.Args
{
    public class ModalConfirmationEventArgs : EventArgs
    {
        public bool Value { get; }
        public Operation Operation { get; }

        public ModalConfirmationEventArgs(bool value, Operation operation)
        {
            Value = value;
            Operation = operation;
        }
    }
}

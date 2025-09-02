namespace FamoNET.Model
{
    public enum ProgressStatus
    {
        InProgress,
        Completed
    }

    public enum AxisMode
    {
        Mjd = 0,
        Date = 1,
        Offset = 2
    }

    public enum TerminalMessageType
    {
        Ok,
        Error
    }

    public enum Operation
    {
        Create,
        Update,
        Delete
    }    
}

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
        Seconds = 2
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
    public enum AllanTauMode
    {
        AllTau = 1,
        Octave = 2,
        Decade = 10
    }

    public enum AllanType
    {
        Normal = 1,
        Overlapping = 2,
        Modified = 3
    }
}

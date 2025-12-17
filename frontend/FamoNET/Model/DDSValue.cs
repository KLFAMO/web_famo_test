namespace FamoNET.Model
{
    public class DDSValue
    {        
        public double Value { get; set; }
        public double Min { get; set; }
        public double Max { get; set; }
        public string Command { get; set; }
        public string Description { get; set; }
        public bool IsValid => Value >= Min && Value <= Max;
    }
}

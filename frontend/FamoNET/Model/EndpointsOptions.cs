namespace FamoNET.Model
{
    public class EndpointsOptions
    {
        public const string SectionName = "Endpoints";

        public string AndaUri { get; set; }        
        public string FreqMonitorUri { get; set; }
        public string FXMCounterUri { get; set; }
        public string DevicesUri { get; set; }
        public string TelnetUri { get; set; }
    }
}

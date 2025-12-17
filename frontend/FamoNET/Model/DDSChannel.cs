namespace FamoNET.Model
{
    public class DDSChannel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DDSValue Frequency { get; set; }
        public DDSValue Amplitude { get; set; }
        public DDSValue Phase { get; set; }
    }
}

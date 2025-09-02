using System.Net;

namespace FamoNET.Model
{
    public class DDS
    {
        public int Id { get; set; }
        public string IP { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<DDSChannel> Channels { get; set; }
    }
}

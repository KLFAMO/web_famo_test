using FamoNET.Converters;
using System.Text.Json.Serialization;

namespace FamoNET.Model
{    
    [JsonConverter(typeof(DDSDeviceConverter))]
    public class DDSDevice : Device
    {        
        public List<DDSChannel> Channels { get; set; }
        public bool IsLocked { get; set; }
        public DDSDevice()
        {
            
        }

        public DDSDevice(Device device) : base(device) 
        {
            
        }
    }
}

using System.Text.Json.Serialization;

namespace FamoNET.Model
{
    /*
     * 
     * "name": "uc_dds4ch_new_1",
        "ip_famo": "192.168.3.3",
        "device_type": "dds_kam",
        "description": "Sr4 blue and red MOT AOMs\r\nIND",
        "location": "Sr4"
     */
    public class Device
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("description")]
        public string Description { get; set; }
        [JsonPropertyName("ip_famo")]
        public string IP { get; set; }
        [JsonPropertyName("device_type")]
        public string DeviceType { get; set; }
        [JsonPropertyName("location")]
        public string Location { get; set; }
    }
}

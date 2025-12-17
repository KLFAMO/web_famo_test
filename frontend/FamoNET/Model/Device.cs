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
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("description")]
        public string Description { get; set; }
        [JsonPropertyName("ip_famo")]
        public string IP { get; set; }
        [JsonPropertyName("element_type")]
        public string DeviceType { get; set; }
        [JsonPropertyName("location")]
        public string Location { get; set; }
        public string Tags { get; set; }

        public Device()
        {
            
        }

        public Device(Device d)
        {
            Id = d.Id;
            Name = d.Name;
            Description = d.Description;
            IP = d.IP;
            DeviceType = d.DeviceType;
            Location = d.Location;
            Tags = d.Tags;
        }
    }
}

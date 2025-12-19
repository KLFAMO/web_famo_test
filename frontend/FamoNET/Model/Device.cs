using System.Text.Json.Serialization;

namespace FamoNET.Model
{    
    public class Device
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("port")]
        public int Port { get; set; }
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
            Port = d.Port;
            DeviceType = d.DeviceType;
            Location = d.Location;
            Tags = d.Tags;
        }
    }
}

using System.Text.Json.Serialization;

namespace FamoNET.Model.Dto
{
    public class TelnetResponseDto
    {
        [JsonPropertyName("response")]
        public string Response { get; set; }
        [JsonPropertyName("status")]
        public string Status { get; set; }
        public bool IsSuccess { get => Status == "ok"; }
    }
}

using System.Text.Json.Serialization;

namespace FamoNET.Model.Dto
{    
    public class DataSetDto
    {
        [JsonPropertyName("mjd")]
        public List<decimal> MJD { get; set; }

        [JsonPropertyName("value")]
        public List<decimal> Values { get; set; }
    }
}

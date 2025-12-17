using System.Text.Json.Serialization;

namespace FamoNET.Model.Dto
{    
    public class DataSetDto
    {
        [JsonPropertyName("mjd")]
        public List<double> MJD { get; set; }

        [JsonPropertyName("value")]
        public List<double> Values { get; set; }
    }
}

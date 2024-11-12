using FamoNET.Model;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FamoNET.Converters
{
    public class ChartParamsConverter : JsonConverter<ChartParams>
    {
        public override ChartParams Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, ChartParams value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}

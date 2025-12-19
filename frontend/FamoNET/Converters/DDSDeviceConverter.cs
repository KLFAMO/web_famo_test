using FamoNET.Model;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace FamoNET.Converters
{
    /*
    {
  "device_type": "dds_kam",
  "eth_communication": {
    "port": 23,
    "parameters": {
      "DDS": {
        "CH0": {
          "FREQ": {
            "val": 0,
            "min": 0,
            "max": 180,
            "label": "Frequency of channel 0 in MHz"
          },
          "AMP": {
            "val": 0,
            "min": 0,
            "max": 1,
            "label": "Amplitude of channel 0 (from 0 to 1)"
          }
        },
        "CH1": {
          "FREQ": {
            "val": 0,
            "min": 0,
            "max": 180,
            "label": "Frequency of channel 1 in MHz"
          },
          "AMP": {
            "val": 0,
            "min": 0,
            "max": 1
          }
        }
      }
    }
  }
} 


     */
    public class DDSDeviceConverter : JsonConverter<DDSDevice>
    {
        public override DDSDevice Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // 1. Parse the JSON into a Node for easier navigation
            var root = JsonNode.Parse(ref reader);
            if (root == null) return null;

            var device = new DDSDevice();

            // 2. Map root properties (Fields that match straightforwardly)
            // Note: We use case-insensitive matching pattern or explicit strings based on your JSON
            device.DeviceType = root["device_type"]?.ToString();

            // Map other fields if they exist in the incoming JSON, otherwise they remain null/default
            device.Id = (int?)root["id"] ?? 0;
            device.Name = root["name"]?.ToString();
            device.IP = root["eth_communication"]?["ip"]?.ToString(); // Fallback logic example
            device.Port = Int32.Parse(root["eth_communication"]?["port"]?.ToString());

            // 3. Navigate to the core data: eth_communication -> parameters -> DDS
            var ddsNode = root["eth_communication"]?["parameters"]?["DDS"];

            if (ddsNode is JsonObject ddsObject)
            {
                device.Channels = new List<DDSChannel>();

                // 4. Iterate over the dynamic keys ("CH0", "CH1", etc.)
                foreach (var channelProperty in ddsObject)
                {
                    string channelKey = channelProperty.Key; // "CH0"
                    JsonNode channelData = channelProperty.Value;

                    var channel = new DDSChannel
                    {
                        Name = channelKey,
                        // Extract ID from "CH0" -> 0. Fallback to -1 if parsing fails.
                        Id = int.TryParse(channelKey.Replace("CH", ""), out int cid) ? cid : -1,
                        Description = $"Channel {channelKey}"
                    };

                    // 5. Map the nested components (FREQ, AMP) to the typed properties
                    if (channelData is JsonObject channelObj)
                    {
                        // Map FREQ to Frequency
                        if (channelObj["FREQ"] is JsonObject freqNode)
                        {
                            channel.Frequency = ParseDDSValue(freqNode, "FREQ");
                            channel.Frequency.Command = $"DDS:{channelKey}:FREQ:VAL";
                        }
                            

                        // Map AMP to Amplitude
                        if (channelObj["AMP"] is JsonObject ampNode)
                        {
                            channel.Amplitude = ParseDDSValue(ampNode, "AMP");
                            channel.Amplitude.Command = $"DDS:{channelKey}:AMP:VAL";
                        }
                            

                        // Map PHS/PHASE if it exists (assuming key might be "PHS")
                        if (channelObj["PH"] is JsonObject phsNode)
                        {
                            channel.Phase = ParseDDSValue(phsNode, "PH");
                            channel.Phase.Command = $"DDS:{channelKey}:PH:VAL";
                        }
                            
                    }

                    device.Channels.Add(channel);
                }
            }

            return device;
        }

        // Helper to map the inner value object
        private DDSValue ParseDDSValue(JsonNode node, string commandName)
        {
            return new DDSValue
            {
                Command = commandName, // "FREQ" or "AMP"
                Value = (double?)node["val"] ?? 0,
                Min = (double?)node["min"] ?? 0,
                Max = (double?)node["max"] ?? 0,
                Description = node["label"]?.ToString() ?? node["description"]?.ToString()
            };
        }

        public override void Write(Utf8JsonWriter writer, DDSDevice value, JsonSerializerOptions options)
        {
            // Implementing Write is optional if you only read from API.
            // It requires manually reconstructing the deep nesting.
            writer.WriteStartObject();

            writer.WriteString("device_type", value.DeviceType);

            writer.WritePropertyName("eth_communication");
            writer.WriteStartObject();
            writer.WritePropertyName("parameters");
            writer.WriteStartObject();
            writer.WritePropertyName("DDS");
            writer.WriteStartObject();

            if (value.Channels != null)
            {
                foreach (var channel in value.Channels)
                {
                    writer.WritePropertyName(channel.Name ?? $"CH{channel.Id}");
                    writer.WriteStartObject();

                    if (channel.Frequency != null) WriteDDSValue(writer, "FREQ", channel.Frequency);
                    if (channel.Amplitude != null) WriteDDSValue(writer, "AMP", channel.Amplitude);
                    if (channel.Phase != null) WriteDDSValue(writer, "PH", channel.Phase);

                    writer.WriteEndObject();
                }
            }

            writer.WriteEndObject(); // End DDS
            writer.WriteEndObject(); // End parameters
            writer.WriteEndObject(); // End eth_communication

            writer.WriteEndObject();
        }

        private void WriteDDSValue(Utf8JsonWriter writer, string key, DDSValue val)
        {
            writer.WritePropertyName(key);
            writer.WriteStartObject();
            writer.WriteNumber("val", val.Value);
            writer.WriteNumber("min", val.Min);
            writer.WriteNumber("max", val.Max);
            if (!string.IsNullOrEmpty(val.Description))
                writer.WriteString("label", val.Description);
            writer.WriteEndObject();
        }
    }
}

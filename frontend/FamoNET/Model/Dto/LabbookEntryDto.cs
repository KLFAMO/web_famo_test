using System.Text.Json.Serialization;

namespace FamoNET.Model.Dto
{
    public class LabbookEntryDTO
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("start_mjd")]
        public string StartMjd { get; set; }
        [JsonPropertyName("end_mjd")]
        public string EndMjd { get; set; }
        [JsonPropertyName("message")]
        public string Message { get; set; }
        [JsonPropertyName("tag")]
        public string Tag { get; set; }
        [JsonPropertyName("parentId")]
        public int ParentId { get; set; }
        [JsonPropertyName("extra")]
        public string Extra { get; set; }

        public LabbookEntryDTO(LabbookEntry dto)
        {
            if (dto == null)
                return;

            Id = dto.Id;
            StartMjd = dto.StartMjd.ToString();
            EndMjd = dto.EndMjd.ToString();
            Message = dto.Message;
            Tag = dto.Tag;
            ParentId = dto.ParentId;
            Extra = dto.Extra;
        }

        public LabbookEntryDTO()
        {
            
        }
    }
}

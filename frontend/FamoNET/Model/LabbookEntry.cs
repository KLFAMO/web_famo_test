using FamoNET.Model.Dto;
using System.Text.Json.Serialization;

namespace FamoNET.Model
{    
    public class LabbookEntry
    {        
        public int Id { get; set; }        
        public double StartMjd { get; set; }        
        public double EndMjd { get; set; }        
        public string Message { get; set; }        
        public string Tag { get; set; }        
        public int ParentId { get; set; }            
        public string Extra { get; set; }
        public LabbookEntry Child { get; set; }

        public LabbookEntry()
        {
            
        }

        public LabbookEntry(LabbookEntryDTO dto)
        {
            if (dto == null)
                return;

            Id = dto.Id;
            StartMjd = Convert.ToDouble(dto.StartMjd);
            EndMjd = Convert.ToDouble(dto.EndMjd);
            Message = dto.Message;
            Tag = dto.Tag;
            ParentId = dto.ParentId;
            Extra = dto.Extra;
        }
    }
}

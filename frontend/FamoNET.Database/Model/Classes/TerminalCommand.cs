using FamoNET.Database.Model.Interfaces;

namespace FamoNET.Database.Model.Classes
{
    public class TerminalCommand : IDbObject
    {
        public int Id { get; set; }
        public string IP { get; set; }
        public string Request { get; set; }
        public string Response { get; set; }
        public int ResponseType { get; set; }
        public DateTime CreatedOn { get; set; }
        public int State { get; set; }
    }
}

using FamoNET.Database.Model.Interfaces;

namespace FamoNET.Database.Model.Classes
{
    public class DDSChannel : IDbObject
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedOn { get; set; }
        public int State { get; set; }
        public bool IsLocked { get; set; }
        public DDSDevice Device { get; set; }
    }
}

using FamoNET.Database.Model.Interfaces;

namespace FamoNET.Database.Model.Classes
{
    public class DDSDevice : IDbObject
    {
        public int Id { get; set; }
        public DateTime CreatedOn { get; set; }
        public int State { get; set; }

        public bool IsLocked { get; set; }
        public int DeviceId { get; set; }
    }
}

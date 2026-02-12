using FamoNET.Database.Model.Interfaces;

namespace FamoNET.Database.Model.Classes
{
    public class RemoteChartFile : IDbObject
    {
        public int Id { get; set; }
        public DateTime CreatedOn { get; set; }
        public int State { get; set; }        
        public int ApiDeviceId { get; set; }
    }
}

using FamoNET.Database.Model.Interfaces;

namespace FamoNET.Database.Model.Classes
{
    public abstract class DbObjectBase : IDbObject
    {        
        public int Id { get; set; }
        public DateTime CreatedOn { get; set; }
        public int State { get; set; }
    }
}

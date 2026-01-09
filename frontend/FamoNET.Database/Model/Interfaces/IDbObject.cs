namespace FamoNET.Database.Model.Interfaces
{
    public interface IDbObject
    {
        public int Id { get; set; }
        public DateTime CreatedOn { get; set; }
        public int State { get; set; }
    }
}

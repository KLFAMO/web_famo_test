namespace FamoNET.Model
{
    public class DataPoint
    {        
        public decimal X { get; set; }
        public decimal Y { get; set; }

        public DataPoint(decimal x, decimal y)
        {
            X = x; 
            Y = y;
        }
    }
}

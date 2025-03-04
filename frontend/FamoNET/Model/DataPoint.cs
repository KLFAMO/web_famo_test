namespace FamoNET.Model
{
    public class DataPoint
    {        
        public double X { get; set; }
        public double Y { get; set; }

        public DataPoint(double x, double y)
        {
            X = x; 
            Y = y;
        }
    }
}

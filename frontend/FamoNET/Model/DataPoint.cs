namespace FamoNET.Model
{
    public class DataPoint<T>
    {        
        public T X { get; set; }
        public double Y { get; set; }

        public DataPoint()
        {
            
        }

        public DataPoint(T x, double y)
        {
            X = x; 
            Y = y;
        }

        public DataPoint(DataPoint<T> point)
        {
            X = point.X;
            Y = point.Y;
        }
    }
}

namespace FamoNET.Model
{
    public class ViewportParams<T>
    {
        public T MinX { get; set; }
        public T MaxX { get; set; }
        public double MinY { get; set; }
        public double MaxY { get; set; }
        public AxisMode AxisMode { get; set; }

        public ViewportParams()
        {
                
        }

        public ViewportParams(ViewportParams<T> vp)
        {
            MinX = vp.MinX;
            MaxX = vp.MaxX;
            MinY = vp.MinY;
            MaxY = vp.MaxY;
            AxisMode = vp.AxisMode;
        }        
    }
}

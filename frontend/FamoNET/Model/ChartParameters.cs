namespace FamoNET.Model
{
    public class ChartParameters<T>
    {
        public string Title { get; set; }
        public ViewportParams<T> Viewport { get; set; } = new ViewportParams<T>();
        public double Offset { get; set; }        
        public bool Logarithmic { get; set; }
        public bool DisableXLabels {  get; set; }
        public bool DisableEvents { get; set; }
        public bool InvertYAxis { get; set; }
        public AxisMode AxisMode { get; set; }
        public ChartParameters()
        {
            
        }

        public ChartParameters(ChartParameters<T> chartParams)
        {
            Title = chartParams.Title;
            Viewport = new ViewportParams<T>(chartParams.Viewport);
            Offset = chartParams.Offset;            
            Logarithmic = chartParams.Logarithmic;
            DisableEvents = chartParams.DisableEvents;
            InvertYAxis = chartParams.InvertYAxis;
            AxisMode = chartParams.AxisMode;
        }
    }
}

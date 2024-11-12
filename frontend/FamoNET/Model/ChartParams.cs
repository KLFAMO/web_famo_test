namespace FamoNET.Model
{
    public class ChartParams
    {
        public string Title { get; set; }
        public decimal MinX { get; set; }
        public decimal MaxX { get; set; }
        public decimal MinY { get; set; }
        public decimal MaxY { get; set; }

        public ChartParams()
        {
            
        }

        public ChartParams(ChartParams chartParams)
        {
            Title = chartParams.Title;
            MinX = chartParams.MinX;
            MaxX = chartParams.MaxX;
            MinY = chartParams.MinY;
            MaxY = chartParams.MaxY;
        }
    }
}

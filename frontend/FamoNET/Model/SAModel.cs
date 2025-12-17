namespace FamoNET.Model
{
    public class SAModel
    {
        public double RBW { get; set; }
        public double CenterFrequency { get; set; }
        public double Span { get; set; }
        public double VBW { get; set; }
        public List<double> Data { get; set; } = new List<double>();
    }
}

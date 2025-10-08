namespace FamoNET.Model
{
    public class SpectrumAnalyzerParameters
    {
        public List<double> Frequencies { get; set; }
        public double RBW { get; set; }
        public double VBW { get; set; }
        public double Span { get; set; }
        public double CenterFrequency { get; set; }
    }
}

namespace FamoNET.Model
{
    public class SpectrumAnalyzerParameters
    {
        public List<double> Frequencies { get; set; }
        public double RBW { get; set; }
        public double VBW { get; set; }
        public double Span { get; set; }
        public double CenterFrequency { get; set; }

        public SpectrumAnalyzerParameters()
        {
            
        }

        public SpectrumAnalyzerParameters(SpectrumAnalyzerParameters other)
        {
            if (other == null)
                return;

            Frequencies = other.Frequencies is null ? new List<double>() : new List<double>(other.Frequencies);
            RBW = other.RBW;
            VBW = other.VBW;
            Span = other.Span;
            CenterFrequency = other.CenterFrequency;
        }
    }
}

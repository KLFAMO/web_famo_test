namespace FamoNET.Model.Args
{
    public class SpectrumAnalyzerEventArgs : EventArgs
    {
        public SpectrumAnalyzerParameters Parameters { get; }
        public SpectrumAnalyzerEventArgs(SpectrumAnalyzerParameters spectrumAnalyzerParameters)
        {
            Parameters = spectrumAnalyzerParameters;
        }
    }
}

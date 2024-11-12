namespace FamoNET.Model.Args
{
    public class ChartParamsEventArgs : EventArgs
    {
        public ChartParams ChartParams { get; }
        public ChartParamsEventArgs(ChartParams chartParams)
        {
            ChartParams = chartParams;
        }
    }
}

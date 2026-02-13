namespace FamoNET.Model
{
    public class DataSeries<T>
    {
        public Guid Guid { get; private set; }
        public List<DataPoint<T>> OriginalData { get; private set; }
        public List<DataPoint<T>> ModifiedData { get; set; }
        public string Label { get; set; }
        public string MathExpressionX { get; set; }
        public string MathExpressionY { get; set; }

        public DataSeries(List<DataPoint<T>> data, string label = null)
        {
            Guid = Guid.NewGuid();
            OriginalData = data;
            Label = label;
        }

        public DataSeries(DataSeries<T> ds)
        {
            Guid = ds.Guid;
            Label = ds.Label;
            OriginalData = new List<DataPoint<T>>(ds.OriginalData);
            ModifiedData = ds.ModifiedData != null ? new List<DataPoint<T>>(ds.ModifiedData) : new List<DataPoint<T>>();
            MathExpressionX = ds.MathExpressionX;
            MathExpressionY = ds.MathExpressionY;
        }
    }
}

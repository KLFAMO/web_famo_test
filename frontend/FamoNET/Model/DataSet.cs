using FamoNET.Model.Dto;

namespace FamoNET.Model
{
    public class DataSet
    {
        public List<DataPoint> Data { get; set; } = null;
        public string LineColor { get; set; } = null;
        public string ChartType { get; set; } = ChartTypes.Spline;

        public DataSet()
        {
            
        }

        public DataSet(DataSet dsp)
        {
            Data = new List<DataPoint>(dsp.Data);
            LineColor = dsp.LineColor;
            ChartType = dsp.ChartType;
        }

        public DataSet(DataSetDto dto)
        {
            Data = new List<DataPoint>();
            for (int i=0; i<dto.MJD.Count; ++i)            
                Data.Add(new DataPoint(dto.MJD[i], dto.Values[i]));            
        }
    }
}

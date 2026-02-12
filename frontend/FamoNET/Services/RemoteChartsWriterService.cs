using FamoNET.Model;
using FamoNET.Model.Args;

namespace FamoNET.Services
{
    public class RemoteChartsWriterService
    {
        public event EventHandler<GenericEventArgs<string>> FileAdded;
        public event EventHandler<GenericEventArgs<(string dataFolder, List<DataPoint<double>> points)>> RequestWizard;
        public void RaiseRequestWizard(string dataFolder, List<DataPoint<double>> points)
        {
            RequestWizard?.Invoke(this, new GenericEventArgs<(string dataFolder, List<DataPoint<double>> points)>((dataFolder, points)));
        }

        public void WriteFile(FileStatus file, string dataFolder, List<DataPoint<double>> points, string parameters = null, string units = null) 
        {            
            using (StreamWriter writetext = new StreamWriter(Path.Combine(dataFolder, file.Name)))
            {
                if (parameters !=  null)
                {
                    writetext.WriteLine($"Parameters: {parameters}");
                }

                if(file.Description != null && file.Description.Length > 0)
                {
                    writetext.WriteLine("$Description: {file.Description}");
                }

                if (units != null)
                {
                    writetext.WriteLine($"Units: {units}");
                }
                    
                foreach (var point in points)
                {
                    writetext.WriteLine($"{point.X} {point.Y}");
                }
            }

            FileAdded?.Invoke(this, new GenericEventArgs<string>(dataFolder));
        }        
    }
}

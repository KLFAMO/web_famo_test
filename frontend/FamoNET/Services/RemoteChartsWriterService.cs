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

        public void WriteFile(FileStatus file, string dataFolder, List<DataPoint<double>> points) 
        {            
            using (StreamWriter writetext = new StreamWriter(Path.Combine(dataFolder, file.Name)))
            {
                writetext.WriteLine(file.Description);
                foreach (var point in points)
                {
                    writetext.WriteLine($"{point.X} {point.Y}");
                }
            }

            FileAdded?.Invoke(this, new GenericEventArgs<string>(dataFolder));
        }        
    }
}

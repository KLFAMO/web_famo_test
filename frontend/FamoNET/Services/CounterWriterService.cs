using FamoNET.Model;

namespace FamoNET.Services
{
    public class CounterWriterService
    {
        private string _filename = string.Empty;
        private StreamWriter _writer;
        private double _lastTime = 0;

        private int[] _channelIndices;        

        public CounterWriterService()
        {
        }

        public void Write(List<CounterChannel> channels)
        {
            if (_writer == null)
                return;

            string line = $"{Math.Round(_lastTime, 3)} ";

            foreach (var channel in channels)
            {
                if (_channelIndices.Contains(channel.Id))
                    line += $"{channel.Frequency.ToString("0.#")} ";
            }

            _writer.WriteLine(line);
            _lastTime += 1.0 / 100; //change later. Rate needs to be dynamic
        }

        public void ConfigureAndStartSave(string fileName, int[] channelIndices)
        {
            _lastTime = 0;
            _channelIndices = channelIndices;
            if (!Directory.Exists("Data"))
                Directory.CreateDirectory("Data");

            _writer = new StreamWriter(Path.Combine("Data", fileName));            
        }

        public void StopSave()
        {            
            _writer?.Dispose();
            _writer = null;
        }

        public void Dispose()
        {            
            _writer?.Dispose();
        }
    }
}

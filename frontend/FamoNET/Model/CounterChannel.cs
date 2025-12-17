namespace FamoNET.Model
{
    public class CounterChannel
    {
        public int Id { get; set; }
        public double Frequency { get; set; }

        public CounterChannel(int id, double frequency)
        {
            Id = id;
            Frequency = frequency;
        }
    }
}

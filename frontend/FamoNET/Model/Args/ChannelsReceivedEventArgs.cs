namespace FamoNET.Model.Args
{
    public class ChannelsReceivedEventArgs : EventArgs
    {
        public List<CounterChannel> Channels { get; }

        public ChannelsReceivedEventArgs(List<CounterChannel> channels)
        {
            Channels = channels;
        }
    }
}

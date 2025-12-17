namespace FamoNET.Model
{
    public class RemoteChartsSubscriptionInfo
    {
        public Guid SessionId { get; } = Guid.NewGuid();
        public string IP { get; set; }
        public int Port { get; set; }
        public int Rate { get; set; }
        public DateTime LastFetch { get; set; }
       
        public override bool Equals(object obj)
        {
            if (obj is RemoteChartsSubscriptionInfo other)
            {
                return SessionId == other.SessionId &&
                       string.Equals(IP, other.IP) &&
                       string.Equals(Port, other.Port);
            }
            return false;
        }
        
        public override int GetHashCode()
        {            
            return HashCode.Combine(SessionId, IP, Port, Rate);
        }

        public static bool operator ==(RemoteChartsSubscriptionInfo left, RemoteChartsSubscriptionInfo right)
        {
            if (left is null && right is null) return true;
            if (left is null || right is null) return false;
            return left.Equals(right);
        }

        public static bool operator !=(RemoteChartsSubscriptionInfo left, RemoteChartsSubscriptionInfo right)
        {
            return !(left == right);
        }
    }
}

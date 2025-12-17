namespace FamoNET.Utils
{
    public static class FamoMath
    {
        public static DateTime Convert_MJDToDateTime(double mjd)
        {
            DateTime mjdEpoch = new DateTime(1858, 11, 17, 0, 0, 0, DateTimeKind.Utc);
            DateTime result = mjdEpoch.AddDays(mjd);
            return result;
        }

        public static double Convert_DateTimeToMjd(DateTime date)
        {            
            DateTime mjdEpoch = new DateTime(1858, 11, 17, 0, 0, 0, DateTimeKind.Utc);         
            TimeSpan difference = date.ToUniversalTime() - mjdEpoch;
            
            return difference.TotalDays;
        }
    }
}

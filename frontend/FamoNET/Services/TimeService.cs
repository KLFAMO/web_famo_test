namespace FamoNET.Services
{
    public class TimeService
    {
        public static double GetJulianDate(DateTime dateTime)
        {
            int year = dateTime.Year;
            int month = dateTime.Month;
            double day = dateTime.Day + (dateTime.Hour / 24.0) + (dateTime.Minute / 1440.0) + (dateTime.Second / 86400.0);

            if (month <= 2)
            {
                year -= 1;
                month += 12;
            }

            int A = year / 100;
            int B = 2 - A + (A / 4);

            return Math.Floor(365.25 * (year + 4716))
                                   + Math.Floor(30.6001 * (month + 1))
                                   + day + B - 1524.5;
        }
    }
}

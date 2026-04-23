using FamoNET.Model;
using NLog;
using org.mariuszgromada.math.mxparser;

namespace FamoNET.Utils
{
    public static class FamoMath
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
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

        public static List<DataPoint<double>> Allan(List<double> frequencies, double tau0, AllanTauMode tauMode)
        {
            var result = new List<DataPoint<double>>();

            int m = 1;
            int decade = 1;

            while (m < frequencies.Count / 5)
            {
                int M = frequencies.Count / m;

                //calculate y_1, y_2,..., y_n avgs
                List<double> avgs = new();
                double sum = 0;

                for (int i = 0; i < frequencies.Count; ++i)
                {
                    sum += frequencies[i];

                    if ((i + 1) % m == 0)
                    {
                        avgs.Add(sum / m);
                        sum = 0;
                    }
                }

                if (avgs.Count != M)
                    throw new InvalidOperationException();

                //calculate sum of avgs
                sum = 0;
                for (int i = 0; i < M - 1; ++i)
                {
                    sum += Math.Pow(avgs[i + 1] - avgs[i], 2);
                }

                double sig = Math.Sqrt(1.0 / (2.0 * (M - 1)) * sum);
                double tau = m * tau0;
                result.Add(new DataPoint<double>() { X = tau, Y = sig });

                //increment m
                if (tauMode == AllanTauMode.AllTau)
                {
                    m += 1;
                }
                else if (tauMode == AllanTauMode.Octave)
                {
                    m *= (int)tauMode;
                }
                else if (tauMode == AllanTauMode.Decade)
                {
                    if (m % (decade * 4) == 0)
                    {
                        decade *= 10;
                        m = decade;
                    }
                    else if (m % (decade * 2) == 0)
                    {
                        m = decade * 4;
                    }
                    else
                    {
                        m = decade * 2;
                    }
                }
            }

            return result;
        }
        public static List<DataPoint<double>> OverlappingAllan(List<double> frequencies, double tau0, AllanTauMode tauMode)
        {
            var result = new List<DataPoint<double>>();

            int N = frequencies.Count;
            int m = 1;
            int decade = 1;
            while (m < frequencies.Count / 4)
            {
                double[] cumSum = new double[N + 1];
                cumSum[0] = 0.0;
                for (int i = 0; i < N; i++)
                {
                    cumSum[i + 1] = cumSum[i] + frequencies[i];
                }

                double sumSqDiff = 0.0;
                int count = 0;

                for (int i = 0; i <= N - 2 * m; i++)
                {
                    double avg1 = (cumSum[i + m] - cumSum[i]) / m;
                    double avg2 = (cumSum[i + 2 * m] - cumSum[i + m]) / m;

                    double diff = avg2 - avg1;
                    sumSqDiff += diff * diff;
                    count++;
                }

                double sig = Math.Sqrt(sumSqDiff / (2.0 * count));
                               
                double tau = m * tau0;
                result.Add(new DataPoint<double>() { X = tau, Y = sig });

                //increment m
                if (tauMode == AllanTauMode.AllTau)
                {
                    m += 1;
                }
                else if (tauMode == AllanTauMode.Octave)
                {
                    m *= (int)tauMode;
                }
                else if (tauMode == AllanTauMode.Decade)
                {
                    if (m % (decade * 4) == 0)
                    {
                        decade *= 10;
                        m = decade;
                    }
                    else if (m % (decade * 2) == 0)
                    {
                        m = decade * 4;
                    }
                    else
                    {
                        m = decade * 2;
                    }
                }
            }

            return result;
        }
        public static List<DataPoint<double>> ModifiedAllan(List<double> frequencies, double tau0, AllanTauMode tauMode)
        {
            var result = new List<DataPoint<double>>();

            int m = 1;
            int decade = 1;

            while (m < frequencies.Count / 4)
            {
                int N = frequencies.Count;

                double[] cumSum = new double[N + 1];
                cumSum[0] = 0.0;
                for (int i = 0; i < N; i++)
                {
                    cumSum[i + 1] = cumSum[i] + frequencies[i];
                }

                double sumSq = 0.0;
                int count = 0;

                double currentModifiedSum = 0.0;
                for (int k = 0; k < m; k++)
                {
                    double avgA = (cumSum[k + m] - cumSum[k]) / m;
                    double avgB = (cumSum[k + 2 * m] - cumSum[k + m]) / m;
                    double diff = avgB - avgA;

                    currentModifiedSum += diff;
                }

                sumSq += currentModifiedSum * currentModifiedSum;
                count++;

                for (int j = 1; j <= N - 3 * m; j++)
                {
                    int leavingIdx = j - 1;
                    double avgA_out = (cumSum[leavingIdx + m] - cumSum[leavingIdx]) / m;
                    double avgB_out = (cumSum[leavingIdx + 2 * m] - cumSum[leavingIdx + m]) / m;
                    double diffOut = avgB_out - avgA_out;

                    int enteringIdx = j + m - 1;
                    double avgA_in = (cumSum[enteringIdx + m] - cumSum[enteringIdx]) / m;
                    double avgB_in = (cumSum[enteringIdx + 2 * m] - cumSum[enteringIdx + m]) / m;
                    double diffIn = avgB_in - avgA_in;

                    currentModifiedSum = currentModifiedSum - diffOut + diffIn;

                    sumSq += currentModifiedSum * currentModifiedSum;
                    count++;
                }

                double sig = Math.Sqrt(sumSq / (2.0 * m * m * count));
                double tau = m * tau0;
                result.Add(new DataPoint<double>() { X = tau, Y = sig });

                if (tauMode == AllanTauMode.AllTau)
                {
                    m += 1;
                }
                else if (tauMode == AllanTauMode.Octave)
                {
                    m *= (int)tauMode;
                }
                else if (tauMode == AllanTauMode.Decade)
                {
                    if (m % (decade * 4) == 0)
                    {
                        decade *= 10;
                        m = decade;
                    }
                    else if (m % (decade * 2) == 0)
                    {
                        m = decade * 4;
                    }
                    else
                    {
                        m = decade * 2;
                    }
                }
            }

            return result;
        }    
        public static DataSeries<double> ApplyMathFormulaToDataSeries(string xFormula, string yFormula, ref DataSeries<double> dataSeries)
        {                        
            Argument x = new Argument("x");
            Argument y = new Argument("y");
            var xe = new Expression(xFormula, x);
            var ye = new Expression(yFormula, y);

            if ((!string.IsNullOrWhiteSpace(xFormula) && !xe.checkSyntax()) || (!string.IsNullOrWhiteSpace(xFormula) && !ye.checkSyntax()))
            {
                throw new InvalidDataException("Invalid math exception");
            }
                

            dataSeries.ModifiedData = new List<DataPoint<double>>();

            foreach (var point in dataSeries.OriginalData)
            {
                var modifiedDataPoint = new DataPoint<double>();

                if (!string.IsNullOrWhiteSpace(xFormula))
                {
                    x.setArgumentValue(point.X);
                    modifiedDataPoint.X = xe.calculate();
                }
                else
                {
                    modifiedDataPoint.X = point.X;
                }

                if (!string.IsNullOrWhiteSpace(yFormula))
                {
                    y.setArgumentValue(point.Y);
                    modifiedDataPoint.Y = ye.calculate();
                }
                else
                {
                    modifiedDataPoint.Y = point.Y;
                }

                dataSeries.ModifiedData.Add(modifiedDataPoint);
            }

            dataSeries.MathExpressionX = xFormula;
            dataSeries.MathExpressionY = yFormula;

            return dataSeries;
        }
    }
}

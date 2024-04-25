using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
class Program
{
    public static void Main()
    {
        // Process.Start("perf",  "stat record -e \"/power/energy-cores/\" -a /home/blitzcrank/p6/TestRunner/oop");
        // Process.Start("perf",  "stat report");
        Process perf = new Process();
        ProcessStartInfo perfStartInfo = new ProcessStartInfo("perf");
        perfStartInfo.Arguments = ("stat -e cache-references,cache-misses,\"power/energy-cores/\" -a /home/blitzcrank/p6/oop/bin/Release/net8.0/oop /home/blitzcrank/p6/Replay/game");
        perfStartInfo.UseShellExecute = false;
        perfStartInfo.RedirectStandardError = true;
        perf.StartInfo = perfStartInfo;
        List<MeasurementResults> results = new List<MeasurementResults>();
        for(int i = 0; i<100; i++)
        {
            perf.Start();
            StreamReader reader = perf.StandardError;
            string output = reader.ReadToEnd();
            results.Add(new MeasurementResults(output));
        }

        DateTime time = DateTime.Now;
        StreamWriter writer = new StreamWriter("../Measurements/" + time.ToString("yyMMddH"));
        writer.WriteLine("CacheRefs;CacheMisses;Energy(J);time(s)");
        foreach(var measurement in results)
        {
            writer.WriteLine(measurement.ToString());
        }
        writer.Close();
    }
}

class MeasurementResults
{
    public MeasurementResults(string perfString)
    {
        MatchCollection matchesINT = INTRegex.Matches(perfString);
        MatchCollection matchesFP = FPRegex.Matches(perfString);

        CacheRefs = int.Parse(matchesINT[0].Value, NumberStyles.AllowThousands);
        CacheMisses = int.Parse(matchesINT[1].Value, NumberStyles.AllowThousands);
        Energy = double.Parse(matchesFP[1].Value);
        Seconds = double.Parse(matchesFP[2].Value);
    }
    Regex FPRegex = new Regex(@"\d*\.\d*");
    Regex INTRegex = new Regex(@"\d{1,3}(\,\d{3})*");

    public double Energy;
    public double Seconds;
    public int CacheRefs;
    public int CacheMisses;
    public override string ToString()
    {
        return CacheRefs+";"+CacheMisses+";"+Energy+";"+ Seconds;
    }
}
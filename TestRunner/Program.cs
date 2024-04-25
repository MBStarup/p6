using System.Diagnostics;
using System.Text.RegularExpressions;
class Program
{
    public static void Main()
    {
        // Process.Start("perf",  "stat record -e \"/power/energy-cores/\" -a /home/blitzcrank/p6/TestRunner/oop");
        // Process.Start("perf",  "stat report");
        Process perf = new Process();
        ProcessStartInfo perfStartInfo = new ProcessStartInfo("perf");
        perfStartInfo.Arguments = ("stat -e \"power/energy-cores/\" -a /home/blitzcrank/p6/oop/bin/Release/net8.0/oop");
        perfStartInfo.UseShellExecute = false;
        perfStartInfo.RedirectStandardError = true;
        perf.StartInfo = perfStartInfo;
        List<MeasurementResults> results = new List<MeasurementResults>();
        for(int i = 0; i<20; i++)
        {
            perf.Start();
            StreamReader reader = perf.StandardError;
            string output = reader.ReadToEnd();
            results.Add(new MeasurementResults(output));
        }
        Console.WriteLine("Energy | Time");
        foreach(var m in results)
        {
            Console.WriteLine(m.Energy + " | " + m.Seconds);
        }
    }
}

class MeasurementResults
{
    public MeasurementResults(string perfString)
    {
        MatchCollection matches = regex.Matches(perfString);
        
        Energy = double.Parse(matches[0].Value);
        Seconds = double.Parse(matches[1].Value);
    }
    Regex regex = new Regex(@"\d*\.\d*");
    public double Energy;
    public double Seconds;
}
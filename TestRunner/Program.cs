using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;

class Program
{
    public const string PERFOPTIONS = "stat -e cache-references,cache-misses,\"power/energy-cores/\" -a";
    public const string PATHTOOOP = "/home/blitzcrank/p6/oop/bin/Release/net8.0/oop";
    public const string PATHTOREPLAYS = "/home/blitzcrank/p6/Replay/";
    public const string REPLAYNAME = "game";
    public static void Main(string[] args)
    {
        bool OOP = false;
        bool DOP = false;
        int iterations = 100;
        if(args.Length == 0)
        {
            Console.WriteLine("Must specify paradigm");
        }
        else
        {
            if(args[0] == "oop")
            {
                OOP = true;
            }
            if(args[0] == "dop")
            {
                DOP = true;
            }
        }
        if(args.Length >= 2)
        {
            if(!int.TryParse(args[1],out iterations))
            {
                Console.WriteLine("Number of iterations must be an integer");
            }
        }
        if(OOP)
        {
            TestRunner testRunner = new TestRunner(PERFOPTIONS, PATHTOOOP, PATHTOREPLAYS, REPLAYNAME);
            testRunner.RunTest(iterations,"OOP");
        }

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

class TestRunner
{
    public TestRunner(string perfoptions, string pathtoexe, string pathtoreplays, string replay)
    {
        perf = new Process();
        string arguments = $"{perfoptions} {pathtoexe} {pathtoreplays}{replay}";
        perfStartInfo = new ProcessStartInfo("perf");
        perfStartInfo.Arguments = arguments;
        perfStartInfo.UseShellExecute = false;
        perfStartInfo.RedirectStandardError = true;
        perf.StartInfo = perfStartInfo;
    }
    Process perf;
    ProcessStartInfo perfStartInfo;

    public void RunTest(int iterations, string paradigm)
    {
        List<MeasurementResults> results = new List<MeasurementResults>();
        for(int i = 0; i<iterations; i++)
        {
            perf.Start();
            StreamReader reader = perf.StandardError;
            string output = reader.ReadToEnd();
            results.Add(new MeasurementResults(output));
        }

        DateTime time = DateTime.Now;
        StreamWriter writer = new StreamWriter("../Measurements/"+paradigm+time.ToString("yyMMddHHmm"));
        writer.WriteLine("CacheRefs;CacheMisses;Energy(J);time(s)");
        foreach(var measurement in results)
        {
            writer.WriteLine(measurement.ToString());
        }
        writer.Close();
    }
}
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;

class Program
{
    public const string PERFOPTIONS = "stat -e cache-references,cache-misses,\"power/energy-cores/\" -a";
    public static List<(string Path, string Name)> Games = [];
    public static Queue<string> Replays = [];
    public static string USAGE = $"Usage: {System.AppDomain.CurrentDomain.FriendlyName} iterations {{-p game [-n name]}}... replay...";

    private static void ExitWithUsage()
    {
        Console.WriteLine(USAGE);
        Environment.Exit(-1);
    }
    public static void Main(string[] args)
    {
        if (args.Length < 4) ExitWithUsage();

        var argQueue = new Queue<string>(args);

        if (!int.TryParse(argQueue.Dequeue(), out int iterations)) ExitWithUsage();

        var nextArg = argQueue.Peek();
        while (nextArg == "-p")
        {
            argQueue.Dequeue(); //remove the -p
            var game = argQueue.Dequeue();
            var name = game;
            nextArg = argQueue.Peek();
            if (nextArg == "-n")
            {
                argQueue.Dequeue(); //remove the -n
                name = argQueue.Dequeue();
                nextArg = argQueue.Peek();
            }
            Games.Add((game, name));
        }

        Replays = argQueue; //the rest of the args are replays

        System.Console.WriteLine($"Running {iterations} iterations of replays [{(String.Join("; ", Replays))}] with games [{(String.Join("; ", Games))}]");


        foreach (var replay in Replays)
        {
            foreach (var game in Games)
            {
                TestRunner testRunner = new TestRunner(PERFOPTIONS, game.Path, replay);
                testRunner.RunTest(iterations, game.Name);
            }
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
        return CacheRefs + ";" + CacheMisses + ";" + Energy + ";" + Seconds;
    }
}

class TestRunner
{
    private string replay;
    public TestRunner(string perfoptions, string pathtoexe, string pathtoreplay)
    {
        replay = Path.GetFileNameWithoutExtension(pathtoreplay);
        perf = new Process();
        string arguments = $"{perfoptions} {pathtoexe} {pathtoreplay}";
        perfStartInfo = new ProcessStartInfo("perf");
        perfStartInfo.Arguments = arguments;
        perfStartInfo.UseShellExecute = false;
        perfStartInfo.RedirectStandardError = true;
        perf.StartInfo = perfStartInfo;
    }
    Process perf;
    ProcessStartInfo perfStartInfo;

    public void RunTest(int iterations, string name)
    {
        System.Console.Write($"Running test with replay {replay} on game {name}: ");
        var (cursor_left, cursor_top) = Console.GetCursorPosition();
        var outfile = Path.Combine("..", "Measurements", $"{replay}_{name}_{DateTime.Now.ToString("yyMMddHHmm")}_{iterations}.data");
        List<MeasurementResults> results = new List<MeasurementResults>();
        for (int i = 0; i < iterations; i++)
        {
            Console.SetCursorPosition(cursor_left, cursor_top);
            System.Console.Write(i);
            perf.Start();
            StreamReader reader = perf.StandardError;
            string output = reader.ReadToEnd();
            results.Add(new MeasurementResults(output));
        }
        System.Console.WriteLine($"\nDone, writing to file {outfile}\n");

        StreamWriter writer = new StreamWriter(outfile);
        writer.WriteLine("CacheRefs;CacheMisses;Energy(J);time(s)");
        foreach (var measurement in results) writer.WriteLine(measurement.ToString());
        writer.Close();
    }
}
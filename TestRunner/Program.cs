using System;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;

class Program
{
    public static string ParseInt(string str)  {System.Console.WriteLine($"parsing int: {str}"); return Int64.Parse(str, NumberStyles.AllowThousands).ToString();}
    public static string ParseDouble(string str) => double.Parse(str, NumberStyles.AllowThousands | NumberStyles.AllowDecimalPoint).ToString();
    public static (string Name, Func<string, string> Parser)[] PERFSTATS = [
        ("cache-references",            ParseInt),
        ("cache-misses",                ParseInt),
        ("L1-dcache-loads",             ParseInt),
        ("L1-dcache-load-misses",       ParseInt),
        ("L1-dcache-stores",            ParseInt),
        ("L1-icache-load-misses",       ParseInt),
        ("LLC-loads",                   ParseInt),
        ("LLC-load-misses",             ParseInt),
        ("LLC-stores",                  ParseInt),
        ("LLC-store-misses",            ParseInt),
        ("power/energy-cores/",         ParseDouble)
    ];

    public static string PERFOPTIONS = $"stat -e \"{String.Join("\",\"",PERFSTATS.Select(x => x.Name))}\" -a";
    public static List<(string Path, string Name)> Games = [];
    public static Queue<string> Replays = [];
    public static string USAGE = $"Usage: {System.AppDomain.CurrentDomain.FriendlyName} iterations {{-p game [-n name]}}... replay...";

    private static void ExitWithError(string error)
    {
        Console.WriteLine(error);
        Environment.Exit(-1);
    }
    private static void ExitWithUsage() => ExitWithError(USAGE);
    public static void Main(string[] args)
    {    
        if (args.Length < 4) ExitWithUsage();

        var argQueue = new Queue<string>(args);
        if (argQueue.Peek().ToLower() == "-h" || argQueue.Peek().ToLower() == "--help") ExitWithUsage();

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

        foreach (var game in Games) if (!File.Exists(game.Path)) ExitWithError($"Could not find game {game.Path}");
        foreach (var replay in Replays) if (!File.Exists(replay)) ExitWithError($"Could not find replay {replay}");

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
        List<string> results = new List<string>(iterations);
        for (int i = 0; i < iterations; i++)
        {
            StringBuilder result = new();
            Console.SetCursorPosition(cursor_left, cursor_top);
            System.Console.Write(i + 1);
            perf.Start();
            StreamReader reader = perf.StandardError;
            System.Console.WriteLine("--------------");
            for (int j = 0; j < 3; j++)reader.ReadLine(); //skip empty/usless lines
            for (int j = 0; j < Program.PERFSTATS.Count(); j++) {result.Append(Program.PERFSTATS[j].Parser(reader.ReadLine().Split(" ", StringSplitOptions.RemoveEmptyEntries)[0])); result.Append(";");}
            for (int j = 0; j < 1; j++)reader.ReadLine(); //skip empty line
            for (int j = 0; j < 1; j++) result.Append(Program.ParseDouble(reader.ReadLine().Split(" ", StringSplitOptions.RemoveEmptyEntries)[0])); //read time

            results.Add(result.ToString());
        }
        System.Console.WriteLine($"\nDone, writing to file {outfile}\n");

        StreamWriter writer = new StreamWriter(outfile);
        writer.WriteLine($"{String.Join(";",Program.PERFSTATS.Select(x => x.Name))};time");
        foreach (var result in results) writer.WriteLine(result.ToString());
        writer.Close();
    }
}
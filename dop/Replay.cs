namespace P6.DOP;

public class Recording
{
    public Recording()
    {
        loopInputs = new List<LoopInputs>();
    }
    public StreamWriter writer;
    public List<LoopInputs> loopInputs;
    public void recordState(Game game)
    {
        loopInputs.Add(new LoopInputs(game.DeltaTime, game.KeyState));
    }
    public void saveRecording()
    {
        var ReplayFolder = Path.Combine("..", "Replays");
        System.IO.Directory.CreateDirectory(ReplayFolder);
        var file = File.Open(Path.Combine(ReplayFolder, $"{DateTime.Now.ToString("yyyy-M-dd-HH-mm-ss")}.replay"), System.IO.FileMode.OpenOrCreate);
        using (StreamWriter writer = new StreamWriter(file))
        {
            System.Console.WriteLine($"Saving replay to {file.Name}");
            foreach (LoopInputs loop in loopInputs)
            {
                writer.WriteLine(loop.ToString());
            }
            writer.WriteLine("GAMEEND");
        }
    }
}
public class LoopInputs
{
    public LoopInputs(float deltaTime, KeyState keystate)
    {
        DeltaTime = deltaTime;
        KeyState = new KeyState
        {
            Up = keystate.Up,
            Down = keystate.Down,
            Left = keystate.Left,
            Right = keystate.Right,
            Shoot = keystate.Shoot,
            WeaponChoice = keystate.WeaponChoice
        };

    }
    public LoopInputs(float deltaTime, string values)
    {
        DeltaTime = deltaTime;
        KeyState = new KeyState
        {
            Up = uint.Parse(values[0].ToString()),
            Down = uint.Parse(values[1].ToString()),
            Left = uint.Parse(values[2].ToString()),
            Right = uint.Parse(values[3].ToString()),
            Shoot = uint.Parse(values[4].ToString()),
            WeaponChoice = uint.Parse(values[5].ToString())
        };
    }
    public float DeltaTime;
    public KeyState KeyState;
    private string keyStateString()
    {
        return KeyState.Up.ToString() + KeyState.Down.ToString() + KeyState.Left.ToString() + KeyState.Right.ToString() + KeyState.Shoot.ToString() +
        KeyState.WeaponChoice.ToString();
    }
    public override string ToString()
    {
        return DeltaTime.ToString() + " " + keyStateString();
    }
}

public class Replay
{
    public Replay(string replayFile)
    {
        inputs = new List<LoopInputs>();
        reader = new StreamReader(replayFile);
        while ((line = reader.ReadLine()) != "GAMEEND")
        {
            splitline = line.Split(" ");
            inputs.Add(new LoopInputs(float.Parse(splitline[0]), splitline[1]));
        }
    }
    public List<LoopInputs> inputs;
    private StreamReader reader;
    private string[] splitline;
    private string line;
    private int replaystep = 0;
    public void update(Game game)
    {
        if (replaystep >= inputs.Count)
        {
            game.KeyState.Close = 1;
        }
        else
        {
            game.DeltaTime = inputs[replaystep].DeltaTime;
            game.KeyState = inputs[replaystep].KeyState;
            replaystep++;
        }
    }
}
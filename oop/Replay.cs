
class Recording
{
    public Recording()
    {
        loopInputs = new List<LoopInputs>();
        currentTimeStamp = 0;
    }
    public StreamWriter writer;
    public List<LoopInputs> loopInputs;
    public float currentTimeStamp;
    public void recordState(Game game)
    {
        currentTimeStamp += game.DeltaTime;
        loopInputs.Add(new LoopInputs(currentTimeStamp, game.KeyState));
    }
    public void saveRecording()
    {
        writer = new StreamWriter("game");
        foreach(LoopInputs loop in loopInputs)
        {
            writer.WriteLine(loop.ToString());
        }
        writer.WriteLine("GAMEEND");
        writer.Close();
    }
}
class LoopInputs
{
    public LoopInputs(float timestamp, KeyState keystate)
    {
        TimeStamp = timestamp;
        KeyState = new KeyState
        {
            Up = keystate.Up,
            Down = keystate.Down,
            Left = keystate.Left,
            Right = keystate.Right,
            Shoot = keystate.Shoot
        };

    }
    public LoopInputs(float timestamp, string values)
    {
        TimeStamp = timestamp;
        KeyState = new KeyState
        {
            Up = uint.Parse(values[0].ToString()),
            Down = uint.Parse(values[1].ToString()),
            Left = uint.Parse(values[2].ToString()),
            Right = uint.Parse(values[3].ToString()),
            Shoot = int.Parse(values[4].ToString())
        };
    }
    public float TimeStamp;
    public KeyState KeyState;
    private string keyStateString()
    {
        return KeyState.Up.ToString() + KeyState.Down.ToString() + KeyState.Left.ToString() + KeyState.Right.ToString() + KeyState.Shoot.ToString();
    }
    public override string ToString()
    {
        return TimeStamp.ToString() + " " + keyStateString();
    }
}

class Replay
{
    public Replay(string replayFile)
    {
        inputs = new List<LoopInputs>();
        reader = new StreamReader(replayFile);
        while((line = reader.ReadLine())!= "GAMEEND")
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
    public void updateInputs(Game game)
    {
        if(replaystep >= inputs.Count)
        {
            game.KeyState.Close = 1;
        }
        else
        {
            game.KeyState.Up = inputs[replaystep].KeyState.Up;
            game.KeyState.Down = inputs[replaystep].KeyState.Down;
            game.KeyState.Left = inputs[replaystep].KeyState.Left;
            game.KeyState.Right = inputs[replaystep].KeyState.Right;
            game.KeyState.Shoot = inputs[replaystep].KeyState.Shoot;
            replaystep++;
        }
    }
}
#define RENDERING
#define INPUT
#define _RECORDING
#define _REPLAYING
using System;
using System.Diagnostics;
using System.Numerics;
using SDL2;
using CircularBuffer;



namespace P6.DOP;
public static class Program
{
    public const int WINDOW_X = 1200;
    public const int WINDOW_Y = 800;

    public static void Main(string[] args)
    {
        Game game = new(69420);

        // setup the "scene"
        Vector2 maxPos = new(Program.WINDOW_X, Program.WINDOW_Y);
        for (int i = 0; i < 100; i++)
        {
            var axeMan = new AxeMan
            {
                Position = new Vector2((float)game.Rand.NextDouble(), (float)game.Rand.NextDouble()) * maxPos
            };
            axeMan.Collider.R = 12.5f;
            axeMan.ID = (uint)i;
            axeMan.Color = 0xFFFF0000;
            game.AxeMans.Add(axeMan);
        }

        for (int i = 0; i < 100; i++)
        {
            var slime = new Slime
            {
                Position = new Vector2((float)game.Rand.NextDouble(), (float)game.Rand.NextDouble()) * maxPos
            };
            slime.Collider.R = 12.5f;
            slime.ID = (uint)i;
            slime.Color = 0xFF00FF00;
            game.Slimes.Add(slime);
        }
        Player player = new Player { Position = new Vector2(100, 100) };
        player.Collider.R = 12.5f;
        game.Players.Add(player);


        game.Run();
    }
}

public struct AxeMan
{
    public uint ID;
    public uint Color;
    public Circle Collider;
    public Vector2 Position { get => Collider.Center; set => Collider.Center = value; }
    public Vector2 Velocity;
    public static void OnRemove(Game game, AxeMan axeMan) { }
    public static void Update(Game game, ref AxeMan axeMan)
    {
        foreach (AxeMan otherAxeMan in game.AxeMans)
        {
            if (otherAxeMan.ID != axeMan.ID && otherAxeMan.Collider.Overlaps(axeMan.Collider))
            {
                while (axeMan.Collider.Overlaps(otherAxeMan.Collider))
                {
                    var direction = Vector2.Normalize(axeMan.Collider.Center - otherAxeMan.Collider.Center);
                    axeMan.Position += new Vector2(direction.X, direction.Y) * 0.1f;
                }
            }
        }
        foreach (Slime slime in game.Slimes)
        {
            if (axeMan.Collider.Overlaps(slime.Collider))
            {
                while (axeMan.Collider.Overlaps(slime.Collider))
                {
                    var direction = Vector2.Normalize(axeMan.Collider.Center - slime.Collider.Center);
                    axeMan.Position += new Vector2(direction.X, direction.Y) * 0.1f;
                }
            }
        }
        foreach (Player player in game.Players)
        {
            if (axeMan.Collider.Overlaps(player.Collider))
            {
                while (axeMan.Collider.Overlaps(player.Collider))
                {
                    var direction = Vector2.Normalize(axeMan.Collider.Center - player.Collider.Center);
                    axeMan.Position += new Vector2(direction.X, direction.Y) * 0.1f;
                }
            }
        }
        foreach (LootBox lootBox in game.LootBoxs)
        {
            if (axeMan.Collider.Overlaps(lootBox.Collider)) { }
        }
        foreach (Shell shell in game.Shells)
        {
            if (axeMan.Collider.Overlaps(shell.Collider)) { }
        }
        foreach (Bolt bolt in game.Bolts)
        {
            if (axeMan.Collider.Overlaps(bolt.Collider)) { }
        }
        foreach (Arrow arrow in game.Arrows)
        {
            if (axeMan.Collider.Overlaps(arrow.Collider)) { }
        }
    }
}
public struct Slime
{
    public uint ID;
    public uint Color;
    public Circle Collider;
    public Vector2 Position { get => Collider.Center; set => Collider.Center = value; }
    public Vector2 Velocity;
    public static void OnRemove(Game game, Slime slime) { }
    public static void Update(Game game, ref Slime slime)
    {
        foreach (AxeMan axeMan in game.AxeMans)
        {
            if (slime.Collider.Overlaps(axeMan.Collider))
            {
                if (slime.Collider.Overlaps(axeMan.Collider))
                {
                    while (slime.Collider.Overlaps(axeMan.Collider))
                    {
                        var direction = Vector2.Normalize(slime.Collider.Center - axeMan.Collider.Center);
                        slime.Position += new Vector2(direction.X, direction.Y) * 0.1f;
                    }
                }
            }
        }
        foreach (Slime otherSlime in game.Slimes)
        {
            if (otherSlime.ID != slime.ID && otherSlime.Collider.Overlaps(slime.Collider))
            {
                if (slime.Collider.Overlaps(otherSlime.Collider))
                {
                    while (slime.Collider.Overlaps(otherSlime.Collider))
                    {
                        var direction = Vector2.Normalize(slime.Collider.Center - otherSlime.Collider.Center);
                        slime.Position += new Vector2(direction.X, direction.Y) * 0.1f;
                    }
                }
            }
        }
        foreach (Player player in game.Players)
        {
            if (slime.Collider.Overlaps(player.Collider))
            {
                if (slime.Collider.Overlaps(player.Collider))
                {
                    while (slime.Collider.Overlaps(player.Collider))
                    {
                        var direction = Vector2.Normalize(slime.Collider.Center - player.Collider.Center);
                        slime.Position += new Vector2(direction.X, direction.Y) * 0.1f;
                    }
                }
            }
        }
        foreach (LootBox lootBox in game.LootBoxs)
        {
            if (slime.Collider.Overlaps(lootBox.Collider)) { }
        }
        foreach (Shell shell in game.Shells)
        {
            if (slime.Collider.Overlaps(shell.Collider)) { }
        }
        foreach (Bolt bolt in game.Bolts)
        {
            if (slime.Collider.Overlaps(bolt.Collider)) { }
        }
        foreach (Arrow arrow in game.Arrows)
        {
            if (slime.Collider.Overlaps(arrow.Collider)) { }
        }
    }
}
public struct Player()
{
    public uint ID;
    public uint Color = 0xFFFF00FF;
    public Circle Collider;
    public Vector2 Position { get => Collider.Center; set => Collider.Center = value; }
    public int CurrentWeapon { get; } //TODO: implement
    public Vector2 Direction { get; set; }
    public List<Weapon> Weapons { get; private set; } = new();

    public Vector2 Velocity;
    public int AttackCooldown;

    public static void OnRemove(Game game, Player player) { }
    public static void Update(Game game, ref Player player)
    {

        {
            float speed = 0.0025f;

            if (game.KeyState.Right > 0) player.Velocity.X += speed * game.DeltaTime;
            if (game.KeyState.Left > 0) player.Velocity.X -= speed * game.DeltaTime;
            if (game.KeyState.Down > 0) player.Velocity.Y += speed * game.DeltaTime;
            if (game.KeyState.Up > 0) player.Velocity.Y -= speed * game.DeltaTime;

            if (player.Velocity.LengthSquared() > 0.0f) player.Direction = Vector2.Normalize(player.Velocity);

            if (game.KeyState.Shoot == 1)
                if (player.AttackCooldown <= 0)
                {
                    player.Weapons[player.CurrentWeapon].Attack(game, player);
                }

            foreach (Weapon weapon in player.Weapons)
            {
                weapon.Update(game);
            }

            {
                player.Position = player.Position + player.Velocity * game.DeltaTime;
                player.Velocity *= MathF.Pow(0.996f, game.DeltaTime); // drag
            }
        }

        foreach (AxeMan axeMan in game.AxeMans)
        {
            if (player.Collider.Overlaps(axeMan.Collider))
            {
                if (player.Collider.Overlaps(axeMan.Collider))
                {
                    if (player.Collider.Overlaps(axeMan.Collider))
                    {
                        while (player.Collider.Overlaps(axeMan.Collider))
                        {
                            var direction = Vector2.Normalize(player.Collider.Center - axeMan.Collider.Center);
                            player.Position += new Vector2(direction.X, direction.Y) * 0.1f;
                        }
                    }
                }
            }
        }
        foreach (Slime slime in game.Slimes)
        {
            if (player.Collider.Overlaps(slime.Collider))
            {
                if (player.Collider.Overlaps(slime.Collider))
                {
                    if (player.Collider.Overlaps(slime.Collider))
                    {
                        while (player.Collider.Overlaps(slime.Collider))
                        {
                            var direction = Vector2.Normalize(player.Collider.Center - slime.Collider.Center);
                            player.Position += new Vector2(direction.X, direction.Y) * 0.1f;
                        }
                    }
                }
            }
        }
        foreach (Player otherPlayer in game.Players)
        {
            if (otherPlayer.ID != player.ID && otherPlayer.Collider.Overlaps(player.Collider))
            {
                if (player.Collider.Overlaps(otherPlayer.Collider))
                {
                    if (player.Collider.Overlaps(otherPlayer.Collider))
                    {
                        while (player.Collider.Overlaps(otherPlayer.Collider))
                        {
                            var direction = Vector2.Normalize(player.Collider.Center - otherPlayer.Collider.Center);
                            player.Position += new Vector2(direction.X, direction.Y) * 0.1f;
                        }
                    }
                }
            }
        }
        foreach (LootBox lootBox in game.LootBoxs)
        {
            if (player.Collider.Overlaps(lootBox.Collider)) { }
        }
        foreach (Shell shell in game.Shells)
        {
            if (player.Collider.Overlaps(shell.Collider)) { }
        }
        foreach (Bolt bolt in game.Bolts)
        {
            if (player.Collider.Overlaps(bolt.Collider)) { }
        }
        foreach (Arrow arrow in game.Arrows)
        {
            if (player.Collider.Overlaps(arrow.Collider)) { }
        }


    }
}
public struct LootBox
{
    public uint ID;
    public uint Color;
    public Circle Collider;
    public Vector2 Position { get => Collider.Center; set => Collider.Center = value; }
    public Vector2 Velocity;
    public static void OnRemove(Game game, LootBox lootBox) { }
    public static void Update(Game game, ref LootBox lootBox)
    {
        foreach (AxeMan axeMan in game.AxeMans)
        {
            if (lootBox.Collider.Overlaps(axeMan.Collider)) { }
        }
        foreach (Slime slime in game.Slimes)
        {
            if (lootBox.Collider.Overlaps(slime.Collider)) { }
        }
        foreach (Player player in game.Players)
        {
            if (lootBox.Collider.Overlaps(player.Collider)) { }
        }
        foreach (LootBox otherLootBox in game.LootBoxs)
        {
            if (otherLootBox.ID != lootBox.ID && otherLootBox.Collider.Overlaps(lootBox.Collider)) { }
        }
        foreach (Shell shell in game.Shells)
        {
            if (lootBox.Collider.Overlaps(shell.Collider)) { }
        }
        foreach (Bolt bolt in game.Bolts)
        {
            if (lootBox.Collider.Overlaps(bolt.Collider)) { }
        }
        foreach (Arrow arrow in game.Arrows)
        {
            if (lootBox.Collider.Overlaps(arrow.Collider)) { }
        }
    }
}
public struct Shell
{
    public uint ID;
    public uint Color;
    public Circle Collider;
    public Vector2 Position { get => Collider.Center; set => Collider.Center = value; }
    public Vector2 Velocity;
    public static void OnRemove(Game game, Shell shell) { }
    public static void Update(Game game, ref Shell shell)
    {
        foreach (AxeMan axeMan in game.AxeMans)
        {
            if (shell.Collider.Overlaps(axeMan.Collider)) { }
        }
        foreach (Slime slime in game.Slimes)
        {
            if (shell.Collider.Overlaps(slime.Collider)) { }
        }
        foreach (Player player in game.Players)
        {
            if (shell.Collider.Overlaps(player.Collider)) { }
        }
        foreach (LootBox lootBox in game.LootBoxs)
        {
            if (shell.Collider.Overlaps(lootBox.Collider)) { }
        }
        foreach (Shell otherShell in game.Shells)
        {
            if (otherShell.ID != shell.ID && otherShell.Collider.Overlaps(shell.Collider)) { }
        }
        foreach (Bolt bolt in game.Bolts)
        {
            if (shell.Collider.Overlaps(bolt.Collider)) { }
        }
        foreach (Arrow arrow in game.Arrows)
        {
            if (shell.Collider.Overlaps(arrow.Collider)) { }
        }
    }
}
public struct Bolt
{
    public uint ID;
    public uint Color;
    public Circle Collider;
    public Vector2 Position { get => Collider.Center; set => Collider.Center = value; }
    public Vector2 Velocity;
    public static void OnRemove(Game game, Bolt bolt) { }
    public static void Update(Game game, ref Bolt bolt)
    {
        foreach (AxeMan axeMan in game.AxeMans)
        {
            if (bolt.Collider.Overlaps(axeMan.Collider)) { }
        }
        foreach (Slime slime in game.Slimes)
        {
            if (bolt.Collider.Overlaps(slime.Collider)) { }
        }
        foreach (Player player in game.Players)
        {
            if (bolt.Collider.Overlaps(player.Collider)) { }
        }
        foreach (LootBox lootBox in game.LootBoxs)
        {
            if (bolt.Collider.Overlaps(lootBox.Collider)) { }
        }
        foreach (Shell shell in game.Shells)
        {
            if (bolt.Collider.Overlaps(shell.Collider)) { }
        }
        foreach (Bolt otherBolt in game.Bolts)
        {
            if (otherBolt.ID != bolt.ID && otherBolt.Collider.Overlaps(bolt.Collider)) { }
        }
        foreach (Arrow arrow in game.Arrows)
        {
            if (bolt.Collider.Overlaps(arrow.Collider)) { }
        }
    }
}
public struct Arrow
{
    public uint ID;
    public uint Color;
    public Circle Collider;
    public Vector2 Position { get => Collider.Center; set => Collider.Center = value; }
    public Vector2 Velocity;
    public static void OnRemove(Game game, Arrow arrow) { }
    public static void Update(Game game, ref Arrow arrow)
    {
        foreach (AxeMan axeMan in game.AxeMans)
        {
            if (arrow.Collider.Overlaps(axeMan.Collider)) { }
        }
        foreach (Slime slime in game.Slimes)
        {
            if (arrow.Collider.Overlaps(slime.Collider)) { }
        }
        foreach (Player player in game.Players)
        {
            if (arrow.Collider.Overlaps(player.Collider)) { }
        }
        foreach (LootBox lootBox in game.LootBoxs)
        {
            if (arrow.Collider.Overlaps(lootBox.Collider)) { }
        }
        foreach (Shell shell in game.Shells)
        {
            if (arrow.Collider.Overlaps(shell.Collider)) { }
        }
        foreach (Bolt bolt in game.Bolts)
        {
            if (arrow.Collider.Overlaps(bolt.Collider)) { }
        }
        foreach (Arrow otherArrow in game.Arrows)
        {
            if (otherArrow.ID != arrow.ID && otherArrow.Collider.Overlaps(arrow.Collider)) { }
        }
    }
}



public class Game(int seed)
{
    public float DeltaTime;
    public Random Rand = new Random(seed);

    public List<AxeMan> AxeMans = new();
    public List<Slime> Slimes = new();
    public List<Player> Players = new();
    public List<LootBox> LootBoxs = new();
    public List<Shell> Shells = new();
    public List<Bolt> Bolts = new();
    public List<Arrow> Arrows = new();

    public List<AxeMan> AxeMansToSpawn = new();
    public List<Slime> SlimesToSpawn = new();
    public List<Player> PlayersToSpawn = new();
    public List<LootBox> LootBoxsToSpawn = new();
    public List<Shell> ShellsToSpawn = new();
    public List<Bolt> BoltsToSpawn = new();
    public List<Arrow> ArrowsToSpawn = new();

    public List<AxeMan> AxeMansToRemove = new();
    public List<Slime> SlimesToRemove = new();
    public List<Player> PlayersToRemove = new();
    public List<LootBox> LootBoxsToRemove = new();
    public List<Shell> ShellsToRemove = new();
    public List<Bolt> BoltsToRemove = new();
    public List<Arrow> ArrowsToRemove = new();

    (int X, int Y) windowSize = (Program.WINDOW_X, Program.WINDOW_Y);

    public KeyState KeyState = new();




#if RECORDING
    public Recording record;
#endif
#if REPLAYING
    public Replay replay;
#endif
    public void Run()
    {
#if RECORDING
        record = new Recording();
#endif
#if REPLAYING
        replay = new Replay("game");
#endif
#if RENDERING
        SDLTools.Assert(SDL.SDL_Init(SDL.SDL_INIT_VIDEO));

        var window = SDL.SDL_CreateWindow(
                "hello_sdl2",
                SDL.SDL_WINDOWPOS_UNDEFINED, SDL.SDL_WINDOWPOS_UNDEFINED,
                windowSize.X, windowSize.Y,
                SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN
                );
        Trace.Assert(window != 0, $"Failed to create window: {SDL.SDL_GetError()}");


        // var screenSurface = SDL.SDL_GetWindowSurface(window);
        var renderer = SDL.SDL_CreateRenderer(window, -1, 0);
        SDLTools.Assert(SDL.SDL_SetRenderDrawBlendMode(renderer, SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND));
#endif

        var watch = Stopwatch.StartNew();
        var frameTimeBuffer = new CircularBuffer<float>(100);
        while (true)
        {
            // the code that you want to measure comes here
#if INPUT
            while (SDL.SDL_PollEvent(out SDL.SDL_Event e) != 0)
            {

                if (e.type == SDL.SDL_EventType.SDL_QUIT) KeyState.Close = 1;
                if (e.type == SDL.SDL_EventType.SDL_KEYDOWN)
                {
                    if (e.key.keysym.sym == SDL.SDL_Keycode.SDLK_d) KeyState.Right = 1;
                    if (e.key.keysym.sym == SDL.SDL_Keycode.SDLK_a) KeyState.Left = 1;
                    if (e.key.keysym.sym == SDL.SDL_Keycode.SDLK_w) KeyState.Up = 1;
                    if (e.key.keysym.sym == SDL.SDL_Keycode.SDLK_s) KeyState.Down = 1;
                    if (e.key.keysym.sym == SDL.SDL_Keycode.SDLK_ESCAPE) KeyState.Close = 1;
                    if (e.key.keysym.sym == SDL.SDL_Keycode.SDLK_e) KeyState.Shoot = 1;
                }

                if (e.type == SDL.SDL_EventType.SDL_KEYUP)
                {
                    if (e.key.keysym.sym == SDL.SDL_Keycode.SDLK_d) KeyState.Right = 0;
                    if (e.key.keysym.sym == SDL.SDL_Keycode.SDLK_a) KeyState.Left = 0;
                    if (e.key.keysym.sym == SDL.SDL_Keycode.SDLK_w) KeyState.Up = 0;
                    if (e.key.keysym.sym == SDL.SDL_Keycode.SDLK_s) KeyState.Down = 0;
                    if (e.key.keysym.sym == SDL.SDL_Keycode.SDLK_ESCAPE) KeyState.Close = 0;
                    if (e.key.keysym.sym == SDL.SDL_Keycode.SDLK_e) KeyState.Shoot = 0;
                }
            }
#if !RECORDING
            if (KeyState.Close > 0) Environment.Exit(0);
#endif

#endif
#if REPLAYING
            replay.update(this);
            if (KeyState.Close > 0) Environment.Exit(0);
#endif
#if !REPLAYING
            float realDeltaTime = ((float)watch.ElapsedTicks / (float)Stopwatch.Frequency) * 1000;
            watch.Restart();
            DeltaTime = Math.Clamp(realDeltaTime, float.MinValue, 1000 / 60);
            frameTimeBuffer.PushBack(realDeltaTime);
#endif
#if RECORDING
            record.recordState(this);
            if (KeyState.Close > 0)
            {
                record.saveRecording();
                Environment.Exit(0);
            }

#endif
            // SDL.SDL_SetWindowTitle(window, $"avg frametime: {frameTimeBuffer.Aggregate(0.0, (x, y) => x + y) / frameTimeBuffer.Count()}");
            SDL.SDL_SetWindowTitle(window, $"FPS: {frameTimeBuffer.Count() / (frameTimeBuffer.Aggregate(0.0, (x, y) => x + y) / 1000)}");

            //update
            for (int i = 0; i < AxeMans.Count; i++)
            {
                AxeMan axeMan = AxeMans[i];
                AxeMan.Update(this, ref axeMan);
                AxeMans[i] = axeMan;
            }
            for (int i = 0; i < Slimes.Count; i++)
            {
                Slime slime = Slimes[i];
                Slime.Update(this, ref slime);
                Slimes[i] = slime;
            }
            for (int i = 0; i < Players.Count; i++)
            {
                Player player = Players[i];
                Player.Update(this, ref player);
                Players[i] = player;
            }
            for (int i = 0; i < LootBoxs.Count; i++)
            {
                LootBox lootBox = LootBoxs[i];
                LootBox.Update(this, ref lootBox);
                LootBoxs[i] = lootBox;
            }
            for (int i = 0; i < Shells.Count; i++)
            {
                Shell shell = Shells[i];
                Shell.Update(this, ref shell);
                Shells[i] = shell;
            }
            for (int i = 0; i < Bolts.Count; i++)
            {
                Bolt bolt = Bolts[i];
                Bolt.Update(this, ref bolt);
                Bolts[i] = bolt;
            }
            for (int i = 0; i < Arrows.Count; i++)
            {
                Arrow arrow = Arrows[i];
                Arrow.Update(this, ref arrow);
                Arrows[i] = arrow;
            }

            //remove
            foreach (AxeMan axeMan in AxeMansToRemove)
            {
                AxeMan.OnRemove(this, axeMan);
            }
            AxeMans.RemoveAll(AxeMansToRemove.Contains);
            AxeMansToRemove.Clear();
            foreach (Slime slime in SlimesToRemove)
            {
                Slime.OnRemove(this, slime);
            }
            Slimes.RemoveAll(SlimesToRemove.Contains);
            SlimesToRemove.Clear();
            foreach (Player player in PlayersToRemove)
            {
                Player.OnRemove(this, player);
            }
            Players.RemoveAll(PlayersToRemove.Contains);
            PlayersToRemove.Clear();
            foreach (LootBox lootBox in LootBoxsToRemove)
            {
                LootBox.OnRemove(this, lootBox);
            }
            LootBoxs.RemoveAll(LootBoxsToRemove.Contains);
            LootBoxsToRemove.Clear();
            foreach (Shell shell in ShellsToRemove)
            {
                Shell.OnRemove(this, shell);
            }
            Shells.RemoveAll(ShellsToRemove.Contains);
            ShellsToRemove.Clear();
            foreach (Bolt bolt in BoltsToRemove)
            {
                Bolt.OnRemove(this, bolt);
            }
            Bolts.RemoveAll(BoltsToRemove.Contains);
            BoltsToRemove.Clear();
            foreach (Arrow arrow in ArrowsToRemove)
            {
                Arrow.OnRemove(this, arrow);
            }
            Arrows.RemoveAll(ArrowsToRemove.Contains);
            ArrowsToRemove.Clear();

            //spawn stuff
            AxeMans.AddRange(AxeMansToSpawn);
            AxeMansToSpawn.Clear();
            Slimes.AddRange(SlimesToSpawn);
            SlimesToSpawn.Clear();
            Players.AddRange(PlayersToSpawn);
            PlayersToSpawn.Clear();
            LootBoxs.AddRange(LootBoxsToSpawn);
            LootBoxsToSpawn.Clear();
            Shells.AddRange(ShellsToSpawn);
            ShellsToSpawn.Clear();
            Bolts.AddRange(BoltsToSpawn);
            BoltsToSpawn.Clear();
            Arrows.AddRange(ArrowsToSpawn);
            ArrowsToSpawn.Clear();

            //render
#if RENDERING
            var bgRect = new SDL.SDL_Rect { x = 0, y = 0, w = windowSize.X, h = windowSize.Y };
            SDLTools.SetRenderDrawColor(renderer, 0xFF111111);
            SDLTools.Assert(SDL.SDL_RenderClear(renderer));

            foreach (AxeMan axeMan in AxeMans)
            {
                var color = axeMan.Color;
                axeMan.Collider.Render(renderer, color);
            }
            foreach (Slime slime in Slimes)
            {
                var color = slime.Color;
                slime.Collider.Render(renderer, color);
            }
            foreach (Player player in Players)
            {
                var color = player.Color;
                player.Collider.Render(renderer, color);
            }
            foreach (LootBox lootBox in LootBoxs)
            {
                var color = lootBox.Color;
                lootBox.Collider.Render(renderer, color);
            }
            foreach (Shell shell in Shells)
            {
                var color = shell.Color;
                shell.Collider.Render(renderer, color);
            }
            foreach (Bolt bolt in Bolts)
            {
                var color = bolt.Color;
                bolt.Collider.Render(renderer, color);
            }
            foreach (Arrow arrow in Arrows)
            {
                var color = arrow.Color;
                arrow.Collider.Render(renderer, color);
            }

            { //scope (for name "player"), refactor out whenever
                try
                {

                    Player player = Players[0];
                    {
                        SDLTools.SetRenderDrawColor(renderer, 0xFFFFFFFF); //TEMP
                        // if (player.CurrentWeapon.RemainingCooldown > 0)
                        // {
                        //     SDLTools.SetRenderDrawColor(renderer, 0xFFFF0000);
                        // }
                        // else
                        // {
                        //     SDLTools.SetRenderDrawColor(renderer, 0xFFFFFFFF);
                        // }
                        // for (int i = 0; i < player.CurrentWeapon.Ammo; i++)
                        // {
                        //     var rect2 = new SDL.SDL_Rect { x = 10 + i * 20, y = 10, w = 10, h = 10 };
                        //     SDLTools.Assert(SDL.SDL_RenderFillRect(renderer, ref rect2));
                        // }
                        int dirIndicatorScale = 10;
                        SDL.SDL_RenderDrawLine(renderer, (int)(player.Position.X), (int)(player.Position.Y), (int)(player.Position.X + player.Direction.X * dirIndicatorScale), (int)(player.Position.Y + player.Direction.Y * dirIndicatorScale));
                    }
                }
                catch
                {
                    Console.WriteLine("No player");
                    Environment.Exit(-1);
                }
            }

            SDL.SDL_RenderPresent(renderer);
#endif

            // Thread.Sleep((int)Math.Max(1_000.0 / 60.0 - DeltaTime, 0.0));
        }
    }

    public void RemoveAxeMan(AxeMan axeMan) => AxeMansToRemove.Add(axeMan);
    public void RemoveSlime(Slime slime) => SlimesToRemove.Add(slime);
    public void RemovePlayer(Player player) => PlayersToRemove.Add(player);
    public void RemoveLootBox(LootBox lootBox) => LootBoxsToRemove.Add(lootBox);
    public void RemoveShell(Shell shell) => ShellsToRemove.Add(shell);
    public void RemoveBolt(Bolt bolt) => BoltsToRemove.Add(bolt);
    public void RemoveArrow(Arrow arrow) => ArrowsToRemove.Add(arrow);

    public void SpawnAxeMan(AxeMan axeMan) => AxeMansToSpawn.Add(axeMan);
    public void SpawnSlime(Slime slime) => SlimesToSpawn.Add(slime);
    public void SpawnPlayer(Player player) => PlayersToSpawn.Add(player);
    public void SpawnLootBox(LootBox lootBox) => LootBoxsToSpawn.Add(lootBox);
    public void SpawnShell(Shell shell) => ShellsToSpawn.Add(shell);
    public void SpawnBolt(Bolt bolt) => BoltsToSpawn.Add(bolt);
    public void SpawnArrow(Arrow arrow) => ArrowsToSpawn.Add(arrow);

    // public static List<Item> GenerateLoot()
    // {
    //     Random rng = new Random(404);
    //     var loot = new List<Item>();
    //     if (rng.Next(10) > 5)
    //         loot.Add(new HealthPack());
    //     loot.Add(new ArrowBundle(rng.Next(1, 10)));
    //     loot.Add(new BoltBundle(rng.Next(1, 3)));
    //     loot.Add(new ShellBox(rng.Next(1, 3) * 5));
    //     return loot;
    // }
}


public abstract class Weapon
{
    public int Ammo;
    private float _remainingCooldown;
    abstract public int Range { get; }
    abstract public int Damage { get; }
    abstract public float Cooldown { get; }
    virtual public float RemainingCooldown { get => _remainingCooldown; set => _remainingCooldown = value; }
    public virtual void Attack(Game game, Player player)
    {
        RemainingCooldown = Cooldown;
    }
    public void Update(Game game)
    {
        RemainingCooldown -= game.DeltaTime;
    }
}

public struct KeyState
{
    public uint Up;
    public uint Down;
    public uint Left;
    public uint Right;
    public uint Close;
    public int Shoot;
}
public struct Rect(float x, float y, float width, float height)
{
    public float X = x;
    public float Y = y;
    public float Width = width;
    public float Height = height;

    public Vector2 Center { get => new(X, Y); }

    public bool Overlaps(Rect other)
    {
        return
               ((X >= other.X && X < other.X + other.Width)                     // leftmost point within other
            || (X + Width >= other.X && X + Width < other.X + other.Width))     // rightmost point within other
            && ((Y >= other.Y && Y < other.Y + other.Height)                    // bottommost point within other
            || (Y + Height >= other.Y && Y + Height < other.Y + other.Height)); // topmost point within other
    }
}
public struct Circle(float x, float y, float r)
{
    public float X = x;
    public float Y = y;
    public float R = r;

    public Vector2 Center { get => new Vector2(X, Y); set { X = value.X; Y = value.Y; } }
    public float Width { get => R + R; }
    public float Height { get => R + R; }

    public bool Overlaps(Circle other)
    {
        return (Center - other.Center).LengthSquared() < Math.Pow((R + other.R), 2);
    }

#if RENDERING
    public void Render(nint renderer, uint color)
    {
        SDLTools.Assert(SDLTools.SetRenderDrawColor(renderer, color));

        // For filled circle
        //! CREDIT: https://stackoverflow.com/a/65745687
        // for (int w = 0; w < R * 2; w++)
        // {
        //     for (int h = 0; h < R * 2; h++)
        //     {
        //         int dx = (int)(R - w); // horizontal offset
        //         int dy = (int)(R - h); // vertical offset
        //         if ((dx * dx + dy * dy) <= (R * R))
        //         {
        //             SDL.SDL_RenderDrawPoint(renderer, (int)(X + dx), (int)(Y + dy));
        //         }
        //     }
        // }

        // For non-filled circle (faster)
        //! CREDIT: https://discourse.libsdl.org/t/query-how-do-you-draw-a-circle-in-sdl2-sdl2/33379
        int diameter = (int)(R * 2);
        int x = (int)(R - 1);
        int y = 0;
        int tx = 1;
        int ty = 1;
        int error = (tx - diameter);


        while (x >= y)
        {
            // Each of the following renders an octant of the circle
            SDLTools.Assert(SDL.SDL_RenderDrawPoint(renderer, (int)(X + x), (int)(Y - y)));
            SDLTools.Assert(SDL.SDL_RenderDrawPoint(renderer, (int)(X + x), (int)(Y + y)));
            SDLTools.Assert(SDL.SDL_RenderDrawPoint(renderer, (int)(X - x), (int)(Y - y)));
            SDLTools.Assert(SDL.SDL_RenderDrawPoint(renderer, (int)(X - x), (int)(Y + y)));
            SDLTools.Assert(SDL.SDL_RenderDrawPoint(renderer, (int)(X + y), (int)(Y - x)));
            SDLTools.Assert(SDL.SDL_RenderDrawPoint(renderer, (int)(X + y), (int)(Y + x)));
            SDLTools.Assert(SDL.SDL_RenderDrawPoint(renderer, (int)(X - y), (int)(Y - x)));
            SDLTools.Assert(SDL.SDL_RenderDrawPoint(renderer, (int)(X - y), (int)(Y + x)));

            if (error <= 0)
            {
                ++y;
                error += ty;
                ty += 2;
            }

            if (error > 0)
            {
                --x;
                tx += 2;
                error += (tx - diameter);
            }
        }
    }
#endif
}
public static class SDLTools
{
    public static void Assert(nint err_code) => Trace.Assert(err_code == 0, $"SDL Error: {SDL.SDL_GetError()}");
    public static int SetRenderDrawColor(nint renderer, uint color) => SDL.SDL_SetRenderDrawColor(renderer, (byte)(color >> 16), (byte)(color >> 8), (byte)(color >> 0), (byte)(color >> 24));
    public static uint ColorFromRGB(uint red, uint blue, uint green) => ColorFromRGBA(red, green, blue, 255);
    public static uint ColorFromRGBA(uint red, uint blue, uint green, uint alpha) => (alpha << 24) + (red << 16) + (green << 8) + (blue);
}

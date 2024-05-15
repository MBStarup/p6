#define RENDERING
#define INPUT
// #define RECORDING
// #define REPLAYING
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
        string path = "";
        if (args.Length != 0)
        {
            path = args[0];
        }

        Game game = new(69420);

        // public static List<Item> GenerateLoot()
        // {
        //     Random rng = new Random(404);
        //     var loot = new List<Item>();

        //     return loot;
        // }

        // setup the "scene"
        Vector2 maxPos = new(Program.WINDOW_X, Program.WINDOW_Y);
        Random lootRNG = new Random(404);
        for (int i = 0; i < 100; i++)
        {
            var axeMan = new AxeMan
            {
                Position = new Vector2((float)game.Rand.NextDouble(), (float)game.Rand.NextDouble()) * maxPos
            };
            axeMan.Collider.R = 12.5f;
            axeMan.ID = (uint)i;
            axeMan.Color = 0xFFFF0000;

            int lootHealth = 0;
            if (lootRNG.Next(10) > 5)
                lootHealth = 250;
            int lootShell = lootRNG.Next(1, 3);
            int lootBolt = lootRNG.Next(1, 3);
            int lootArrow = lootRNG.Next(1, 3);

            axeMan.Loot = new LootBox { Color = 0xFFBF9000, Health = lootHealth, Shells = lootShell, Bolts = lootBolt, Arrows = lootArrow };

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


            int lootHealth = 0;
            if (lootRNG.Next(10) > 5)
                lootHealth = 250;
            int lootShell = lootRNG.Next(1, 3);
            int lootBolt = lootRNG.Next(1, 3);
            int lootArrow = lootRNG.Next(1, 3);

            slime.Loot = new LootBox { Color = 0xFFBF9000, Health = lootHealth, Shells = lootShell, Bolts = lootBolt, Arrows = lootArrow };

            game.Slimes.Add(slime);
        }
        Player player = new Player { Position = new Vector2(100, 100) };
        player.Collider.R = 12.5f;
        player.Color = 0xFFFF00FF;
        game.Players.Add(player);


        game.Run(path);
    }
}

public struct AxeMan()
{
    public uint ID;
    public override bool Equals(object obj)
    {
        if (!(obj is AxeMan axeMan)) return false;
        return ID.Equals(axeMan.ID);
    }
    public uint Color;
    public Circle Collider;
    public Vector2 Position { get => Collider.Center; set => Collider.Center = value; }
    public int Damage = 3;

    public Vector2 Velocity;
    public int MaxHP = 21;
    public int HP = 21;
    public LootBox Loot;

    public static void OnRemove(Game game, AxeMan axeMan)
    {
        axeMan.Loot.Collider = new Circle { X = axeMan.Position.X, Y = axeMan.Position.Y, R = 5f };
        game.SpawnLootBox(axeMan.Loot);
    }
    public static void Update(Game game, ref AxeMan axeMan)
    {

        float speed = 0.01f;
        {

            Player player = new();
            try
            {
                player = game.Players[0];
            }
            catch
            {
                Console.WriteLine("no player");
                Environment.Exit(-1);
            }
            Vector2 playerPos = player.Position;
            axeMan.Velocity = Vector2.Normalize(playerPos - axeMan.Position) * speed;

            axeMan.Color = SDLTools.ColorFromRGB(155 + (uint)(100 * (axeMan.HP / axeMan.MaxHP)), (uint)(100 * (axeMan.HP / axeMan.MaxHP)), (uint)(100 * (axeMan.HP / axeMan.MaxHP)));


            {
                axeMan.Position += axeMan.Velocity * game.DeltaTime;
                axeMan.Velocity *= MathF.Pow(0.996f, game.DeltaTime); // drag
            }
        }

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
        for (int i = 0; i < game.Players.Count; i++)
        {
            Player player = game.Players[i];
            if (axeMan.Collider.Overlaps(player.Collider))
            {
                while (axeMan.Collider.Overlaps(player.Collider))
                {
                    var direction = Vector2.Normalize(axeMan.Collider.Center - player.Collider.Center);
                    axeMan.Position += new Vector2(direction.X, direction.Y) * 0.1f;
                }
                if ((player.HP -= axeMan.Damage) <= 0)
                {
                    game.RemovePlayer(player);
                }

            }
            game.Players[i] = player;
        }
    }
}
public struct Slime()
{
    public uint ID;
    public override bool Equals(object obj)
    {
        if (!(obj is Slime slime)) return false;
        return ID.Equals(slime.ID);
    }
    public uint Color;
    public Circle Collider;
    public Vector2 Position { get => Collider.Center; set => Collider.Center = value; }
    public int Damage = 1;

    public LootBox Loot;

    public Vector2 Velocity;
    public int MaxHP = 5;
    public int HP = 5;

    public static void OnRemove(Game game, Slime slime)
    {
        slime.Loot.Collider = new Circle { X = slime.Position.X, Y = slime.Position.Y, R = 5f };
        game.SpawnLootBox(slime.Loot);
    }
    public static void Update(Game game, ref Slime slime)
    {
        foreach (AxeMan axeMan in game.AxeMans)
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
        foreach (Slime otherSlime in game.Slimes)
        {
            if (otherSlime.ID != slime.ID && otherSlime.Collider.Overlaps(slime.Collider))
            {
                while (slime.Collider.Overlaps(otherSlime.Collider))
                {
                    var direction = Vector2.Normalize(slime.Collider.Center - otherSlime.Collider.Center);
                    slime.Position += new Vector2(direction.X, direction.Y) * 0.1f;
                }
            }
        }
        for (int i = 0; i < game.Players.Count; i++)
        {
            Player player = game.Players[i];
            if (slime.Collider.Overlaps(player.Collider))
            {
                while (slime.Collider.Overlaps(player.Collider))
                {
                    var direction = Vector2.Normalize(slime.Collider.Center - player.Collider.Center);
                    slime.Position += new Vector2(direction.X, direction.Y) * 0.1f;
                }
                if ((player.HP -= slime.Damage) <= 0)
                {
                    game.RemovePlayer(player); //! Probably shoulndn't change player further after this, as I think structs might be compared by value when looking for which one to remove
                }
            }
            game.Players[i] = player;
        }
    }
}
public struct Player()
{
    public uint ID;
    public override bool Equals(object obj)
    {
        if (!(obj is Player player)) return false;
        return ID.Equals(player.ID);
    }
    public uint Color;
    public Circle Collider;
    public Vector2 Position { get => Collider.Center; set => Collider.Center = value; }
    public int CurrentWeapon = 0;
    public Vector2 Direction { get; set; }
    public List<Weapon> Weapons { get; private set; } = [new Bow(), new Crossbow(), new Shotgun()];
    public Vector2 Velocity;
    public int AttackCooldown;
    public int HP = MaxHP;
    public const int MaxHP = 1000;

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
            {
                if (player.AttackCooldown <= 0)
                {
                    player.Weapons[player.CurrentWeapon].Attack(game, player);
                }
            }
            else switch (game.KeyState.WeaponChoice)
                {
                    case 1: player.CurrentWeapon = 0; break;
                    case 2: player.CurrentWeapon = 1; break;
                    case 3: player.CurrentWeapon = 2; break;
                    default: break;
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
                while (player.Collider.Overlaps(axeMan.Collider))
                {
                    var direction = Vector2.Normalize(player.Collider.Center - axeMan.Collider.Center);
                    player.Position += new Vector2(direction.X, direction.Y) * 0.1f;
                }
            }
        }
        foreach (Slime slime in game.Slimes)
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
        foreach (Player otherPlayer in game.Players)
        {
            if (otherPlayer.ID != player.ID && otherPlayer.Collider.Overlaps(player.Collider))
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
public struct LootBox
{
    public uint ID;
    public override bool Equals(object obj)
    {
        if (!(obj is LootBox lootBox)) return false;
        return ID.Equals(lootBox.ID);
    }
    public uint Color;
    public Circle Collider;
    public Vector2 Position { get => Collider.Center; set => Collider.Center = value; }
    public Vector2 Velocity;
    public int Health;
    public int Shells;
    public int Bolts;
    public int Arrows;


    public static void OnRemove(Game game, LootBox lootBox) { }
    public static void Update(Game game, ref LootBox lootBox)
    {
        foreach (AxeMan axeMan in game.AxeMans)
        {
            if (lootBox.Collider.Overlaps(axeMan.Collider)) { game.RemoveLootBox(lootBox); }
        }
        foreach (Slime slime in game.Slimes)
        {
            if (lootBox.Collider.Overlaps(slime.Collider)) { game.RemoveLootBox(lootBox); }
        }
        for (int i = 0; i < game.Players.Count; i++)
        {
            Player player = game.Players[i];
            if (lootBox.Collider.Overlaps(player.Collider))
            {
                player.HP = Math.Max(player.HP + lootBox.Health, Player.MaxHP);
                foreach (Weapon weapon in player.Weapons)
                {
                    if (weapon is Shotgun s) s.Ammo += lootBox.Shells;
                    else if (weapon is Crossbow c) c.Ammo += lootBox.Bolts;
                    else if (weapon is Bow b) b.Ammo += lootBox.Arrows;
                }
                game.RemoveLootBox(lootBox);
            }
            game.Players[i] = player;
        }
    }
}
public struct Shell
{
    public uint ID;
    public override bool Equals(object obj)
    {
        if (!(obj is Shell shell)) return false;
        return ID.Equals(shell.ID);
    }
    public uint Color;
    public Circle Collider;
    public Vector2 Position { get => Collider.Center; set => Collider.Center = value; }
    public Vector2 Velocity;
    public Vector2 Origin;
    public Vector2 Direction;
    public int Damage;
    public float Speed;
    public float RangeSquared;

    public static void OnRemove(Game game, Shell shell) { }
    public static void Update(Game game, ref Shell shell)
    {
        {
            if ((shell.Position - shell.Origin).LengthSquared() > shell.RangeSquared) game.RemoveShell(shell);
            shell.Velocity = shell.Direction * shell.Speed;
            shell.Position = shell.Position + shell.Velocity * game.DeltaTime;
            shell.Velocity *= MathF.Pow(0.996f, game.DeltaTime); // drag
        }
        for (int i = 0; i < game.AxeMans.Count; i++)
        {
            AxeMan axeMan = game.AxeMans[i];
            if (shell.Collider.Overlaps(axeMan.Collider))
            {
                axeMan.HP -= shell.Damage;
                if (axeMan.HP <= 0) game.RemoveAxeMan(axeMan);
                game.RemoveShell(shell);
            }
            game.AxeMans[i] = axeMan; //dealing with value struct memes this seems slow or somethign idk :shrug:
        }
        for (int i = 0; i < game.Slimes.Count; i++)
        {
            Slime slime = game.Slimes[i];
            if (shell.Collider.Overlaps(slime.Collider))
            {
                slime.HP -= shell.Damage;
                if (slime.HP <= 0) game.RemoveSlime(slime);
                game.RemoveShell(shell);
            }
            game.Slimes[i] = slime; //dealing with value struct memes this seems slow or somethign idk :shrug:
        }
    }
}
public struct Bolt
{
    public uint ID;
    public override bool Equals(object obj)
    {
        if (!(obj is Bolt bolt)) return false;
        return ID.Equals(bolt.ID);
    }
    public uint Color;
    public Circle Collider;
    public Vector2 Position { get => Collider.Center; set => Collider.Center = value; }
    public Vector2 Velocity;
    public Vector2 Origin;
    public Vector2 Direction;
    public int Damage;
    public float Speed;
    public float RangeSquared;
    public int PenetrationPower;
    public static void OnRemove(Game game, Bolt bolt) { }
    public static void Update(Game game, ref Bolt bolt)
    {
        {
            if ((bolt.Position - bolt.Origin).LengthSquared() > bolt.RangeSquared) game.RemoveBolt(bolt);
            bolt.Velocity = bolt.Direction * bolt.Speed;
            bolt.Position = bolt.Position + bolt.Velocity * game.DeltaTime;
            bolt.Velocity *= MathF.Pow(0.996f, game.DeltaTime); // drag
        }
        for (int i = 0; i < game.AxeMans.Count; i++)
        {
            AxeMan axeMan = game.AxeMans[i];
            if (bolt.Collider.Overlaps(axeMan.Collider))
            {
                axeMan.HP -= bolt.Damage;
                if (axeMan.HP <= 0) game.RemoveAxeMan(axeMan);
                if (--bolt.PenetrationPower <= 0)
                    game.RemoveBolt(bolt);
            }
            game.AxeMans[i] = axeMan; //dealing with value struct memes this seems slow or somethign idk :shrug:
        }
        for (int i = 0; i < game.Slimes.Count; i++)
        {
            Slime slime = game.Slimes[i];
            if (bolt.Collider.Overlaps(slime.Collider))
            {
                slime.HP -= bolt.Damage;
                if (slime.HP <= 0) game.RemoveSlime(slime);
                if (--bolt.PenetrationPower <= 0)
                    game.RemoveBolt(bolt);
            }
            game.Slimes[i] = slime; //dealing with value struct memes this seems slow or somethign idk :shrug:
        }
    }
}
public struct Arrow
{
    public uint ID;
    public override bool Equals(object obj)
    {
        if (!(obj is Arrow arrow)) return false;
        return ID.Equals(arrow.ID);
    }
    public uint Color;
    public Circle Collider;
    public Vector2 Position { get => Collider.Center; set => Collider.Center = value; }
    public Vector2 Velocity;
    public Vector2 Origin;
    public Vector2 Direction;
    public int Damage;
    public float Speed;
    public float RangeSquared;
    public static void OnRemove(Game game, Arrow arrow) { }
    public static void Update(Game game, ref Arrow arrow)
    {
        {
            if ((arrow.Position - arrow.Origin).LengthSquared() > arrow.RangeSquared) game.RemoveArrow(arrow);
            arrow.Velocity = arrow.Direction * arrow.Speed;
            arrow.Position = arrow.Position + arrow.Velocity * game.DeltaTime;
            arrow.Velocity *= MathF.Pow(0.996f, game.DeltaTime); // drag
        }
        for (int i = 0; i < game.AxeMans.Count; i++)
        {
            AxeMan axeMan = game.AxeMans[i];
            if (arrow.Collider.Overlaps(axeMan.Collider))
            {
                axeMan.HP -= arrow.Damage;
                if (axeMan.HP <= 0) game.RemoveAxeMan(axeMan);
                game.RemoveArrow(arrow);
            }
            game.AxeMans[i] = axeMan; //dealing with value struct memes this seems slow or somethign idk :shrug:
        }
        for (int i = 0; i < game.Slimes.Count; i++)
        {
            Slime slime = game.Slimes[i];
            if (arrow.Collider.Overlaps(slime.Collider))
            {
                slime.HP -= arrow.Damage;
                if (slime.HP <= 0) game.RemoveSlime(slime);
                game.RemoveArrow(arrow);
            }
            game.Slimes[i] = slime; //dealing with value struct memes this seems slow or somethign idk :shrug:
        }
    }
}



public class Game(int seed)
{
    private uint id_counter = 0;
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
    public void Run(string ReplayPath = "")
    {
#if RECORDING
        record = new Recording();
#endif
#if REPLAYING
        replay = new Replay(ReplayPath);
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
                    if (e.key.keysym.sym == SDL.SDL_Keycode.SDLK_1 && KeyState.WeaponChoice == 0) KeyState.WeaponChoice = 1;
                    if (e.key.keysym.sym == SDL.SDL_Keycode.SDLK_2 && KeyState.WeaponChoice == 0) KeyState.WeaponChoice = 2;
                    if (e.key.keysym.sym == SDL.SDL_Keycode.SDLK_3 && KeyState.WeaponChoice == 0) KeyState.WeaponChoice = 3;
                }

                if (e.type == SDL.SDL_EventType.SDL_KEYUP)
                {
                    if (e.key.keysym.sym == SDL.SDL_Keycode.SDLK_d) KeyState.Right = 0;
                    if (e.key.keysym.sym == SDL.SDL_Keycode.SDLK_a) KeyState.Left = 0;
                    if (e.key.keysym.sym == SDL.SDL_Keycode.SDLK_w) KeyState.Up = 0;
                    if (e.key.keysym.sym == SDL.SDL_Keycode.SDLK_s) KeyState.Down = 0;
                    if (e.key.keysym.sym == SDL.SDL_Keycode.SDLK_ESCAPE) KeyState.Close = 0;
                    if (e.key.keysym.sym == SDL.SDL_Keycode.SDLK_e) KeyState.Shoot = 0;
                    if (e.key.keysym.sym == SDL.SDL_Keycode.SDLK_1) KeyState.WeaponChoice = 0;
                    if (e.key.keysym.sym == SDL.SDL_Keycode.SDLK_2) KeyState.WeaponChoice = 0;
                    if (e.key.keysym.sym == SDL.SDL_Keycode.SDLK_3) KeyState.WeaponChoice = 0;
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
#if RENDERING
            SDL.SDL_SetWindowTitle(window, $"FPS: {frameTimeBuffer.Count() / (frameTimeBuffer.Aggregate(0.0, (x, y) => x + y) / 1000)}");
#endif
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
                axeMan.Collider.Render(renderer, axeMan.Color);
            }
            foreach (Slime slime in Slimes)
            {
                slime.Collider.Render(renderer, slime.Color);
            }
            foreach (Player player in Players)
            {
                player.Collider.Render(renderer, player.Color);
            }
            foreach (LootBox lootBox in LootBoxs)
            {
                lootBox.Collider.Render(renderer, lootBox.Color);
            }
            foreach (Shell shell in Shells)
            {
                shell.Collider.Render(renderer, shell.Color);
            }
            foreach (Bolt bolt in Bolts)
            {
                bolt.Collider.Render(renderer, bolt.Color);
            }
            foreach (Arrow arrow in Arrows)
            {
                arrow.Collider.Render(renderer, arrow.Color);
            }

            { //scope (for name "player"), refactor out whenever
                try
                {

                    Player player = Players[0];
                    {
                        SDLTools.SetRenderDrawColor(renderer, 0xFFFFFFFF); //TEMP
                        if (player.Weapons[player.CurrentWeapon].RemainingCooldown > 0)
                        {
                            SDLTools.SetRenderDrawColor(renderer, 0xFFFF0000);
                        }
                        else
                        {
                            SDLTools.SetRenderDrawColor(renderer, 0xFFFFFFFF);
                        }
                        for (int i = 0; i < player.Weapons[player.CurrentWeapon].Ammo; i++)
                        {
                            var rect2 = new SDL.SDL_Rect { x = 10 + i * 20, y = 10, w = 10, h = 10 };
                            SDLTools.Assert(SDL.SDL_RenderFillRect(renderer, ref rect2));
                        }
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

    public void SpawnAxeMan(AxeMan axeMan)
    {
        axeMan.ID = ++id_counter;
        AxeMansToSpawn.Add(axeMan);
    }
    public void SpawnSlime(Slime slime)
    {
        slime.ID = ++id_counter;
        SlimesToSpawn.Add(slime);
    }
    public void SpawnPlayer(Player player)
    {
        player.ID = ++id_counter;
        PlayersToSpawn.Add(player);
    }
    public void SpawnLootBox(LootBox lootBox)
    {
        lootBox.ID = ++id_counter;
        LootBoxsToSpawn.Add(lootBox);
    }
    public void SpawnShell(Shell shell)
    {
        shell.ID = ++id_counter;
        ShellsToSpawn.Add(shell);
    }
    public void SpawnBolt(Bolt bolt)
    {
        bolt.ID = ++id_counter;
        BoltsToSpawn.Add(bolt);
    }
    public void SpawnArrow(Arrow arrow)
    {
        arrow.ID = ++id_counter;
        ArrowsToSpawn.Add(arrow);
    }
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

class Axe : Weapon
{
    public override int Range => 1;
    public override int Damage => 5;
    public override float Cooldown => 2;
}

class Bow : Weapon
{
    public Bow()
    {
        Ammo = 10;
    }
    public override int Range => 10; //intentionally left unused
    public override int Damage => 25;
    public override float Cooldown => 1000;
    public override void Attack(Game game, Player player)
    {
        if (Ammo > 0 && RemainingCooldown <= 0)
        {
            Ammo--;
            game.SpawnArrow(new Arrow { RangeSquared = 10000f, Origin = player.Position, Direction = player.Direction, Damage = Damage, Color = 0xFF6A329F, Collider = new Circle(player.Position.X + player.Direction.X * 15f, player.Position.Y + player.Direction.Y * 15f, 2.5f), Speed = 0.5f });
            base.Attack(game, player);
        }
    }
}

class Crossbow : Weapon
{
    public Crossbow()
    {
        Ammo = 5;
    }
    public override int Range => 15; // intentialonally left unused
    public override int Damage => 10;
    public override float Cooldown => 3000;
    public override void Attack(Game game, Player player)
    {
        if (Ammo > 0 && RemainingCooldown <= 0)
        {
            Ammo--;
            game.SpawnBolt(new Bolt { PenetrationPower = 3, RangeSquared = 10000f, Origin = player.Position, Direction = player.Direction, Damage = Damage, Color = 0xFFC90076, Collider = new Circle(player.Position.X + player.Direction.X * 15f, player.Position.Y + player.Direction.Y * 15f, 2.5f), Speed = 0.5f });
            base.Attack(game, player);
        }
    }
}

class Shotgun : Weapon
{
    public Shotgun()
    {
        Ammo = 25;
        angleBetweenShots = -(2 * startAngle / (numberShots - 1));
    }
    private Vector2 shotDirection;
    private Vector2 tempDirection;
    private float startAngle = 0.5f;
    private float angleBetweenShots;
    public int numberShots = 5;
    public override int Range => 5; // intentialonally left unused
    public override int Damage => 7;
    public override float Cooldown => 4000;
    public override void Attack(Game game, Player player)
    {
        if (Ammo >= numberShots && RemainingCooldown <= 0)
        {
            Ammo -= numberShots;
            shotDirection = player.Direction;
            /* Have to use temp variable, because assigning X would change the result of Y */
            tempDirection.X = MathF.Cos(startAngle) * shotDirection.X - MathF.Sin(startAngle) * shotDirection.Y;
            tempDirection.Y = MathF.Sin(startAngle) * shotDirection.X + MathF.Cos(startAngle) * shotDirection.Y;
            shotDirection = tempDirection;
            game.SpawnShell(new Shell { RangeSquared = 10000f, Origin = player.Position, Direction = shotDirection, Damage = Damage, Color = 0xFF6A329F, Collider = new Circle(player.Position.X + player.Direction.X * 15f, player.Position.Y + player.Direction.Y * 15f, 2.5f), Speed = 0.5f });
            for (int i = 1; i < numberShots; i++)
            {
                tempDirection.X = MathF.Cos(angleBetweenShots) * shotDirection.X - MathF.Sin(angleBetweenShots) * shotDirection.Y;
                tempDirection.Y = MathF.Sin(angleBetweenShots) * shotDirection.X + MathF.Cos(angleBetweenShots) * shotDirection.Y;
                shotDirection = tempDirection;
                game.SpawnShell(new Shell { RangeSquared = 10000f, Origin = player.Position, Direction = shotDirection, Damage = Damage, Color = 0xFF6A329F, Collider = new Circle(player.Position.X + player.Direction.X * 15f, player.Position.Y + player.Direction.Y * 15f, 2.5f), Speed = 0.5f });
            }
            base.Attack(game, player);
        }
    }
}

public struct KeyState
{
    public uint Up;
    public uint Down;
    public uint Left;
    public uint Right;
    public uint Close;
    public uint Shoot;
    public uint WeaponChoice;
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

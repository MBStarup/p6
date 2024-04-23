#define RENDERING
#define INPUT
#define RECORDING
// #define REPLAYING

using System.Diagnostics;
using System.Linq;
using System.Numerics;
using CircularBuffer;
using SDL2;
using Items;
using System.Collections;
using Weapons;
using System.Reflection.Metadata;


static class Program
{
    // honestly move these, just wanted them somewhere "global" for the random spawning to use them
    public const int WINDOW_X = 1200;
    public const int WINDOW_Y = 800;


    public static void Main(string[] args)
    {
        Game game = new(69420);

        // setup the "scene"
        Vector2 maxPos = new(Program.WINDOW_X, Program.WINDOW_Y);
        for (int i = 0; i < 100; i++)
        {
            var axeMan = new AxeMan(Game.GenerateLoot())
            {
                Position = new Vector2((float)game.Rand.NextDouble(), (float)game.Rand.NextDouble()) * maxPos
            };
            game.GameObjects.Add(axeMan);
        }

        for (int i = 0; i < 100; i++)
        {
            var slime = new Slime(Game.GenerateLoot())
            {
                Position = new Vector2((float)game.Rand.NextDouble(), (float)game.Rand.NextDouble()) * maxPos
            };
            game.GameObjects.Add(slime);
        }
        game.GameObjects.Add(new Player { Position = new Vector2(100, 100) });

        game.Run();
    }
}

class KeyState
{
    public uint Up;
    public uint Down;
    public uint Left;
    public uint Right;
    public uint Close;
    public int Shoot;
}
struct Rect(float x, float y, float width, float height)
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

struct Circle(float x, float y, float r)
{
    public float X = x;
    public float Y = y;
    public float R = r;

    public Vector2 Center { get => new Vector2(X, Y); }
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

static class SDLTools
{
    public static void Assert(nint err_code) => Trace.Assert(err_code == 0, $"SDL Error: {SDL.SDL_GetError()}");
    public static int SetRenderDrawColor(nint renderer, uint color) => SDL.SDL_SetRenderDrawColor(renderer, (byte)(color >> 16), (byte)(color >> 8), (byte)(color >> 0), (byte)(color >> 24));
    public static uint ColorFromRGB(uint red, uint blue, uint green) => ColorFromRGBA(red, green, blue, 255);
    public static uint ColorFromRGBA(uint red, uint blue, uint green, uint alpha) => (alpha << 24) + (red << 16) + (green << 8) + (blue);
}

class Game(int seed)
{
    public float DeltaTime;
    public Random Rand = new Random(seed);

    public List<GameObject> GameObjects = new();
    public List<GameObject> ToRemove = new();
    public List<GameObject> ToSpawn = new();
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
/*             if (KeyState.Right > 0) KeyState.Right += 1;
            if (KeyState.Left > 0) KeyState.Left += 1;
            if (KeyState.Up > 0) KeyState.Up += 1;
            if (KeyState.Down > 0) KeyState.Down += 1;
            if (KeyState.Close > 0) KeyState.Close += 1;
            if (KeyState.Shoot > 0) KeyState.Shoot += 1; */

            while (SDL.SDL_PollEvent(out SDL.SDL_Event e) != 0)
            {

                if (e.type == SDL.SDL_EventType.SDL_QUIT) KeyState.Close = 1;
                if (e.type == SDL.SDL_EventType.SDL_KEYDOWN)
                {
                    if (e.key.keysym.sym == SDL.SDL_Keycode.SDLK_d && KeyState.Right == 0) KeyState.Right = 1;
                    if (e.key.keysym.sym == SDL.SDL_Keycode.SDLK_a && KeyState.Left == 0) KeyState.Left = 1;
                    if (e.key.keysym.sym == SDL.SDL_Keycode.SDLK_w && KeyState.Up == 0) KeyState.Up = 1;
                    if (e.key.keysym.sym == SDL.SDL_Keycode.SDLK_s && KeyState.Down == 0) KeyState.Down = 1;
                    if (e.key.keysym.sym == SDL.SDL_Keycode.SDLK_ESCAPE && KeyState.Close == 0) KeyState.Close = 1;
                    if (e.key.keysym.sym == SDL.SDL_Keycode.SDLK_e && KeyState.Shoot == 0) KeyState.Shoot = 1;
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
            if (KeyState.Close >0)
            {
                record.saveRecording();
                Environment.Exit(0);
            }

#endif
            // SDL.SDL_SetWindowTitle(window, $"avg frametime: {frameTimeBuffer.Aggregate(0.0, (x, y) => x + y) / frameTimeBuffer.Count()}");
            SDL.SDL_SetWindowTitle(window, $"FPS: {frameTimeBuffer.Count() / (frameTimeBuffer.Aggregate(0.0, (x, y) => x + y) / 1000)}");
            //update
            foreach (var gameObject in GameObjects)
            {
                //update
                gameObject.Update(this);

                foreach (var other in GameObjects)
                {
                    if (gameObject != other && gameObject.Collider.Overlaps(other.Collider))
                    {
                        gameObject.OnCollision(this, other);
                    }
                }
            }

            //remove stuff
            foreach (var gameObject in ToRemove)
            {
                gameObject.OnRemoval(this);
            }
            GameObjects.RemoveAll(ToRemove.Contains);
            ToRemove.Clear();

            //spawn stuff
            GameObjects.AddRange(ToSpawn);
            ToSpawn.Clear();

            //render
#if RENDERING
            var bgRect = new SDL.SDL_Rect { x = 0, y = 0, w = windowSize.X, h = windowSize.Y };
            SDLTools.SetRenderDrawColor(renderer, 0xFF111111);
            SDLTools.Assert(SDL.SDL_RenderClear(renderer));
            foreach (var gameObject in GameObjects)
            {
                var color = gameObject.Color;

                gameObject.Collider.Render(renderer, color);
            }

            Player player = GameObjects.FirstOrDefault(x => x is Player, null) as Player;
            if (player == null) System.Console.WriteLine("no player");
            else
            {
                if (player.CurrentWeapon.RemainingCooldown > 0)
                {
                    SDLTools.SetRenderDrawColor(renderer, 0xFFFF0000);
                }
                else
                {
                    SDLTools.SetRenderDrawColor(renderer, 0xFFFFFFFF);
                }
                for (int i = 0; i < player.CurrentWeapon.Ammo; i++)
                {
                    var rect2 = new SDL.SDL_Rect { x = 10 + i * 20, y = 10, w = 10, h = 10 };
                    SDLTools.Assert(SDL.SDL_RenderFillRect(renderer, ref rect2));
                }
                int dirIndicatorScale = 10;
                SDL.SDL_RenderDrawLine(renderer, (int)(player.Position.X), (int)(player.Position.Y), (int)(player.Position.X + player.direction.X * dirIndicatorScale), (int)(player.Position.Y + player.direction.Y * dirIndicatorScale));
            }

            SDL.SDL_RenderPresent(renderer);
#endif

            // Thread.Sleep((int)Math.Max(1_000.0 / 60.0 - DeltaTime, 0.0));
        }
    }

    public void Remove(GameObject gameObject) => ToRemove.Add(gameObject);
    public void Spawn(GameObject gameObject) => ToSpawn.Add(gameObject);
    public static List<Item> GenerateLoot()
    {
        Random rng = new Random(404);
        var loot = new List<Item>();
        if (rng.Next(10) > 5)
            loot.Add(new HealthPack());
        loot.Add(new ArrowBundle(rng.Next(1, 10)));
        loot.Add(new BoltBundle(rng.Next(1, 3)));
        loot.Add(new ShellBox(rng.Next(1, 3) * 5));
        return loot;
    }
}

// interface IRenderable
// {
//     Vector2 Position { get; set; }
//     uint Color { get; set; }

//     // Textures and whatnot
// }

// interface IPhysicsable
// {
//     Vector2 Position { get; set; }
//     Vector2 Velocity { get; set; }

//     // collision boxes and whatnot
// }

abstract class GameObject /* : IPhysicsable, IRenderable Turns out to be cancer */
{
    public Vector2 Position;
    public Vector2 Velocity;

    public uint Color = 0xFFFF00FF;

    // public Rect<float> Collider { get => new(Position.X, Position.Y, 25, 25); }
    public virtual Circle Collider { get => new(Position.X, Position.Y, 12.5f); }

    public virtual void Update(Game game)
    {
        Position += Velocity * game.DeltaTime;

        Velocity *= MathF.Pow(0.996f, game.DeltaTime); // drag
    }

    public abstract void OnCollision(Game game, GameObject other);
    public virtual void OnRemoval(Game game) { }


}

class Player : GameObject
{
    public Player()
    {
        Weapons = [new Shotgun(), new Crossbow(),];
    }
    public Weapon? CurrentWeapon => Weapons[currentWeapon]; // TODO: might throw if no weapons
    private int currentWeapon = 0;
    private float attackCooldown = 0;
    public int MaxHP;
    public int HP;
    public List<Weapon> Weapons;
    public Vector2 direction = new(1, 0);
    public override void Update(Game game)
    {
        float speed = 0.0025f;

        if (game.KeyState.Right > 0) Velocity.X += speed * game.DeltaTime;
        if (game.KeyState.Left > 0) Velocity.X -= speed * game.DeltaTime;
        if (game.KeyState.Down > 0) Velocity.Y += speed * game.DeltaTime;
        if (game.KeyState.Up > 0) Velocity.Y -= speed * game.DeltaTime;

        if (Velocity.LengthSquared() > 0.0f) direction = Vector2.Normalize(Velocity);

        if (game.KeyState.Shoot == 1)
            if (attackCooldown <= 0)
            {
                Weapons[currentWeapon].Attack(game, this);
            }
        foreach (Weapon weapon in Weapons)
        {
            weapon.Update(game);
        }
        base.Update(game);
    }

    public override void OnCollision(Game game, GameObject other)
    {
        (other as LootBox)?.OnCollision(game, this);
        while (this.Collider.Overlaps(other.Collider))
        {
            var direction = Vector2.Normalize(Collider.Center - other.Collider.Center);
            Position += new Vector2(direction.X, direction.Y) * 0.1f;
        }
    }
}

abstract class Enemy : GameObject
{
    public Enemy(List<Item> loot)
    {
        Loot = loot;
        HP = MaxHP;
    }
    public virtual int MaxHP{ get; }
    public int HP;
    public List<Item> Loot;
    public override void OnRemoval(Game game)
    {
        game.Spawn(new LootBox(Loot, this));
    }

    public override void OnCollision(Game game, GameObject other)
    {
        if (other is LootBox)
        {
            game.Remove(other);
        }
        while (this.Collider.Overlaps(other.Collider))
        {
            var direction = Vector2.Normalize(Collider.Center - other.Collider.Center);
            Position += new Vector2(direction.X, direction.Y) * 0.1f;
        }
    }
    public void TakeDamageFrom(Game game, Projectile projectile)
    {
        HP -= projectile.Damage;
        System.Console.WriteLine(this.GetType().ToString() + " took " + projectile.Damage + " damage, and is now at " + HP + " HP!");
        if (HP <= 0)
        {
            System.Console.WriteLine("and then it died.");
            game.Remove(this);
        }
    }

}

class AxeMan : Enemy
{
    public AxeMan(List<Item> loot) : base(loot)
    {
        Color = 0xFFFF0000;
    }
    public override int MaxHP => 21;
    public override void Update(Game game)
    {
        // track player
        float speed = 0.01f;
        var playerPos = game.GameObjects.FirstOrDefault(x => x is Player, null)?.Position ?? new(0.0f, 0.0f);
        Velocity = Vector2.Normalize(playerPos - Position) * speed;

#if RENDERING
        Color = SDLTools.ColorFromRGB(155 + (uint)(100 * (HP / MaxHP)), (uint)(100 * (HP / MaxHP)), (uint)(100 * (HP / MaxHP)));
#endif
        base.Update(game);
    }
}

class Slime : Enemy
{
    public Slime(List<Item> loot) : base(loot)
    {
        Color = 0xFF00FF00;
    }
    public override int MaxHP => 5;
}
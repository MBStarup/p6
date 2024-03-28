#define RENDERING
#define INPUT

using System.Diagnostics;
using System.Linq;
using System.Numerics;
using CircularBuffer;
using SDL2;


static class Program
{
    // honestly move these, just wanted them somewhere "global" for the random spawning to use them
    public const int WINDOW_X = 1200;
    public const int WINDOW_Y = 800;


    public static void Main()
    {
        Game game = new(69420);

        // setup the "scene"
        Vec3D<double> maxPos = (Program.WINDOW_X, Program.WINDOW_Y, 0);
        for (int i = 0; i < 100; i++)
        {
            var axeMan = new AxeMan
            {
                Position = (game.Rand.NextDouble(), game.Rand.NextDouble(), game.Rand.NextDouble()) * maxPos
            };
            game.GameObjects.Add(axeMan);
        }

        for (int i = 0; i < 100; i++)
        {
            var slime = new Slime
            {
                Position = (game.Rand.NextDouble(), game.Rand.NextDouble(), game.Rand.NextDouble()) * maxPos
            };
            game.GameObjects.Add(slime);
        }
        game.GameObjects.Add(new Player { Position = (100, 100, 0) });

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
}

struct Vec2D<T>(T x, T y) where T : INumber<T>
{
    public T X = x;
    public T Y = y;

    public override readonly string ToString() => $"({X}, {Y})";

    public static implicit operator Vec2D<T>((T X, T Y) v) => new(v.X, v.Y);
    public static implicit operator Vec3D<T>(Vec2D<T> v) => (v.X, v.Y, T.Zero);
    public static Vec2D<T> operator -(Vec2D<T> v) => (-v.X, -v.Y);
    public static Vec2D<T> operator +(Vec2D<T> v1, Vec2D<T> v2) => (v1.X + v2.X, v1.Y + v2.Y);
    public static Vec2D<T> operator -(Vec2D<T> v1, Vec2D<T> v2) => (v1.X - v2.X, v1.Y - v2.Y);
    public static Vec2D<T> operator *(Vec2D<T> v1, Vec2D<T> v2) => (v1.X * v2.X, v1.Y * v2.Y);
    public static Vec2D<T> operator /(Vec2D<T> v1, Vec2D<T> v2) => (v1.X / v2.X, v1.Y / v2.Y);
    public static Vec2D<T> operator *(Vec2D<T> v, T x) => (v.X * x, v.Y * x);
    public static Vec2D<T> operator /(Vec2D<T> v, T x) => (v.X / x, v.Y / x);

    public readonly Vec2D<T> Norm() => this / Abs();
    public readonly T Abs() => Math.Sqrt((dynamic)Abs2()); // cast to double, couldn't find an interface for that specifically
    public readonly T Abs2() => Math.Pow((dynamic)X, 2) + Math.Pow((dynamic)Y, 2); // cast to double, couldn't find an interface for that specifically
}

struct Vec3D<T>(T x, T y, T z) where T : INumber<T>
{
    public T X = x;
    public T Y = y;
    public T Z = z;

    public override readonly string ToString() => $"({X}, {Y}, {Z})";

    public static implicit operator Vec3D<T>((T X, T Y, T Z) v) => new(v.X, v.Y, v.Z);
    public static implicit operator Vec2D<T>(Vec3D<T> v) => new(v.X, v.Y);
    public static Vec3D<T> operator -(Vec3D<T> v) => (-v.X, -v.Y, -v.Z);
    public static Vec3D<T> operator +(Vec3D<T> v1, Vec3D<T> v2) => (v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z);
    public static Vec3D<T> operator -(Vec3D<T> v1, Vec3D<T> v2) => (v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z);
    public static Vec3D<T> operator *(Vec3D<T> v1, Vec3D<T> v2) => (v1.X * v2.X, v1.Y * v2.Y, v1.Z * v2.Z);
    public static Vec3D<T> operator /(Vec3D<T> v1, Vec3D<T> v2) => (v1.X / v2.X, v1.Y / v2.Y, v1.Z / v2.Z);
    public static Vec3D<T> operator *(Vec3D<T> v, T x) => (v.X * x, v.Y * x, v.Z * x);
    public static Vec3D<T> operator /(Vec3D<T> v, T x) => (v.X / x, v.Y / x, v.Z / x);

    public readonly Vec3D<T> Norm() => this / Abs();
    public readonly T Abs() => Math.Sqrt((dynamic)Abs2()); // cast to double, couldn't find an interface for that specifically
    public readonly T Abs2() => Math.Pow((dynamic)X, 2) + Math.Pow((dynamic)Y, 2) + Math.Pow((dynamic)Z, 2); // cast to double, couldn't find an interface for that specifically
}

class Game(int seed)
{
    public double DeltaTime;
    public Random Rand = new Random(seed);

    public List<GameObject> GameObjects = new();
    (int X, int Y) windowSize = (Program.WINDOW_X, Program.WINDOW_Y);

    public KeyState KeyState = new();

    public void Run()
    {
#if RENDERING
        static void SDL_Assert(nint err_code) { Trace.Assert(err_code == 0, $"SDL Error: {SDL.SDL_GetError()}"); }

        SDL_Assert(SDL.SDL_Init(SDL.SDL_INIT_VIDEO));

        var window = SDL.SDL_CreateWindow(
                "hello_sdl2",
                SDL.SDL_WINDOWPOS_UNDEFINED, SDL.SDL_WINDOWPOS_UNDEFINED,
                windowSize.X, windowSize.Y,
                SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN
                );
        Trace.Assert(window != 0, "Failed to create window");


        var screenSurface = SDL.SDL_GetWindowSurface(window);
#endif

        var watch = Stopwatch.StartNew();
        var frameTimeBuffer = new CircularBuffer<double>(100);
        while (true)
        {
            // the code that you want to measure comes here
#if INPUT
            while (SDL.SDL_PollEvent(out SDL.SDL_Event e) != 0)
            {
                if (e.type == SDL.SDL_EventType.SDL_KEYDOWN)
                {
                    if (e.key.keysym.sym == SDL.SDL_Keycode.SDLK_d) KeyState.Right += 1;
                    if (e.key.keysym.sym == SDL.SDL_Keycode.SDLK_a) KeyState.Left += 1;
                    if (e.key.keysym.sym == SDL.SDL_Keycode.SDLK_w) KeyState.Up += 1;
                    if (e.key.keysym.sym == SDL.SDL_Keycode.SDLK_s) KeyState.Down += 1;
                    if (e.key.keysym.sym == SDL.SDL_Keycode.SDLK_ESCAPE) KeyState.Close += 1;
                }

                if (e.type == SDL.SDL_EventType.SDL_KEYUP)
                {
                    if (e.key.keysym.sym == SDL.SDL_Keycode.SDLK_d) KeyState.Right = 0;
                    if (e.key.keysym.sym == SDL.SDL_Keycode.SDLK_a) KeyState.Left = 0;
                    if (e.key.keysym.sym == SDL.SDL_Keycode.SDLK_w) KeyState.Up = 0;
                    if (e.key.keysym.sym == SDL.SDL_Keycode.SDLK_s) KeyState.Down = 0;
                    if (e.key.keysym.sym == SDL.SDL_Keycode.SDLK_ESCAPE) KeyState.Close = 0;
                }

            }

            if (KeyState.Close > 0) Environment.Exit(0);
#endif

            // watch.Stop();
            DeltaTime = Math.Clamp(((double)watch.ElapsedTicks / (double)Stopwatch.Frequency) * 1000, double.MinValue, 1000 / 60);
            watch.Restart();
            if (DeltaTime < 1.0) System.Console.WriteLine($"Small frame {DeltaTime} ms");
            frameTimeBuffer.PushBack(DeltaTime);
            SDL.SDL_SetWindowTitle(window, $"avg frametime: {frameTimeBuffer.Aggregate(0.0, (x, y) => x + y) / frameTimeBuffer.Count()}");
            // System.Console.WriteLine($"FPS: {frameTimeBuffer.Count() / (frameTimeBuffer.Aggregate(0.0, (x, y) => x + y) / 1000)}");
            //update 
            foreach (var gameObject in GameObjects)
            {
                gameObject.Update(this);
            }

            //render
#if RENDERING
            var bgRect = new SDL.SDL_Rect { x = 0, y = 0, w = windowSize.X, h = windowSize.Y };
            SDL_Assert(SDL.SDL_FillRect(screenSurface, ref bgRect, 0x111111));
            foreach (var gameObject in GameObjects)
            {
                Vec2D<double> size = (15, 15);
                size += size * (1 + gameObject.Position.Z) / 15;
                var rect = new SDL.SDL_Rect { x = (int)gameObject.Position.X, y = (int)gameObject.Position.Y, w = (int)size.X, h = (int)size.Y };
                SDL_Assert(SDL.SDL_FillRect(screenSurface, ref rect, gameObject.Color));
            }

            var rect2 = new SDL.SDL_Rect { x = 10, y = 10, w = 10, h = 10 };
            SDL_Assert(SDL.SDL_FillRect(screenSurface, ref rect2, 0xFFFFFF));

            SDL_Assert(SDL.SDL_UpdateWindowSurface(window));
#endif

            // Thread.Sleep((int)Math.Max(1_000.0 / 60.0 - DeltaTime, 0.0));
        }
    }
}

// interface IRenderable
// {
//     Vec3D<double> Position { get; set; }
//     uint Color { get; set; }

//     // Textures and whatnot
// }

// interface IPhysicsable
// {
//     Vec3D<double> Position { get; set; }
//     Vec3D<double> Velocity { get; set; }

//     // collision boxes and whatnot
// }

abstract class GameObject /* : IPhysicsable, IRenderable Turns out to be cancer */
{
    public Vec3D<double> Position;
    public Vec3D<double> Velocity;

    public uint Color = 0xFF00FF;
    public virtual void Update(Game game)
    {
        Position += Velocity * game.DeltaTime;

        Velocity *= Math.Pow(0.996, game.DeltaTime); // drag

        if (Position.Z > 0) Velocity.Z += -0.00009 * game.DeltaTime; // gravity
        else Position.Z = Velocity.Z = 0; // hitting the ground
    }
}

class Player : GameObject
{
    public override void Update(Game game)
    {
        var speed = 0.005;

        if (game.KeyState.Right > 0) Velocity.X += speed;
        if (game.KeyState.Left > 0) Velocity.X -= speed;
        if (game.KeyState.Down > 0) Velocity.Y += speed;
        if (game.KeyState.Up > 0) Velocity.Y -= speed;

        base.Update(game);
    }
}

abstract class NPC : GameObject
{
    public int HP;
}

abstract class Enemy : NPC
{
    public List<LootItem> Loot;
}

class AxeMan : Enemy
{
    public AxeMan()
    {
        Color = 0xFF0000;
    }
    public override void Update(Game game)
    {
        // track player
        double speed = 0.01;
        var playerPos = game.GameObjects.FirstOrDefault(x => x is Player, null)?.Position ?? (0.0, 0.0, 0.0);
        Velocity = (playerPos - Position).Norm() * speed;

        base.Update(game);
    }
}

class Slime : Enemy
{
    public double Jump = 0.1;

    public Slime()
    {
        Color = 0x00FF00;
    }
    public override void Update(Game game)
    {
        if (Position.Z == 0 && game.Rand.Next() % 100 < 1) Velocity.Z += Jump;

        base.Update(game);
    }
}

class FriendlyNPC : NPC
{
    public List<String> Dialogue;

}


class QuestGiver : FriendlyNPC
{
    public List<Quest> Quests;
}

public class Quest
{
}

public class LootItem
{
}
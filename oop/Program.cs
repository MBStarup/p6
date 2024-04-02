#define RENDERING
#define INPUT

using System.Diagnostics;
using System.Linq;
using System.Numerics;
using CircularBuffer;
using SDL2;
using Items;


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
    public readonly T Abs() => (dynamic)Math.Sqrt((dynamic)Abs2()); // cast to double, couldn't find an interface for that specifically
    public readonly T Abs2() => (dynamic)Math.Pow((dynamic)X, 2) + Math.Pow((dynamic)Y, 2); // cast to double, couldn't find an interface for that specifically
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
    public readonly T Abs() => (dynamic)Math.Sqrt((dynamic)Abs2()); // cast to double, couldn't find an interface for that specifically
    public readonly T Abs2() => (dynamic)Math.Pow((dynamic)X, 2) + Math.Pow((dynamic)Y, 2) + Math.Pow((dynamic)Z, 2); // cast to double, couldn't find an interface for that specifically
}

struct Rect<T>(T x, T y, T width, T height) where T : INumber<T>
{
    public T X = x;
    public T Y = y;
    public T Width = width;
    public T Height = height;

    public Vec2D<T> Center { get => (X, Y); }

    public bool Overlaps(Rect<T> other)
    {
        return
               ((X >= other.X && X < other.X + other.Width)                     // leftmost point within other
            || (X + Width >= other.X && X + Width < other.X + other.Width))     // rightmost point within other
            && ((Y >= other.Y && Y < other.Y + other.Height)                    // bottommost point within other
            || (Y + Height >= other.Y && Y + Height < other.Y + other.Height)); // topmost point within other
    }
}

struct Circle<T>(T x, T y, T r) where T : INumber<T>
{
    public T X = x;
    public T Y = y;
    public T R = r;

    public Vec2D<T> Center { get => (X, Y); }
    public T Width { get => R + R; }
    public T Height { get => R + R; }

    public bool Overlaps(Circle<T> other)
    {
        return (dynamic)(Center - other.Center).Abs2() < Math.Pow((dynamic)(R + other.R), 2); //? Idk maybe making these have a generic T was a bit much LULE we should probably just use the System.Numerics.Vector2, thoughts?
    }

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
            double realDeltaTime = ((double)watch.ElapsedTicks / (double)Stopwatch.Frequency) * 1000;
            watch.Restart();
            DeltaTime = Math.Clamp(realDeltaTime, double.MinValue, 1000 / 60);
            if (DeltaTime < 1.0) Console.WriteLine($"Small frame {DeltaTime} ms");
            frameTimeBuffer.PushBack(realDeltaTime);
            // SDL.SDL_SetWindowTitle(window, $"avg frametime: {frameTimeBuffer.Aggregate(0.0, (x, y) => x + y) / frameTimeBuffer.Count()}");
            SDL.SDL_SetWindowTitle(window, $"FPS: {frameTimeBuffer.Count() / (frameTimeBuffer.Aggregate(0.0, (x, y) => x + y) / 1000)}");
            //update 
            foreach (var gameObject in GameObjects)
            {
                gameObject.Update(this);
                Projectile? projectile = gameObject as Projectile;
                if (projectile != null)
                {
                    projectile.CheckProjectileCollision(GameObjects);
                }
            }


            //render
#if RENDERING
            var bgRect = new SDL.SDL_Rect { x = 0, y = 0, w = windowSize.X, h = windowSize.Y };
            SDL_Assert(SDL.SDL_FillRect(screenSurface, ref bgRect, 0x111111));
            foreach (var gameObject in GameObjects)
            {
                var color = gameObject.Color;
                // Vec2D<double> size = (15, 15);
                // size += size * (1 + gameObject.Position.Z) / 15;
                // var rect = new SDL.SDL_Rect { x = (int)gameObject.Position.X, y = (int)gameObject.Position.Y, w = (int)size.X, h = (int)size.Y };
                var rect = new SDL.SDL_Rect { x = (int)gameObject.Collider.X, y = (int)gameObject.Collider.Y, w = (int)gameObject.Collider.Width, h = (int)gameObject.Collider.Height };
                foreach (var other in GameObjects)
                {
                    while (gameObject != other && gameObject.Collider.Overlaps(other.Collider))
                    {
                        var direction = (gameObject.Collider.Center - other.Collider.Center).Norm();
                        gameObject.Position += new Vec3D<double>(direction.X, direction.Y, 0) * 0.1f;
                    }
                }
                SDL_Assert(SDL.SDL_FillRect(screenSurface, ref rect, color));
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

    // public Rect<double> Collider { get => new(Position.X, Position.Y, 25, 25); }
    public Circle<double> Collider { get => new(Position.X, Position.Y, 25); }

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
    public int HP;
    public List<Weapon> weapons;
    public override void Update(Game game)
    {
        var speed = 0.005;

        if (game.KeyState.Right > 0) Velocity.X += speed * game.DeltaTime;
        if (game.KeyState.Left > 0) Velocity.X -= speed * game.DeltaTime;
        if (game.KeyState.Down > 0) Velocity.Y += speed * game.DeltaTime;
        if (game.KeyState.Up > 0) Velocity.Y -= speed * game.DeltaTime;

        base.Update(game);
    }
}

abstract class NPC : GameObject
{
    public int HP;
}

abstract class Enemy : NPC
{
    public List<Item> Loot;
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

class Projectile : GameObject
{
    public Vec2D<double> origin;
    public double range;
    public void CheckProjectileCollision(List<GameObject> gameObjects)
    {
        foreach (var other in gameObjects)
        {
            if (this != other && this.Collider.Overlaps(other.Collider))
            {

                gameObjects.Remove(this);
            }
        }
        // Implement distance check from origin to check if projectile has travelled max distance
    }
}

class Arrow : Projectile
{

}
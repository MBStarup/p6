using System.Linq;

class Program
{
    public static void Main()
    {
        Game game = new(69420);

        // setup the "scene"
        Vec3D maxPos = (10.0, 10.0, 1.0);
        for (int i = 0; i < 100; i++)
        {
            var slime = new Slime
            {
                Position = (game.Rand.NextDouble() % maxPos.X, game.Rand.NextDouble() % maxPos.Y, game.Rand.NextDouble() % maxPos.Z)
            };
            game.GameObjects.Add(slime);
        }

        game.Run();
    }
}

struct Vec3D(double x, double y, double z)
{
    public double X = x;
    public double Y = y;
    public double Z = z;

    public static implicit operator Vec3D((double X, double Y, double Z) v) => new(v.X, v.Y, v.Z);
    public static Vec3D operator -(Vec3D v) => new(-v.X, -v.Y, -v.Z);
    public static Vec3D operator +(Vec3D v1, Vec3D v2) => new(v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z);
    public static Vec3D operator -(Vec3D v1, Vec3D v2) => new(v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z);
    public static Vec3D operator *(Vec3D v1, Vec3D v2) => new(v1.X * v2.X, v1.Y * v2.Y, v1.Z * v2.Z);
    public static Vec3D operator /(Vec3D v1, Vec3D v2) => new(v1.X / v2.X, v1.Y / v2.Y, v1.Z / v2.Z);
    public static Vec3D operator *(Vec3D v, double x) => new(v.X * x, v.Y * x, v.Z * x);
    public static Vec3D operator /(Vec3D v, double x) => new(v.X / x, v.Y / x, v.Z / x);

}

class Game(int seed)
{
    public double DeltaTime;
    public Random Rand = new Random(seed);

    public List<GameObject> GameObjects = new();
    public void Run()
    {
        var watch = System.Diagnostics.Stopwatch.StartNew();
        while (true)
        {
            // the code that you want to measure comes here
            //inputs
            // no-op

            watch.Stop();
            DeltaTime = watch.ElapsedMilliseconds;
            watch.Restart();
            //update 
            foreach (var gameObject in GameObjects)
            {
                gameObject.Update(this);
            }

            //render
            // no-op
        }
    }
}

abstract class GameObject
{
    public Vec3D Position;
    public Vec3D Velocity;
    public virtual void Update(Game game)
    {
        Position += Velocity * game.DeltaTime;

        Velocity.X *= 0.1 * game.DeltaTime; // drag
        Velocity.Y *= 0.1 * game.DeltaTime; // drag
        Velocity.Z *= 0.1 * game.DeltaTime; // drag

        if (Position.Z > 0)
            Velocity.Z += -0.9 * game.DeltaTime; // gravity
    }
}

class Player : GameObject
{
    // TODO
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
    public override void Update(Game game)
    {
        // track player
        var playerPos = game.GameObjects.FirstOrDefault(x => x is Player, null)?.Position ?? (0.0, 0.0, 0.0);



        base.Update(game);
    }
}

class Slime : Enemy
{
    public double Jump;

    public override void Update(Game game)
    {
        //jump
        if (Position.Z == 0)
            Velocity.Z += Jump;

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
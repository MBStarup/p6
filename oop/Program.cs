using System.Security;

class Program
{
    public static void Main()
    {
        GameObject ob = new();
        System.Console.WriteLine(ob.Position.X);
    }
}

abstract class GameObject
{
    public (float X, float Y, float Z) Position;
    public (float X, float Y, float Z) Velocity;
    public virtual void Update(float DeltaTime)
    {
        Position.X += Velocity.X * DeltaTime;
        Position.Y += Velocity.Y * DeltaTime;
        Position.Z += Velocity.Z * DeltaTime;

        Velocity.X *= 0.1f * DeltaTime; // drag
        Velocity.Y *= 0.1f * DeltaTime; // drag
        Velocity.Z *= 0.1f * DeltaTime; // drag

        if (Position.Z > 0)
            Velocity.Z += -0.9f * DeltaTime; // gravity
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
    public override void Update(float DeltaTime)
    {
        var rand = new Random();
        Velocity.X += ((rand.Next() % 10) - 5) * DeltaTime;
        Velocity.Y += ((rand.Next() % 10) - 5) * DeltaTime;

        base.Update(DeltaTime);
    }
}

class Slime : Enemy
{
    public float Jump;

    public override void Update(float DeltaTime)
    {
        if (Position.Z == 0)
            Velocity.Z += Jump;

        base.Update(DeltaTime);
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
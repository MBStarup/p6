using System.Numerics;
using Items;

namespace Weapons;

abstract class Weapon
{
    private float _remainingCooldown;
    abstract public int Range { get; }
    abstract public int Damage { get; }
    abstract public float Cooldown { get; }
    virtual public float RemainingCooldown{ get => _remainingCooldown; set => _remainingCooldown = value ;}
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
        arrows = 10;
    }
    public int arrows;
    public override int Range => 10;
    public override int Damage => 3;
    public override float Cooldown => 1000;
    public override void Attack(Game game, Player player)
    {
        if (arrows > 0 && RemainingCooldown<=0)
        {
            arrows--;
            game.Spawn(new Arrow(player.Position, player.direction, Damage));
            base.Attack(game, player);
        }
    }
}

class Crossbow : Weapon
{
    public Crossbow()
    {
        bolts =  5;
    }
    public int bolts;
    public override int Range => 15;
    public override int Damage => 10;
    public override float Cooldown => 3000;
    public override void Attack(Game game, Player player)
    {
        if (bolts > 0 && RemainingCooldown<=0)
        {
            bolts--;
            game.Spawn(new Bolt(player.Position, player.direction, Damage));
            base.Attack(game, player);
        }
    }
}

class Shotgun : Weapon
{
    public Shotgun()
    {
        shells = 25;
        angleBetweenShots = -(2*startAngle / (numberShots-1));
    }
    private Vector2 shotDirection;
    private Vector2 tempDirection;
    private float startAngle = 0.5f;
    private float angleBetweenShots;
    public int shells;
    public int numberShots = 5;
    public override int Range => 5;
    public override int Damage => 7;
    public override float Cooldown => 4000;
    public override void Attack(Game game, Player player)
    {
        if(shells >= numberShots && RemainingCooldown<=0)
        {
            shells-=numberShots;
            shotDirection = player.direction;
            /* Have to use temp variable, because assigning X would change the result of Y */
            tempDirection.X = MathF.Cos(startAngle)*shotDirection.X-MathF.Sin(startAngle)*shotDirection.Y;
            tempDirection.Y = MathF.Sin(startAngle)*shotDirection.X+MathF.Cos(startAngle)*shotDirection.Y;
            shotDirection = tempDirection;
            game.Spawn(new Shell(player.Position, shotDirection, Damage));
            for(int i=1;i<numberShots;i++)
            {
                tempDirection.X = MathF.Cos(angleBetweenShots)*shotDirection.X-MathF.Sin(angleBetweenShots)*shotDirection.Y;
                tempDirection.Y = MathF.Sin(angleBetweenShots)*shotDirection.X+MathF.Cos(angleBetweenShots)*shotDirection.Y;
                shotDirection = tempDirection;
                game.Spawn(new Shell(player.Position, shotDirection, Damage));
                Console.WriteLine(shotDirection);
            }
            base.Attack(game, player);
        }
    }
}

class LootBox : GameObject
{
    public LootBox(List<Item> loot, GameObject spawner)
    {
        Color = 0xFFbf9000;
        contents = [.. loot];
        Position = spawner.Position;
    }
    List<Item> contents;
    public override Circle Collider { get => new(Position.X, Position.Y, 5f); } 
    public override void OnCollision(Game game, GameObject other)
    {
        if(other is Player)
        {
            foreach(var item in contents)
            {
                item.OnPickup(game, other as Player);
            }
        }
        if(!(other is Projectile))
        {
            game.Remove(this);
        }
    }
}
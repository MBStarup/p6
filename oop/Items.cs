using System.Numerics;

namespace Items;

abstract class Weapon
{
    abstract public int Range { get; }
    abstract public int Damage { get; }
    public virtual void Attack(Player player, Game game){}
}

class Axe : Weapon
{
    public override int Range => 1;

    public override int Damage => 5;

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
    public override void Attack(Player player, Game game)
    {
        if (arrows > 0)
        {
            arrows--;
            game.Spawn(new Arrow(player.Position, player.direction, Damage));
        }
    }
}

class Crossbow : Weapon
{
    public int bolts;
    public override int Range => 15;
    public override int Damage => 10;
    public override void Attack(Player player, Game game)
    {
        if (bolts > 0)
        {
        }
    }
}

abstract class GroundItem : GameObject
{
    public override void OnCollision(Game game, GameObject other)
    {
        if(other is Player)
        {
            OnPickup(other as Player, game);
        }
        game.Remove(this);
    }
    public abstract void OnPickup(Player player, Game game);
}

class HealthPack : GroundItem
{
    public override void OnPickup(Player player, Game game)
    {
        player.HP += 25;
    }
}

class ArrowBundle : GroundItem
{
    public ArrowBundle(Vector2 location)
    {
        Color = 0xFFFFA500;
        Position = location;
    }
    public override Circle Collider { get => new(Position.X, Position.Y, 5f); } 
    public override void OnPickup(Player player, Game game)
    {
        Bow bow = player.Weapons.Find(weapon => weapon is Bow) as Bow;
        if(bow != null)
        {
            bow.arrows += 5;
        }
        game.Remove(this);
    }
}
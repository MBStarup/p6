using System.Numerics;

namespace Items;

abstract class Item
{
    public abstract void OnPickup(Player player);
}


abstract class Weapon : Item
{
    abstract public int Range { get; }
    abstract public int Damage { get; }
    public virtual void Attack(Player player, Game game){}
    public override void OnPickup(Player player)
    {
        if (player.Weapons.All(weapon => weapon.GetType() != GetType()))
        {
            player.Weapons.Add(this);
        }
    }
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
            game.Spawn(new Arrow(player.Position, player.direction));
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

class HealthPack : Item
{
    public override void OnPickup(Player player)
    {
        player.HP += 25;
    }
}
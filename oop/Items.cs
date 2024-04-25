using Weapons;
namespace Items;

abstract class Item
{
    abstract public void OnPickup(Game game, Player player);
}

class HealthPack : Item
{
    public override void OnPickup(Game game, Player player)
    {
        player.HP = Math.Min(player.HP+250,Player.MaxHP);
    }
}

abstract class AmmoBundle : Item
{
    public AmmoBundle(int amount)
    {
        Amount = amount;
    }
    public int Amount { get; set; }
}

class ArrowBundle : AmmoBundle
{
    public ArrowBundle(int amount) : base(amount)
    {

    }
    public override void OnPickup(Game game, Player player)
    {
        Bow bow = player.Weapons.Find(weapon => weapon is Bow) as Bow;
        if (bow != null)
        {
            bow.Ammo += Amount;
        }
    }
}

class BoltBundle : AmmoBundle
{
    public BoltBundle(int amount) : base(amount)
    {

    }
    public override void OnPickup(Game game, Player player)
    {
        Crossbow crossbow = player.Weapons.Find(weapon => weapon is Crossbow) as Crossbow;
        if (crossbow != null)
        {
            crossbow.Ammo += Amount;
        }
    }
}

class ShellBox : AmmoBundle
{
    public ShellBox(int amount) : base(amount)
    {

    }
    public override void OnPickup(Game game, Player player)
    {
        Shotgun shotgun = player.Weapons.Find(weapon => weapon is Shotgun) as Shotgun;
        if (shotgun != null)
        {
            shotgun.Ammo += Amount;
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
        if (other is Player)
        {
            foreach (var item in contents)
            {
                item.OnPickup(game, other as Player);
            }
        }
        if (!(other is Projectile))
        {
            game.Remove(this);
        }
    }
}
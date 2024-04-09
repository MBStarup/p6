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
        player.HP += 25;
    }
}

abstract class AmmoBundle : Item
{
    public AmmoBundle(int amount)
    {
        Amount = amount;
    }
    public int Amount {get; set;}
}

class ArrowBundle : AmmoBundle
{
    public ArrowBundle(int amount) : base(amount)
    {

    }
    public override void OnPickup(Game game, Player player)
    {
        Bow bow = player.Weapons.Find(weapon => weapon is Bow) as Bow;
        if(bow != null)
        {
            bow.arrows += Amount;
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
        if(crossbow != null)
        {
            crossbow.bolts += Amount;
        }
    }
}
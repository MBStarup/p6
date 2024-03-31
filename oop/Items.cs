namespace Items;

abstract class Item
{

}


abstract class Weapon : Item
{
    
    abstract public int Range { get;}

    abstract public int Damage { get;}

    public void Attack(NPC npc){
        npc.HP -= Damage;
    }
}

class Axe : Weapon
{
    public override int Range => 1;

    public override int Damage => 5;
}

class Bow : Weapon
{
    public int arrows;
    public override int Range => 10;

    public override int Damage => 3;
}

class Crossbow : Weapon
{
    public override int Range => 15;

    public override int Damage => 10;
}
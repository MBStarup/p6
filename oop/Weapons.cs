using System.Numerics;
using Items;

namespace Weapons;

abstract class Weapon
{
    public int Ammo;
    private float _remainingCooldown;
    abstract public int Range { get; }
    abstract public int Damage { get; }
    abstract public float Cooldown { get; }
    virtual public float RemainingCooldown { get => _remainingCooldown; set => _remainingCooldown = value; }
    public virtual void Attack(Game game, Player player)
    {
        RemainingCooldown = Cooldown;
    }
    public void Update(Game game)
    {
        RemainingCooldown -= game.DeltaTime;
    }
}

/* class Axe : Weapon
{
    public override int Range => 1;
    public override int Damage => 5;
    public override float Cooldown => 2;
} */

class Bow : Weapon
{
    public Bow()
    {
        Ammo = 10;
    }
    public override int Range => 10;
    public override int Damage => 3;
    public override float Cooldown => 1000;
    public override void Attack(Game game, Player player)
    {
        if (Ammo > 0 && RemainingCooldown <= 0)
        {
            Ammo--;
            game.Spawn(new Arrow(player.Position, player.direction, Damage));
            base.Attack(game, player);
        }
    }
}

class Crossbow : Weapon
{
    public Crossbow()
    {
        Ammo = 5;
    }
    public override int Range => 15;
    public override int Damage => 10;
    public override float Cooldown => 3000;
    public override void Attack(Game game, Player player)
    {
        if (Ammo > 0 && RemainingCooldown <= 0)
        {
            Ammo--;
            game.Spawn(new Bolt(player.Position, player.direction, Damage));
            base.Attack(game, player);
        }
    }
}

class Shotgun : Weapon
{
    public Shotgun()
    {
        Ammo = 25;
        angleBetweenShots = -(2 * startAngle / (numberShots - 1));
    }
    private Vector2 shotDirection;
    private Vector2 tempDirection;
    private float startAngle = 0.5f;
    private float angleBetweenShots;
    public int numberShots = 5;
    public override int Range => 5;
    public override int Damage => 7;
    public override float Cooldown => 4000;
    public override void Attack(Game game, Player player)
    {
        if (Ammo >= numberShots && RemainingCooldown <= 0)
        {
            Ammo -= numberShots;
            shotDirection = player.direction;
            /* Have to use temp variable, because assigning X would change the result of Y */
            tempDirection.X = MathF.Cos(startAngle) * shotDirection.X - MathF.Sin(startAngle) * shotDirection.Y;
            tempDirection.Y = MathF.Sin(startAngle) * shotDirection.X + MathF.Cos(startAngle) * shotDirection.Y;
            shotDirection = tempDirection;
            game.Spawn(new Shell(player.Position, shotDirection, Damage));
            for (int i = 1; i < numberShots; i++)
            {
                tempDirection.X = MathF.Cos(angleBetweenShots) * shotDirection.X - MathF.Sin(angleBetweenShots) * shotDirection.Y;
                tempDirection.Y = MathF.Sin(angleBetweenShots) * shotDirection.X + MathF.Cos(angleBetweenShots) * shotDirection.Y;
                shotDirection = tempDirection;
                game.Spawn(new Shell(player.Position, shotDirection, Damage));
                Console.WriteLine(shotDirection);
            }
            base.Attack(game, player);
        }
    }
}
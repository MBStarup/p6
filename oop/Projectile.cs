#define RENDERING
#define INPUT

using System.Numerics;

abstract class Projectile : GameObject
{
    public Vector2 Origin;
    public Vector2 Direction;
    public float Speed;
    public double RangeSquared;
    public int Damage;
    public override Circle Collider { get => new(Position.X, Position.Y, 2.5f); }
    public override void Update(Game game)
    {
        if ((Position - Origin).LengthSquared() > RangeSquared) game.Remove(this);
        Velocity = Direction * Speed;

        base.Update(game);
    }

    public override void OnCollision(Game game, GameObject other)
    {
        Enemy enemy = (other as Enemy);
        if (enemy != null)
        {
            enemy.TakeDamageFrom(game, this);
            game.Remove(this);
        }
    }
}

class Arrow : Projectile
{
    public Arrow(Vector2 origin, Vector2 direction, int damage)
    {

        Origin = origin;
        Direction = direction;
        Position = origin + direction * 15f;
        Speed = 0.5f;
        RangeSquared = 10000f;
        Damage = damage;
        Color = 0xFF6a329f;
    }
}

class Bolt : Projectile
{
    public Bolt(Vector2 origin, Vector2 direction, int damage)
    {
        Origin = origin;
        Direction = direction;
        Position = origin + direction * 15f;
        Speed = 0.5f;
        RangeSquared = 10000f;
        Damage = damage;
        PenetrationPower = 3;
        Color = 0xFFc90076;
    }
    public int PenetrationPower { get; set; }
    public override void OnCollision(Game game, GameObject other)
    {
        if (other is Enemy)
        {
            PenetrationPower--;
            ((Enemy)other).TakeDamageFrom(game, this);
        }
        if (PenetrationPower <= 0)
        {
            game.Remove(this);
        }
    }
}

class Shell : Projectile
{
    public Shell(Vector2 origin, Vector2 direction, int damage)
    {

        Origin = origin;
        Direction = direction;
        Position = origin + direction * 15f;
        Speed = 0.3f;
        RangeSquared = 10000f;
        Damage = damage;
        Color = 0xFF6a329f;
    }
}
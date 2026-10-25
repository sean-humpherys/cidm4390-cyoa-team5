public class Weapon
{
    private int _damage;

    public string Name { get; set; }

    public int Damage
    {
        get { return _damage; }
        set { _damage = Math.Max(0, value); }
    }

    public string Symbol { get; set; }

    public Weapon(string name, int damage, string symbol)
    {
        Name = name;
        Damage = damage;
        Symbol = symbol;
    }
}
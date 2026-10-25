using System;

public class Dragon
{
    private int _health;
    private int _strength;
    private int _agility;
    private int _armorClass;

    // Dragon attributes
    public string Name { get; set; }

    public int Strength
    {
        get { return _strength; }
        set { _strength = Math.Max(0, value); }
    }

    public int Health
    {
        get { return _health; }
        set { _health = Math.Max(0, value); }
    }

    public int Agility
    {
        get { return _agility; }
        set { _agility = Math.Max(0, value); }
    }

    public int ArmorClass
    {
        get { return _armorClass; }
        set { _armorClass = Math.Max(0, value); }
    }

    // Dragon's weapon
    public Weapon DragonWeapon { get; set; }

    // Constructor uses the existing Die system
    public Dragon(string name, Die die)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Dragon name cannot be empty.",
                nameof(name));
        }

        ArgumentNullException.ThrowIfNull(die);

        Name = name;

        // Roll dragon attributes using the existing Die
        Strength = die.Roll(20);
        Agility = die.Roll(20);
        Health = die.Roll(20);

        // Calculate armor class
        ArmorClass = 10 + (Agility / 2);

        // Give the dragon its default weapon
        DragonWeapon = new Weapon("claws", 12, "##");
    }

    // Basic attack damage
    public int BasicAttack()
    {
        return Strength;
    }

    // Special attack deals double strength damage
    public int SpecialAttack()
    {
        return Strength * 2;
    }

    // Reduce dragon health when attacked
    public void TakeDamage(int damage)
    {
        if (damage < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(damage),
                "Damage cannot be negative.");
        }

        Health = Math.Max(0, Health - damage);
    }

    // Check if dragon is alive
    public bool IsAlive()
    {
        return Health > 0;
    }

    // Taunt when player misses
    public string MissedMeTaunt()
    {
        return $"{Name} laughs as your attack misses!";
    }

    // Taunt when dragon is hit
    public string WasHitTaunt()
    {
        return $"{Name} roars in pain!";
    }

    // Display dragon statistics
    public string DisplayStats()
    {
        return $"Dragon: {Name}\n" +
               $"Health: {Health}\n" +
               $"Strength: {Strength}\n" +
               $"Agility: {Agility}\n" +
               $"Armor Class: {ArmorClass}\n" +
               $"Weapon: {DragonWeapon.Name}";
    }
}


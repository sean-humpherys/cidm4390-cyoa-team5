using System;

public class Dragon
{
    private int _health;
    private int _strength;
    private int _agility;
    private int _armorClass;

    // Used to pick random taunts
    private readonly Die _die;

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
        _die = die;

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

    // Taunt when the dragon defends the player's attack (messages 250-253).
    // playerRace fills in "Try harder, {race}!"
    public string MissedMeTaunt(Messages? messages = null, string playerRace = "")
    {
        messages ??= new Messages();
        return messages.GetMessage(250 + _die.Roll(4) - 1, playerRace);
    }

    // Taunt when the dragon is hit but still alive (messages 254-257)
    public string WasHitTaunt(Messages? messages = null)
    {
        messages ??= new Messages();
        return messages.GetMessage(254 + _die.Roll(4) - 1);
    }

    // Sarcastic reply after the dragon hurts the player (messages 260-263)
    public string HitPlayerTaunt(Messages? messages = null)
    {
        messages ??= new Messages();
        return messages.GetMessage(260 + _die.Roll(4) - 1);
    }

    // Display dragon statistics in the chosen language (labels are messages 130-137)
    public string DisplayStats(Messages? messages = null)
    {
        messages ??= new Messages();

        return string.Join(Environment.NewLine,
            messages.GetMessage(130, Name),
            messages.GetMessage(133, Strength),
            messages.GetMessage(134, Agility),
            messages.GetMessage(135, Health),
            messages.GetMessage(136, messages.GetMessage(124)), // claws
            messages.GetMessage(137, DragonWeapon.Damage));
    }
}

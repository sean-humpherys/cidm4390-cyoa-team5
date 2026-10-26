
using System;

public enum Race
{
    Human,
    Elf,
    Dwarf,
    Halfling
}

public enum Occupation
{
    Fighter,
    Magician,
    Thief,
    Archer
}

public class Player
{
    public string Name { get; }
    public Race Race { get; }
    public Occupation Occupation { get; }

    public int Strength { get; private set; }
    public int Agility { get; private set; }
    public int Health { get; private set; }

    public Weapon Weapon { get; }

    // Store the strength modifier for the health roll
    private int strengthModifier = 0;

    // The d20 roll and racial modifier from the most recent
    // RollStrength / RollAgility / RollHealth call, so the game can show them.
    public int LastDieRoll { get; private set; }
    public int LastModifier { get; private set; }

    // Used to pick a random taunt
    private readonly Die die;

    public Player(
        string name,
        Race race,
        Occupation occupation,
        Die die)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Player name cannot be empty.",
                nameof(name));
        }

        ArgumentNullException.ThrowIfNull(die);

        if (!Enum.IsDefined(race))
        {
            throw new ArgumentOutOfRangeException(
                nameof(race));
        }

        if (!Enum.IsDefined(occupation))
        {
            throw new ArgumentOutOfRangeException(
                nameof(occupation));
        }

        Name = name.Trim();
        Race = race;
        Occupation = occupation;
        this.die = die;

        Weapon = CreateStartingWeapon();

        // Attributes are now rolled individually
        // during character creation in Game.cs
    }

    // Assign starting weapon based on occupation.
    // ASCII art comes from the stakeholder (Appendix D / Attack Activity Diagram).
    private Weapon CreateStartingWeapon()
    {
        return Occupation switch
        {
            Occupation.Fighter =>
                new Weapon("Long Sword", 12, "-)=====>"),

            Occupation.Magician =>
                new Weapon("Lightning Bolt", 12, "zap~~~~~~"),

            Occupation.Thief =>
                new Weapon("Dagger", 6, "-)==>"),

            Occupation.Archer =>
                new Weapon("Long Bow", 8, "}     -->"),

            _ => throw new InvalidOperationException(
                "Unknown occupation.")
        };
    }

    // Roll Strength using D20 and racial modifiers
    public void RollStrength(Die die)
    {
        ArgumentNullException.ThrowIfNull(die);

        if (Race == Race.Halfling)
        {
            strengthModifier = -die.Roll(4);
        }
        else
        {
            strengthModifier = die.Roll(4);
        }

        LastDieRoll = die.Roll(20);
        LastModifier = strengthModifier;

        Strength = Math.Max(
            0,
            LastDieRoll + strengthModifier);
    }

    // Roll Agility using D20 and racial modifiers
    public void RollAgility(Die die)
    {
        ArgumentNullException.ThrowIfNull(die);

        int agilityModifier = 0;

        if (Race == Race.Elf ||
            Race == Race.Halfling)
        {
            agilityModifier = die.Roll(4);
        }

        LastDieRoll = die.Roll(20);
        LastModifier = agilityModifier;

        Agility = LastDieRoll + agilityModifier;
    }

    // Roll Health using D20 and the Strength modifier
    public void RollHealth(Die die)
    {
        ArgumentNullException.ThrowIfNull(die);

        // Human, Elf, and Dwarf receive the
        // same bonus used for Strength.
        // Halfling receives no Health modifier.
        int healthModifier =
            Race == Race.Halfling ? 0 : strengthModifier;

        LastDieRoll = die.Roll(20);
        LastModifier = healthModifier;

        Health = LastDieRoll + healthModifier;
    }

    // Reduce player health when the dragon's attack lands (never below zero)
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

    // Check if the player is still alive
    public bool IsAlive()
    {
        return Health > 0;
    }

    // Class Diagram: + string MissedMeTaunt()
    // Said by the player after defending against the dragon (messages 140-143).
    public string MissedMeTaunt(Messages? messages = null)
    {
        messages ??= new Messages();
        return messages.GetMessage(140 + die.Roll(4) - 1);
    }

    // Class Diagram: + string DisplayStats()
    // Character summary from the Create Character Activity Diagram,
    // in the player's chosen language (labels are messages 130-137).
    public string DisplayStats(Messages? messages = null)
    {
        messages ??= new Messages();

        return string.Join(Environment.NewLine,
            messages.GetMessage(130, Name),
            messages.GetMessage(131, messages.GetRaceName(Race)),
            messages.GetMessage(132, messages.GetOccupationName(Occupation)),
            messages.GetMessage(133, Strength),
            messages.GetMessage(134, Agility),
            messages.GetMessage(135, Health),
            messages.GetMessage(136, messages.GetWeaponName(Occupation)),
            messages.GetMessage(137, Weapon.Damage));
    }
}

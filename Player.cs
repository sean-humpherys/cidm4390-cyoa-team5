
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

        Weapon = CreateStartingWeapon();

        // Attributes are now rolled individually
        // during character creation in Game.cs
    }

    // Assign starting weapon based on occupation
    private Weapon CreateStartingWeapon()
    {
        return Occupation switch
        {
            Occupation.Fighter =>
                new Weapon("Long Sword", 12, "/====>"),

            Occupation.Magician =>
                new Weapon("Lightning Bolt", 12, "~~~>"),

            Occupation.Thief =>
                new Weapon("Dagger", 6, "/==>"),

            Occupation.Archer =>
                new Weapon("Long Bow", 8, ")---->"),

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

        Strength = Math.Max(
            0,
            die.Roll(20) + strengthModifier);
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

        Agility = die.Roll(20) + agilityModifier;
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

        Health = die.Roll(20) + healthModifier;
    }

    public void DisplayStats()
    {
        Console.WriteLine();
        Console.WriteLine("=== CHARACTER SUMMARY ===");
        Console.WriteLine($"Name: {Name}");
        Console.WriteLine($"Race: {Race}");
        Console.WriteLine($"Occupation: {Occupation}");
        Console.WriteLine();
        Console.WriteLine($"Strength: {Strength}");
        Console.WriteLine($"Agility: {Agility}");
        Console.WriteLine($"Health: {Health}");
        Console.WriteLine();
        Console.WriteLine($"Weapon: {Weapon.Name}");
        Console.WriteLine($"Max Damage: {Weapon.Damage}");
        Console.WriteLine($"Weapon Art: {Weapon.Symbol}");
        Console.WriteLine("=========================");
    }
}

using System;

public class Player
{
    // Identity and starting weapon are assigned only during construction.
    public string Name { get; }
    public Race Race { get; }
    public Occupation Occupation { get; }
    public Weapon Weapon { get; }

    // External code can read attributes; only Player can modify them.
    public int Strength { get; private set; }
    public int Agility { get; private set; }
    public int Health { get; private set; }

    // Details of the most recent attribute roll, for the game's display.
    public int LastDieRoll { get; private set; }
    public int LastModifier { get; private set; }

    // One shared die supplies attribute rolls and taunt selection.
    private readonly Die die;

    // Health reuses the d4 Strength modifier, not the full Strength score.
    private int strengthModifier;

    // 0 = Strength, 1 = Agility, 2 = Health, 3 = all attributes rolled.
    private int nextAttribute;

    // Completion of attribute rolling is separate from whether the player is alive.
    public bool AttributesRolled => nextAttribute == 3;

    public Player(
        string name,
        Race race,
        Occupation occupation,
        Die die)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Player name cannot be empty.", nameof(name));
        }

        ArgumentNullException.ThrowIfNull(die);

        if (!Enum.IsDefined(race))
        {
            throw new ArgumentOutOfRangeException(nameof(race));
        }

        if (!Enum.IsDefined(occupation))
        {
            throw new ArgumentOutOfRangeException(nameof(occupation));
        }

        Name = name.Trim();
        Race = race;
        Occupation = occupation;
        this.die = die;
        Weapon = CreateStartingWeapon();

        // Do not roll here: Game waits for each user action.
        // Integer fields and attributes initially default to zero.
    }

    // Game calls this once per user request; no console input occurs in Player.
    // The fixed order guarantees Strength is rolled before Health.
    public void RollNextAttribute()
    {
        switch (nextAttribute)
        {
            case 0:
                RollStrength();
                break;
            case 1:
                RollAgility();
                break;
            case 2:
                RollHealth();
                break;
            default:
                throw new InvalidOperationException(
                    "All player attributes have already been rolled.");
        }

        // Advance only after the current roll completes successfully.
        nextAttribute++;
    }

    // Only the constructor needs to create the starting weapon.
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
            _ => throw new InvalidOperationException("Unknown occupation.")
        };
    }

    // Consolidates the d20 calculation and tracking shared by all attributes.
    private int RollAttribute(int modifier)
    {
        LastDieRoll = die.Roll(20);
        LastModifier = modifier;
        return LastDieRoll + modifier;
    }

    private void RollStrength()
    {
        // Preserve the original order: roll d4 first, then d20.
        int modifier = die.Roll(4);
        strengthModifier = Race == Race.Halfling ? -modifier : modifier;
        Strength = Math.Max(0, RollAttribute(strengthModifier));
    }

    private void RollAgility()
    {
        // Only Elves and Halflings receive a d4 bonus.
        int modifier = Race == Race.Elf || Race == Race.Halfling
            ? die.Roll(4)
            : 0;
        Agility = RollAttribute(modifier);
    }

    private void RollHealth()
    {
        // Halflings ignore the Strength penalty when rolling Health.
        int modifier = Race == Race.Halfling ? 0 : strengthModifier;
        Health = RollAttribute(modifier);
    }

    // Remains public because Combat needs to apply damage.
    public void TakeDamage(int damage)
    {
        if (damage < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(damage), "Damage cannot be negative.");
        }

        Health = Math.Max(0, Health - damage);
    }

    // Game should finish attribute creation before beginning combat.
    public bool IsAlive()
    {
        return Health > 0;
    }

    // Return one of message IDs 140-143 after a successful defense.
    public string MissedMeTaunt(Messages? messages = null)
    {
        messages ??= new Messages();
        return messages.GetMessage(140 + die.Roll(4) - 1);
    }

    // Return localized text; Game decides when and where to display it.
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
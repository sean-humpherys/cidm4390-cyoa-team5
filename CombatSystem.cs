// CombatSystem.cs  (Class Diagram: class Combat)
// One attack per method, following the Attack Activity Diagram and Appendix D:
//   Attack hits     when a d20 roll  <= the attacker's Strength.
//   Defense works   when a d20 roll  <= the defender's Agility.
//   Damage          is a roll up to the attacker's weapon damage.
// Game.StartCombat() runs the rounds and asks retreat/attack between them.
using System;

public class CombatSystem
{
    public Player Player { get; }
    public Dragon Dragon { get; }

    private readonly Die _die;
    private readonly Messages _messages;

    public CombatSystem(Player player, Dragon dragon, Die die, Messages messages)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(dragon);
        ArgumentNullException.ThrowIfNull(die);
        ArgumentNullException.ThrowIfNull(messages);

        Player = player;
        Dragon = dragon;
        _die = die; // Incorporates the 'rolls' relationship to the Die class
        _messages = messages;
    }

    // True once either side has been defeated
    public bool IsOver => !Player.IsAlive() || !Dragon.IsAlive();

    public void PlayerAttacksDragonSequence()
    {
        if (IsOver) return; // Prevent invalid combat action if someone is already defeated

        Console.WriteLine(_messages.GetMessage(300,
            _messages.GetOccupationName(Player.Occupation),
            _messages.GetWeaponName(Player.Occupation)));
        Console.WriteLine(Player.Weapon.Symbol);

        // Does the player's attack hit?
        if (_die.Roll(20) > Player.Strength)
        {
            Console.WriteLine(_messages.GetMessage(302, Dragon.Name)); // You missed
            return;
        }

        Console.WriteLine(_messages.GetMessage(301, Dragon.Name)); // You hit

        // Does the dragon defend?
        if (_die.Roll(20) <= Dragon.Agility)
        {
            Console.WriteLine(_messages.GetMessage(303, Dragon.Name));
            Console.WriteLine(Dragon.MissedMeTaunt(_messages, _messages.GetRaceName(Player.Race)));
            return;
        }

        int damage = RollDamage(Player.Weapon);
        Dragon.TakeDamage(damage);
        Console.WriteLine(_messages.GetMessage(304, damage, Dragon.Health));

        if (!Dragon.IsAlive())
        {
            Console.WriteLine(_messages.GetMessage(305)); // 'Arggg!' you have defeated the dragon
        }
        else
        {
            Console.WriteLine(Dragon.WasHitTaunt(_messages));
        }
    }

    public void DragonAttacksPlayerSequence()
    {
        if (IsOver) return; // Prevent invalid combat action if someone is already defeated

        Console.WriteLine(_messages.GetMessage(306)); // Dragon's turn to attack

        // Does the dragon's attack hit?
        if (_die.Roll(20) > Dragon.Strength)
        {
            Console.WriteLine(_messages.GetMessage(308)); // The dragon missed you
            return;
        }

        Console.WriteLine(_messages.GetMessage(307, Dragon.Name)); // hits you

        // Does the player defend?
        if (_die.Roll(20) <= Player.Agility)
        {
            Console.WriteLine(_messages.GetMessage(309));
            Console.WriteLine(Player.MissedMeTaunt(_messages));
            return;
        }

        int damage = RollDamage(Dragon.DragonWeapon);
        Player.TakeDamage(damage);
        Console.WriteLine(_messages.GetMessage(310, damage, Player.Health));

        if (!Player.IsAlive())
        {
            Console.WriteLine(_messages.GetMessage(311)); // 'Oh no!' you have been defeated
        }
        else
        {
            Console.WriteLine(Dragon.HitPlayerTaunt(_messages));
        }
    }

    // "Roll a random number up to the maximum damage points of the weapon"
    private int RollDamage(Weapon weapon)
    {
        return weapon.Damage < 1 ? 0 : _die.Roll(weapon.Damage);
    }
}

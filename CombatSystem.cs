// CombatSystem.cs
using System;

public class CombatSystem
{
    public Player Player { get; set; }
    public Dragon Dragon { get; set; }
    private Die _die;

    // Matches the constructor structure from image_4045fc.png while allowing dependency injection for testing
    public CombatSystem(Player player, Dragon dragon, Die die = null) 
    {
        Player = player;
        Dragon = dragon;
        _die = die ?? new Die(); // Incorporates the 'rolls' relationship to the Die class
    }

    public void StartCombat()
    {
        // Manages turn progression alternating between player and enemy
        while (Player.Health > 0 && Dragon.Health > 0)
        {
            PlayerAttacksDragonSequence();

            if (Dragon.Health <= 0)
            {
                break;
            }

            DragonAttacksPlayerSequence();
        }
    }

    public void PlayerAttacksDragonSequence()
    {
        if (Player.Health <= 0) return; // Prevent invalid combat action if player is already defeated

        // Determine attack success and defense outcomes using the Die class
        int attackRoll = _die.Roll(20) + Player.Strength;
        int defenseRoll = _die.Roll(20) + Dragon.Agility;

        if (attackRoll > defenseRoll)
        {
            int damage = _die.Roll(Player.Weapon.MaxDamage);
            Dragon.Health -= damage;
            
            // Handle out-of-bounds health states
            if (Dragon.Health < 0)
            {
                Dragon.Health = 0;
            }
        }
    }

    public void DragonAttacksPlayerSequence()
    {
        if (Dragon.Health <= 0) return; // Prevent invalid combat action if dragon is already defeated

        int attackRoll = _die.Roll(20) + Dragon.Strength;
        int defenseRoll = _die.Roll(20) + Player.Agility;

        if (attackRoll > defenseRoll)
        {
            int damage = _die.Roll(Dragon.Weapon.MaxDamage);
            Player.Health -= damage;
            
            // Handle out-of-bounds health states
            if (Player.Health < 0)
            {
                Player.Health = 0;
            }
        }
    }
}
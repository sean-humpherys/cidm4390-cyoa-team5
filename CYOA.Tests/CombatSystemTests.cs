// CombatSystemTests.cs
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class CombatSystemTests
{
    // A controllable Die to guarantee deterministic unit test results 
    public class MockDie : Die
    {
        private int _fixedRollValue;
        
        public MockDie(int fixedRollValue)
        {
            _fixedRollValue = fixedRollValue;
        }

        public override int Roll(int sides)
        {
            return _fixedRollValue;
        }
    }

    [TestMethod]
    public void PlayerAttacksDragonSequence_HitSuccess_ReducesDragonHealth()
    {
        // Arrange
        var player = new Player { Health = 20, Strength = 10, Weapon = new Weapon { MaxDamage = 8 } };
        var dragon = new Dragon { Health = 20, Agility = 2 };
        
        // MockDie set to 5. 
        // Player attack: 5 + 10 (Strength) = 15
        // Dragon defense: 5 + 2 (Agility) = 7. Hit succeeds. 
        // Damage dealt: 5. Dragon health goes from 20 -> 15.
        var combat = new CombatSystem(player, dragon, new MockDie(5));

        // Act
        combat.PlayerAttacksDragonSequence();

        // Assert
        Assert.AreEqual(15, dragon.Health, "Dragon's health should be reduced by the exact mocked die damage roll.");
    }

    [TestMethod]
    public void DragonAttacksPlayerSequence_Miss_LeavesPlayerHealthUnchanged()
    {
        // Arrange
        var player = new Player { Health = 20, Agility = 10 };
        var dragon = new Dragon { Health = 20, Strength = 2, Weapon = new Weapon { MaxDamage = 10 } };
        
        // MockDie set to 5.
        // Dragon attack: 5 + 2 (Strength) = 7.
        // Player defense: 5 + 10 (Agility) = 15. Hit fails.
        var combat = new CombatSystem(player, dragon, new MockDie(5));

        // Act
        combat.DragonAttacksPlayerSequence();

        // Assert
        Assert.AreEqual(20, player.Health, "Player's health should remain unchanged on a missed attack.");
    }

    [TestMethod]
    public void CombatSequence_HealthDoesNotDropBelowZero()
    {
        // Arrange
        var player = new Player { Health = 2, Strength = 10, Weapon = new Weapon { MaxDamage = 15 } };
        var dragon = new Dragon { Health = 3, Agility = 2, Weapon = new Weapon { MaxDamage = 15 } };
        
        // Rolling a 10 deals 10 damage, which is more than either entity's current health
        var combat = new CombatSystem(player, dragon, new MockDie(10)); 

        // Act
        combat.PlayerAttacksDragonSequence();

        // Assert
        Assert.AreEqual(0, dragon.Health, "Dragon's health should bottom out at 0 instead of entering negative values.");
    }

    [TestMethod]
    public void StartCombat_LoopTerminates_WhenHealthReachesZero()
    {
        // Arrange
        var player = new Player { Health = 50, Strength = 20, Weapon = new Weapon { MaxDamage = 10 } };
        var dragon = new Dragon { Health = 10, Agility = 1, Strength = 1, Weapon = new Weapon { MaxDamage = 10 } };
        
        var combat = new CombatSystem(player, dragon, new MockDie(5));

        // Act
        combat.StartCombat(); // Loop will automatically execute attacks until someone drops to 0

        // Assert
        Assert.AreEqual(0, dragon.Health, "Combat loop should properly conclude with Dragon defeated.");
        Assert.IsTrue(player.Health > 0, "Player should survive the encounter.");
    }

    [TestMethod]
    public void ActionValidation_DeadEntityCannotAttack()
    {
        // Arrange
        var player = new Player { Health = 0, Strength = 20, Weapon = new Weapon { MaxDamage = 10 } };
        var dragon = new Dragon { Health = 20, Agility = 1 };
        
        var combat = new CombatSystem(player, dragon, new MockDie(10));

        // Act
        combat.PlayerAttacksDragonSequence();

        // Assert
        Assert.AreEqual(20, dragon.Health, "A player with 0 health should not be able to execute an attack sequence.");
    }
}
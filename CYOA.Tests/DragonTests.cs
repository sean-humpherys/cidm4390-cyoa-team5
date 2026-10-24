using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class DragonTests
{
    // Create a Dragon using the existing Die class
    private Dragon CreateDragon()
    {
        Die die = new Die();
        return new Dragon("Fire Dragon", die);
    }

    // Test 1: Verify the Dragon's name
    [TestMethod]
    public void Dragon_Constructor_SetsName()
    {
        Dragon dragon = CreateDragon();

        Assert.AreEqual("Fire Dragon", dragon.Name);
    }

    // Test 2: Verify randomly generated attributes
    [TestMethod]
    public void Dragon_Constructor_GeneratesValidAttributes()
    {
        Dragon dragon = CreateDragon();

        Assert.IsTrue(dragon.Strength >= 1 &&
                      dragon.Strength <= 20);

        Assert.IsTrue(dragon.Health >= 1 &&
                      dragon.Health <= 20);

        Assert.IsTrue(dragon.Agility >= 1 &&
                      dragon.Agility <= 20);
    }

    // Test 3: Verify Armor Class calculation
    [TestMethod]
    public void Dragon_Constructor_CalculatesArmorClass()
    {
        Dragon dragon = CreateDragon();

        int expectedArmor = 10 + (dragon.Agility / 2);

        Assert.AreEqual(expectedArmor, dragon.ArmorClass);
    }

    // Test 4: Verify the Dragon starts with claws
    [TestMethod]
    public void Dragon_Constructor_CreatesClaws()
    {
        Dragon dragon = CreateDragon();

        Assert.AreEqual("claws", dragon.DragonWeapon.Name);
        Assert.AreEqual(12, dragon.DragonWeapon.Damage);
        Assert.AreEqual("##", dragon.DragonWeapon.Symbol);
    }

    // Test 5: Verify basic attack damage
    [TestMethod]
    public void Dragon_BasicAttack_ReturnsStrength()
    {
        Dragon dragon = CreateDragon();

        dragon.Strength = 10;

        int damage = dragon.BasicAttack();

        Assert.AreEqual(10, damage);
    }

    // Test 6: Verify special attack damage
    [TestMethod]
    public void Dragon_SpecialAttack_ReturnsDoubleStrength()
    {
        Dragon dragon = CreateDragon();

        dragon.Strength = 10;

        int damage = dragon.SpecialAttack();

        Assert.AreEqual(20, damage);
    }

    // Test 7: Verify health decreases when damaged
    [TestMethod]
    public void Dragon_TakeDamage_ReducesHealth()
    {
        Dragon dragon = CreateDragon();

        dragon.Health = 20;

        dragon.TakeDamage(5);

        Assert.AreEqual(15, dragon.Health);
    }

    // Test 8: Verify health cannot drop below zero
    [TestMethod]
    public void Dragon_TakeDamage_CannotGoBelowZero()
    {
        Dragon dragon = CreateDragon();

        dragon.Health = 10;

        dragon.TakeDamage(50);

        Assert.AreEqual(0, dragon.Health);
    }

    // Test 9: Verify negative health becomes zero
    [TestMethod]
    public void Dragon_Health_NegativeValueBecomesZero()
    {
        Dragon dragon = CreateDragon();

        dragon.Health = -10;

        Assert.AreEqual(0, dragon.Health);
    }

    // Test 10: Verify negative damage throws an exception
    [TestMethod]
    public void Dragon_TakeDamage_NegativeDamageThrowsException()
    {
        Dragon dragon = CreateDragon();

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            () => dragon.TakeDamage(-5));
    }

    // Test 11: Verify Dragon is dead at zero health
    [TestMethod]
    public void Dragon_IsAlive_ReturnsFalseAtZeroHealth()
    {
        Dragon dragon = CreateDragon();

        dragon.Health = 0;

        Assert.IsFalse(dragon.IsAlive());
    }

    // Test 12: Verify Dragon is alive with positive health
    [TestMethod]
    public void Dragon_IsAlive_ReturnsTrueWithHealth()
    {
        Dragon dragon = CreateDragon();

        dragon.Health = 10;

        Assert.IsTrue(dragon.IsAlive());
    }

    // Test 13: Verify DisplayStats includes Dragon name
    [TestMethod]
    public void Dragon_DisplayStats_ContainsName()
    {
        Dragon dragon = CreateDragon();

        string stats = dragon.DisplayStats();

        Assert.IsTrue(stats.Contains("Fire Dragon"));
    }

    // Test 14: Verify empty names are rejected
    [TestMethod]
    public void Dragon_Constructor_EmptyNameThrowsException()
    {
        Die die = new Die();

        Assert.ThrowsExactly<ArgumentException>(
            () => new Dragon("", die));
    }

    // Test 15: Verify null Die objects are rejected
    [TestMethod]
    public void Dragon_Constructor_NullDieThrowsException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(
            () => new Dragon("Fire Dragon", null!));
    }

    // Test 16: Verify Dragon taunts return messages
    [TestMethod]
    public void Dragon_Taunts_ReturnMessages()
    {
        Dragon dragon = CreateDragon();

        Assert.IsFalse(string.IsNullOrWhiteSpace(
            dragon.MissedMeTaunt()));

        Assert.IsFalse(string.IsNullOrWhiteSpace(
            dragon.WasHitTaunt()));
    }
}
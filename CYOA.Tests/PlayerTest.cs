
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

[TestClass]
public class PlayerTests
{
    private readonly Die die = new Die();

    // Verify player information is stored correctly
    [TestMethod]
    public void Constructor_StoresPlayerInformation()
    {
        Player player = new Player(
            "Test Hero",
            Race.Human,
            Occupation.Fighter,
            die);

        Assert.AreEqual("Test Hero", player.Name);
        Assert.AreEqual(Race.Human, player.Race);
        Assert.AreEqual(Occupation.Fighter, player.Occupation);
    }

    // Verify empty player names are rejected
    [TestMethod]
    public void Constructor_EmptyName_ThrowsException()
    {
        Assert.ThrowsExactly<ArgumentException>(() =>
            new Player("", Race.Human,
                Occupation.Fighter, die));
    }

    // Verify null dice are rejected
    [TestMethod]
    public void Constructor_NullDie_ThrowsException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() =>
            new Player("Hero", Race.Human,
                Occupation.Fighter, null!));
    }

    // Verify invalid races are rejected
    [TestMethod]
    public void Constructor_InvalidRace_ThrowsException()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
            new Player("Hero", (Race)99,
                Occupation.Fighter, die));
    }

    // Verify invalid occupations are rejected
    [TestMethod]
    public void Constructor_InvalidOccupation_ThrowsException()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
            new Player("Hero", Race.Human,
                (Occupation)99, die));
    }

    // Verify weapons are assigned by occupation
    [TestMethod]
    [DataRow(Occupation.Fighter, "Long Sword", 12)]
    [DataRow(Occupation.Magician, "Lightning Bolt", 12)]
    [DataRow(Occupation.Thief, "Dagger", 6)]
    [DataRow(Occupation.Archer, "Long Bow", 8)]
    public void Constructor_AssignsCorrectWeapon(
        Occupation occupation,
        string expectedName,
        int expectedDamage)
    {
        Player player = new Player(
            "Hero", Race.Human, occupation, die);

        Assert.AreEqual(expectedName, player.Weapon.Name);
        Assert.AreEqual(expectedDamage, player.Weapon.Damage);
    }

    // Verify Strength rolls for Human, Elf, and Dwarf
    [TestMethod]
    [DataRow(Race.Human)]
    [DataRow(Race.Elf)]
    [DataRow(Race.Dwarf)]
    public void RollStrength_PositiveModifier_WithinRange(
        Race race)
    {
        Player player = new Player(
            "Hero", race, Occupation.Fighter, die);

        player.RollStrength(die);

        Assert.IsTrue(
            player.Strength >= 2 &&
            player.Strength <= 24);
    }

    // Verify Halfling Strength never becomes negative
    [TestMethod]
    public void RollStrength_Halfling_WithinRange()
    {
        Player player = new Player(
            "Hero", Race.Halfling,
            Occupation.Fighter, die);

        player.RollStrength(die);

        Assert.IsTrue(
            player.Strength >= 0 &&
            player.Strength <= 19);
    }

    // Verify Agility rolls for Elf and Halfling
    [TestMethod]
    [DataRow(Race.Elf)]
    [DataRow(Race.Halfling)]
    public void RollAgility_BonusRaces_WithinRange(
        Race race)
    {
        Player player = new Player(
            "Hero", race, Occupation.Fighter, die);

        player.RollAgility(die);

        Assert.IsTrue(
            player.Agility >= 2 &&
            player.Agility <= 24);
    }

    // Verify Agility rolls without racial bonus
    [TestMethod]
    [DataRow(Race.Human)]
    [DataRow(Race.Dwarf)]
    public void RollAgility_NormalRaces_WithinRange(
        Race race)
    {
        Player player = new Player(
            "Hero", race, Occupation.Fighter, die);

        player.RollAgility(die);

        Assert.IsTrue(
            player.Agility >= 1 &&
            player.Agility <= 20);
    }

    // Verify Health uses the Strength modifier
    [TestMethod]
    public void RollHealth_Human_WithinRange()
    {
        Player player = new Player(
            "Hero", Race.Human,
            Occupation.Fighter, die);

        player.RollStrength(die);
        player.RollHealth(die);

        Assert.IsTrue(
            player.Health >= 2 &&
            player.Health <= 24);
    }

    // Verify Halfling Health has no racial modifier
    [TestMethod]
    public void RollHealth_Halfling_WithinRange()
    {
        Player player = new Player(
            "Hero", Race.Halfling,
            Occupation.Fighter, die);

        player.RollStrength(die);
        player.RollHealth(die);

        Assert.IsTrue(
            player.Health >= 1 &&
            player.Health <= 20);
    }

    // Verify attributes start at zero before rolling
    [TestMethod]
    public void Constructor_AttributesStartAtZero()
    {
        Player player = new Player(
            "Hero", Race.Human,
            Occupation.Fighter, die);

        Assert.AreEqual(0, player.Strength);
        Assert.AreEqual(0, player.Agility);
        Assert.AreEqual(0, player.Health);
    }
}


using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class DieTests
{
    [TestMethod]
    public void Roll_Default_ReturnsBetween1And20()
    {
        Die die = new Die();

        for (int i = 0; i < 1000; i++)
        {
            int result = die.Roll();

            Assert.IsTrue(result >= 1 && result <= 20);
        }
    }

    [TestMethod]
    [DataRow(4)]
    [DataRow(6)]
    [DataRow(8)]
    [DataRow(12)]
    [DataRow(20)]
    public void Roll_CustomSides_ReturnsValidRange(int sides)
    {
        Die die = new Die();

        for (int i = 0; i < 1000; i++)
        {
            int result = die.Roll(sides);

            Assert.IsTrue(
                result >= 1 && result <= sides,
                $"Roll({sides}) returned {result}.");
        }
    }

    [TestMethod]
    public void Roll_OneSide_AlwaysReturnsOne()
    {
        Die die = new Die();

        for (int i = 0; i < 100; i++)
        {
            Assert.AreEqual(1, die.Roll(1));
        }
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(-1)]
    [DataRow(-20)]
    public void Roll_InvalidSides_ThrowsException(int sides)
    {
        Die die = new Die();

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            () => die.Roll(sides));
    }

    [TestMethod]
    public void Roll_MultipleCalls_ReturnValidResults()
    {
        Die die = new Die();

        int firstRoll = die.Roll();
        int secondRoll = die.Roll();
        int thirdRoll = die.Roll();

        Assert.IsTrue(firstRoll >= 1 && firstRoll <= 20);
        Assert.IsTrue(secondRoll >= 1 && secondRoll <= 20);
        Assert.IsTrue(thirdRoll >= 1 && thirdRoll <= 20);
    }
}

namespace CYOA.Tests;

// Plays the whole game from start to finish with typed answers and a ScriptedDie,
// checking each path from the Start Game, Attack and End Game Activity Diagrams.
//
// Order of dice rolls in every game:
//   1. Dragon (when Game is created): Strength, Agility, Health
//   2. Human player: d4 strength bonus, d20 Strength, d20 Agility, d20 Health
//   3. Combat rolls, if the player attacks
[TestClass]
[DoNotParallelize]
public sealed class AdventureTests
{
    private TextReader originalIn = null!;
    private TextWriter originalOut = null!;

    [TestInitialize]
    public void SaveConsole()
    {
        originalIn = Console.In;
        originalOut = Console.Out;
    }

    [TestCleanup]
    public void RestoreConsole()
    {
        Console.SetIn(originalIn);
        Console.SetOut(originalOut);
    }

    // Typed answers to get from the language menu to the adventure:
    // English, Create Character, Human, name, Fighter, roll x3, Proceed.
    private static readonly string[] CreateHumanFighter =
        ["1", "1", "1", "Aria", "1", "roll", "roll", "roll", "2"];

    private static string Play(Die die, params string[] inputLines)
    {
        StringWriter output = new StringWriter();
        Console.SetIn(new StringReader(string.Join(Environment.NewLine, inputLines) + Environment.NewLine));
        Console.SetOut(output);

        new Game(die).StartGame();

        return output.ToString();
    }

    private static string[] Then(string[] start, params string[] more)
    {
        return [.. start, .. more];
    }

    private static int CountOf(string text, string value)
    {
        int count = 0;
        int index = 0;
        while ((index = text.IndexOf(value, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += value.Length;
        }
        return count;
    }

    private const string AdventureMenu = "You are walking in the mountains.";

    [TestMethod]
    public void South_ShowsBoredMessage_AndReturnsToAdventureMenu()
    {
        ScriptedDie dice = new ScriptedDie(10, 10, 10, /* player */ 1, 9, 10, 9);

        string output = Play(dice, Then(CreateHumanFighter, "s", "e"));

        Assert.Contains("You take the South path.", output);
        Assert.AreEqual(2, CountOf(output, AdventureMenu));
        Assert.EndsWith("Adventure ends. Bye." + Environment.NewLine, output);
    }

    [TestMethod]
    public void North_ShowsDragonAndStats_ThenRetreatEndsGame()
    {
        ScriptedDie dice = new ScriptedDie(13, 7, 16, /* player */ 1, 9, 10, 9);

        string output = Play(dice, Then(CreateHumanFighter, "n", "r"));

        Assert.Contains("You take the North path.", output);
        Assert.Contains("The dragon says 'I am Fire Dragon. Leave or perish!'", output);
        Assert.Contains("Your magic ring gives you the stats on the dragon.", output);
        Assert.Contains("Strength: 13", output);
        Assert.Contains("Agility: 7", output);
        Assert.Contains("Health points: 16", output);
        Assert.Contains("Do you wish to retreat or attack?", output);
        Assert.EndsWith("You quickly retreat. You come home poor but alive to adventure another day. The End." + Environment.NewLine, output);
    }

    [TestMethod]
    public void Attack_DefeatingTheDragon_ShowsVictoryEnding()
    {
        // Dragon: Str 10, Agi 1, Health 5. Player: Str 20, Agi 10, Health 20.
        ScriptedDie dice = new ScriptedDie(10, 1, 5, /* player */ 1, 19, 10, 19);
        dice.Add(1, 20, 12); // player hits, dragon fails to defend, 12 damage

        string output = Play(dice, Then(CreateHumanFighter, "n", "a"));

        Assert.Contains("you have defeated the dragon", output);
        Assert.EndsWith("With the dragon vanquished, you collect the hoard of gold, return home, rich and famous. The End." + Environment.NewLine, output);
        Assert.DoesNotContain("Dragon's turn to attack.", output);
    }

    [TestMethod]
    public void Attack_PlayerLosesAllHealth_ShowsDefeatEnding()
    {
        // Dragon: Str 20, Agi 20, Health 20. Player: Str 2, Agi 1, Health 2.
        ScriptedDie dice = new ScriptedDie(20, 20, 20, /* player */ 1, 1, 1, 1);
        dice.Add(20);         // player misses
        dice.Add(1, 20, 12);  // dragon hits, player fails to defend, 12 damage

        string output = Play(dice, Then(CreateHumanFighter, "n", "a"));

        Assert.Contains("you have been defeated by the dragon", output);
        Assert.EndsWith("The dragon laughs as your eyesight fades. You are defeated. Your family mourns their loss. The End." + Environment.NewLine, output);
    }

    [TestMethod]
    public void RetreatDuringCombat_ReturnsToAdventureMenu()
    {
        ScriptedDie dice = new ScriptedDie(10, 10, 20, /* player */ 1, 9, 10, 19);
        dice.Add(20); // player misses
        dice.Add(20); // dragon misses

        string output = Play(dice, Then(CreateHumanFighter, "n", "a", "r", "e"));

        Assert.Contains("You hastily retreat licking your wounds.", output);
        Assert.AreEqual(2, CountOf(output, AdventureMenu));
        Assert.DoesNotContain("The End.", output);
        Assert.EndsWith("Adventure ends. Bye." + Environment.NewLine, output);
    }

    [TestMethod]
    public void AttackAgain_ContinuesToTheNextRound()
    {
        ScriptedDie dice = new ScriptedDie(10, 1, 5, /* player Str 10 */ 1, 9, 10, 19);
        dice.Add(11, 11);    // round 1: both miss
        dice.Add(1, 20, 12); // round 2: player hits for 12, dragon defeated

        string output = Play(dice, Then(CreateHumanFighter, "n", "a", "a"));

        Assert.AreEqual(1, CountOf(output, "Dragon's turn to attack."));
        // Once in "Do you wish to retreat or attack?" before combat, once between rounds 1 and 2
        Assert.AreEqual(2, CountOf(output, "Enter 'r' for retreat."));
        Assert.Contains("With the dragon vanquished", output);
    }

    [TestMethod]
    public void ListCharacterStats_ShowsSummaryWithNameAsTyped()
    {
        ScriptedDie dice = new ScriptedDie(10, 10, 10, /* player */ 2, 12, 9, 15);
        string[] inputs = ["1", "1", "1", "Aria", "1", "roll", "roll", "roll", "1", "2", "e"];

        string output = Play(dice, inputs);

        Assert.Contains("Name: Aria", output);
        Assert.Contains("Race: Human", output);
        Assert.Contains("Occupation: Fighter", output);
        Assert.Contains("Strength: 14", output);   // 12 + 2
        Assert.Contains("Agility: 9", output);
        Assert.Contains("Health points: 17", output); // 15 + 2
        Assert.Contains("Weapon: long sword", output);
        Assert.Contains("Weapon maximum damage: 12", output);
    }

    [TestMethod]
    public void RollingStats_ShowsEachDieRollModifierAndStatsSoFar()
    {
        // Elf: d4 +3 then d20 14 for Strength, d4 +2 then d20 9 for Agility, d20 11 (+3) for Health
        ScriptedDie dice = new ScriptedDie(10, 10, 10, /* player */ 3, 14, 2, 9, 11);
        string[] inputs = ["1", "1", "2", "Aria", "4", "roll", "roll", "roll", "2", "e"];

        string output = Play(dice, inputs);

        Assert.Contains("| 14  |  d20", output);           // the die picture
        Assert.Contains("You rolled a 14 on the 20-sided die.", output);
        Assert.Contains("Elf racial modifier: +3", output);
        Assert.Contains("Elf racial modifier: +2", output);
        Assert.Contains("You rolled a 11 on the 20-sided die.", output);
        Assert.Contains("  Strength: 17", output);
        Assert.Contains("  Agility: 11", output);
        Assert.Contains("  Health points: 14", output);
        Assert.AreEqual(3, CountOf(output, "Your stats so far:"));
    }

    [TestMethod]
    public void RollingStats_HalflingShowsNegativeModifier_HumanAgilityShowsNone()
    {
        // Halfling: d4 2 subtracted from Strength roll 10; Agility d4 1 + d20 5; Health d20 7 (no modifier)
        ScriptedDie halflingDice = new ScriptedDie(10, 10, 10, 2, 10, 1, 5, 7);
        string halfling = Play(halflingDice, ["1", "1", "4", "Pip", "3", "roll", "roll", "roll", "2", "e"]);

        Assert.Contains("Halfling racial modifier: -2", halfling);
        Assert.Contains("  Strength: 8", halfling);
        Assert.Contains("No racial modifier.", halfling);   // Halfling health
        Assert.Contains("  Health points: 7", halfling);

        // Human: no agility modifier
        ScriptedDie humanDice = new ScriptedDie(10, 10, 10, 1, 9, 10, 9);
        string human = Play(humanDice, Then(CreateHumanFighter, "e"));

        Assert.Contains("No racial modifier.", human);
        Assert.Contains("  Agility: 10", human);
    }

    [TestMethod]
    public void InvalidAdventureChoice_RePrompts()
    {
        ScriptedDie dice = new ScriptedDie(10, 10, 10, /* player */ 1, 9, 10, 9);

        string output = Play(dice, Then(CreateHumanFighter, "x", "e"));

        Assert.Contains("Invalid choice. Please try again.", output);
        Assert.AreEqual(2, CountOf(output, AdventureMenu));
    }

    [TestMethod]
    public void InvalidRetreatOrAttackChoice_RePrompts()
    {
        ScriptedDie dice = new ScriptedDie(10, 10, 10, /* player */ 1, 9, 10, 9);

        string output = Play(dice, Then(CreateHumanFighter, "n", "x", "r"));

        Assert.AreEqual(2, CountOf(output, "Do you wish to retreat or attack?"));
        Assert.Contains("You quickly retreat.", output);
    }

    [TestMethod]
    public void TypingSomethingOtherThanRoll_AsksAgain()
    {
        ScriptedDie dice = new ScriptedDie(10, 10, 10, /* player */ 1, 9, 10, 9);
        string[] inputs = ["1", "1", "1", "Aria", "1", "jump", "roll", "roll", "roll", "2", "e"];

        string output = Play(dice, inputs);

        Assert.AreEqual(1, CountOf(output, "Please type 'roll'."));
        Assert.EndsWith("Adventure ends. Bye." + Environment.NewLine, output);
    }

    [TestMethod]
    public void WholeGame_PlaysInSpanish()
    {
        ScriptedDie dice = new ScriptedDie(10, 1, 5, /* player */ 1, 19, 10, 19);
        dice.Add(1, 20, 12);
        string[] inputs = ["2", "1", "1", "Aria", "1", "roll", "roll", "roll", "2", "n", "a"];

        string output = Play(dice, inputs);

        Assert.Contains("El dragón dice: 'Soy Fire Dragon. ¡Vete o perece!'", output);
        Assert.Contains("Como Guerrero, sacas tu espada larga.", output);
        Assert.EndsWith("Con el dragón vencido, recoges el tesoro de oro y regresas a casa, rico y famoso. Fin." + Environment.NewLine, output);
    }

    [TestMethod]
    [DataRow(EndCondition.Retreat, "You quickly retreat.")]
    [DataRow(EndCondition.DragonDefeated, "With the dragon vanquished")]
    [DataRow(EndCondition.PlayerDefeated, "The dragon laughs as your eyesight fades.")]
    [DataRow(EndCondition.Quit, "Adventure ends. Bye.")]
    public void EndGame_ShowsTheMatchingEnding(EndCondition condition, string expected)
    {
        StringWriter output = new StringWriter();
        Console.SetOut(output);

        new Game(new ScriptedDie(10, 10, 10)).EndGame(condition);

        Assert.Contains(expected, output.ToString());
    }
}

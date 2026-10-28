namespace CYOA.Tests;

// Tests for Dialogue.cs: spoken lines are labeled with the speaker and the
// narration color is always put back afterwards.
// These capture Console output, so they can't run in parallel with other console tests.
[TestClass]
[DoNotParallelize]
public sealed class DialogueTests
{
    private TextWriter originalOut = null!;
    private ConsoleColor originalColor;
    private StringWriter output = null!;

    [TestInitialize]
    public void Setup()
    {
        originalOut = Console.Out;
        originalColor = Console.ForegroundColor;
        output = new StringWriter();
        Console.SetOut(output);
    }

    [TestCleanup]
    public void Restore()
    {
        Console.SetOut(originalOut);
        Console.ForegroundColor = originalColor;
    }

    [TestMethod]
    public void DialogueColors_AreRedForDragonAndGreenForPlayer()
    {
        Assert.AreEqual(ConsoleColor.Red, Dialogue.DragonColor);
        Assert.AreEqual(ConsoleColor.Green, Dialogue.PlayerColor);
    }

    [TestMethod]
    public void Dragon_PrintsLineLabeledWithDragonName()
    {
        Dialogue.Dragon(new Messages(), "Fire Dragon", "Leave or perish!");

        Assert.AreEqual("Fire Dragon: \"Leave or perish!\"" + Environment.NewLine, output.ToString());
    }

    [TestMethod]
    public void Player_PrintsLineLabeledWithPlayerName()
    {
        Dialogue.Player(new Messages(), "Aria", "Missed me!");

        Assert.AreEqual("Aria: \"Missed me!\"" + Environment.NewLine, output.ToString());
    }

    [TestMethod]
    public void Dialogue_UsesTheLanguagesQuoteStyle()
    {
        Messages french = new Messages();
        french.SetCurrentLanguage(Languages.French);

        Dialogue.Dragon(french, "Dragon de Feu", "Aïe !");

        Assert.Contains("Dragon de Feu : « Aïe ! »", output.ToString());
    }

    [TestMethod]
    public void Say_PutsTheNarrationColorBack()
    {
        // Note: when output is redirected (as in tests), Windows may report the console's
        // default color instead of the one set here. Either way, the color after Say()
        // must match the color before it.
        Console.ForegroundColor = ConsoleColor.Cyan;
        ConsoleColor before = Console.ForegroundColor;

        Dialogue.Say(Dialogue.DragonColor, "Roar!");

        Assert.AreEqual(before, Console.ForegroundColor);
    }
}

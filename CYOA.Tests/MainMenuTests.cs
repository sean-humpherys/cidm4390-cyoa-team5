namespace CYOA.Tests;

// These tests type into the game by swapping Console input/output, so they must not
// run at the same time as each other (MSTestSettings.cs turns on parallel tests).
[TestClass]
[DoNotParallelize]
public sealed class MainMenuTests
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

    // Plays the game with the given lines typed in, and returns everything it printed.
    private static string Play(Game game, params string[] inputLines)
    {
        StringWriter output = new StringWriter();
        Console.SetIn(new StringReader(string.Join(Environment.NewLine, inputLines) + Environment.NewLine));
        Console.SetOut(output);

        game.StartGame();

        return output.ToString();
    }

    [TestMethod]
    public void English_Exit_ShowsWelcomeThenBye()
    {
        string output = Play(new Game(), "1", "2");

        Assert.Contains("Welcome to Choose Your Adventure Game", output);
        Assert.Contains("1. Create Character", output);
        Assert.EndsWith("Adventure ends. Bye." + Environment.NewLine, output);
    }

    [TestMethod]
    [DataRow("2", "Bienvenido al juego Elige tu propia aventura", "La aventura termina. Adiós.")]
    [DataRow("3", "Bienvenue dans le jeu Choisissez votre aventure", "L'aventure se termine. Au revoir.")]
    public void PickingALanguage_ShowsMenusInThatLanguage(string languageChoice, string welcome, string bye)
    {
        string output = Play(new Game(), languageChoice, "2");

        Assert.Contains(welcome, output);
        Assert.Contains(bye, output);
    }

    [TestMethod]
    public void InvalidLanguageChoice_RePromptsUntilValid()
    {
        string output = Play(new Game(), "9", "spanish", "1", "2");

        Assert.AreEqual(2, CountOf(output, "Invalid choice. Please try again."));
        Assert.Contains("Adventure ends. Bye.", output);
    }

    [TestMethod]
    public void InvalidMainMenuChoice_ShowsErrorAndMenuAgain()
    {
        string output = Play(new Game(), "1", "x", "2");

        Assert.Contains("Invalid choice. Please try again.", output);
        Assert.AreEqual(2, CountOf(output, "1. Create Character"));
    }

    [TestMethod]
    public void InputIsTrimmedAndCaseInsensitive()
    {
        string output = Play(new Game(), "  1  ", " 2 ");

        Assert.DoesNotContain("Invalid choice", output);
        Assert.Contains("Adventure ends. Bye.", output);
    }

    [TestMethod]
    public void CreateCharacter_NotBuiltYet_ReturnsToMainMenu()
    {
        // Update this test when the Class Creation PBI replaces the placeholder.
        string output = Play(new Game(), "1", "1", "2");

        Assert.Contains("still being built", output);
        Assert.Contains("Adventure ends. Bye.", output);
    }

    [TestMethod]
    public void Settings_ChangeLanguage_SwitchesTheRestOfTheGame()
    {
        // English -> Settings -> Change language -> French -> back -> Exit
        string output = Play(new Game(), "1", "3", "1", "3", "3", "2");

        Assert.Contains("Langue changée en français.", output);
        Assert.Contains("Paramètres", output); // settings menu shown again in French
        Assert.EndsWith("L'aventure se termine. Au revoir." + Environment.NewLine, output);
    }

    [TestMethod]
    [DataRow("1", ConsoleColor.Green)]
    [DataRow("2", ConsoleColor.White)]
    [DataRow("3", ConsoleColor.Cyan)]
    [DataRow("4", ConsoleColor.Yellow)]
    public void Settings_ChangeTextColor_UpdatesTextColor(string colorChoice, ConsoleColor expected)
    {
        Game game = new Game();

        string output = Play(game, "1", "3", "2", colorChoice, "3", "2");

        Assert.AreEqual(expected, game.TextColor);
        Assert.Contains("Text color changed.", output);
    }

    [TestMethod]
    public void DefaultTextColor_IsGreen()
    {
        Assert.AreEqual(ConsoleColor.Green, new Game().TextColor);
    }

    [TestMethod]
    public void InputEndingEarly_DoesNotHang()
    {
        // No input at all: the game should fall back to English and exit instead of looping forever.
        string output = Play(new Game());

        Assert.Contains("Adventure ends. Bye.", output);
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
}

// Dialogue.cs
// Prints spoken lines in the speaker's color, then puts the narration color back.
//   Dragon speech -> red      Player speech -> green      Narration -> the TextColor setting
//
// Usage:
//     Dialogue.Dragon(messages, dragon.Name, dragon.WasHitTaunt(messages));
//     // prints, in red:   Fire Dragon: "Ow! That actually stung a bit!"
public static class Dialogue
{
    public const ConsoleColor DragonColor = ConsoleColor.Red;
    public const ConsoleColor PlayerColor = ConsoleColor.Green;

    // A line the dragon says, labeled with its name (message 20 formats "Name: \"line\"").
    public static void Dragon(Messages messages, string dragonName, string line)
    {
        Say(DragonColor, messages.GetMessage(20, dragonName, line));
    }

    // A line the player's character says, labeled with the character's name.
    public static void Player(Messages messages, string playerName, string line)
    {
        Say(PlayerColor, messages.GetMessage(20, playerName, line));
    }

    // Writes one line in the given color, then always restores the color that was there before,
    // even if writing fails, so narration never gets stuck in red or green.
    public static void Say(ConsoleColor color, string text)
    {
        ConsoleColor narrationColor = Console.ForegroundColor;
        try
        {
            Console.ForegroundColor = color;
            Console.WriteLine(text);
        }
        finally
        {
            Console.ForegroundColor = narrationColor;
        }
    }
}

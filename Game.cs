using System.Text;

// Game (Class Diagram: class Game)
// Runs the whole game: language picker -> main menu -> character creation -> adventure -> combat -> end.
//
//   PBI Menu (John):             language picker, main menu, settings, exit
//   PBI Class Creation (Jonathan): CreateCharacter()
//   PBI Story (John):            StartAdventure(), MeetTheDragon(), StartCombat(), EndGame()
//   PBI Dragon (Ryan) / Combat (Sina): Dragon.cs, CombatSystem.cs
//
// TEAM RULES FOR THIS FILE
//   - Get every player-facing string from messages.GetMessage(id). Never hard-code text.
//   - Read player input with ReadInput(). It trims and lower-cases, and returns null if input ends.
//     On a real console it also clears the screen and shows the choice at the top ("> n").
//   - Print anything a character SAYS with Dialogue.Dragon(...) or Dialogue.Player(...).
//     They use red / green and put the narration color (TextColor) back afterwards.
public class Game
{
    // Class Diagram: Game "rolls" Die. Sequence Diagram: Game creates Die first.
    private readonly Die die;

    // Class Diagram: Game "Displays" Messages.
    private readonly Messages messages;

    // Class Diagram: Game "Creates" Dragon at startup.
    private readonly Dragon dragon;

    // Class Diagram: Game "Creates" Player (during character creation).
    private Player? player;

    // Settings: the narration color. Gray (the normal console color) by default, so the
    // player's green dialogue and the dragon's red dialogue stand out from it.
    public ConsoleColor TextColor { get; private set; } = ConsoleColor.Gray;

    public Game() : this(new Die())
    {
    }

    // Lets the unit tests pass in a Die with known rolls.
    public Game(Die die)
    {
        ArgumentNullException.ThrowIfNull(die);

        // Start Game Activity Diagram: "Instantiate Die object", then "Instantiate Dragon object".
        this.die = die;
        dragon = new Dragon("Fire Dragon", die);

        messages = new Messages();
    }

    // Class Diagram: + void StartGame()
    public void StartGame()
    {
        try
        {
            Console.OutputEncoding = Encoding.UTF8; // so é, ñ, ç display correctly
        }
        catch (IOException)
        {
            // Some terminals don't allow changing the encoding. The game still works.
        }

        ApplyTextColor();
        // ClearScreen(); // SCREEN CLEAR (disabled): start the game on a blank screen
        ChooseLanguage();
        Console.WriteLine(messages.GetMessage(3)); // Welcome to Choose Your Adventure Game
        ShowMainMenu();
        Console.ResetColor(); // leave the user's terminal the way we found it
    }

    // Class Diagram: + void EndGame()
    // End Game Activity Diagram: one message for each way the game can end.
    public void EndGame(EndCondition condition)
    {
        int messageId = condition switch
        {
            EndCondition.Retreat => 400,        // You quickly retreat... The End.
            EndCondition.DragonDefeated => 401, // With the dragon vanquished... The End.
            EndCondition.PlayerDefeated => 402, // The dragon laughs as your eyesight fades... The End.
            _ => 5                              // Adventure ends. Bye.
        };

        Console.WriteLine(messages.GetMessage(messageId));
    }

    // ------------------------------------------------------------------------
    // PBI Menu (John): language picker, main menu, settings
    // ------------------------------------------------------------------------

    // Shown once at launch, before anything else, so every later message is in the chosen language.
    private void ChooseLanguage()
    {
        while (true)
        {
            Console.WriteLine(messages.GetMessage(1)); // trilingual prompt + 1/2/3
            string? choice = ReadInput();

            if (choice == null)
            {
                return; // input ended: keep the default language (English)
            }

            Languages? language = ParseLanguageChoice(choice);
            if (language != null)
            {
                messages.SetCurrentLanguage(language.Value);
                return;
            }

            Console.WriteLine(messages.GetMessage(2)); // Invalid choice
        }
    }

    // Start Game Activity Diagram: "1. Create Character or 2. Exit".
    // Settings was added as option 3 so the stakeholder's 1 and 2 keep their meaning.
    private void ShowMainMenu()
    {
        bool running = true;
        while (running)
        {
            Console.WriteLine(messages.GetMessage(4)); // 1. Create Character 2. Exit 3. Settings
            string? choice = ReadInput();

            switch (choice)
            {
                case "1":
                    if (CreateCharacter())
                    {
                        StartAdventure();
                        running = false; // every adventure path finishes at End Game
                    }
                    break;

                case "2":
                case null: // input ended (e.g. Ctrl+Z / Ctrl+D): treat as Exit
                    Console.WriteLine(messages.GetMessage(5)); // Adventure ends. Bye.
                    running = false;
                    break;

                case "3":
                    ShowSettings();
                    break;

                default:
                    Console.WriteLine(messages.GetMessage(2)); // Invalid choice
                    break;
            }
        }
    }

    private void ShowSettings()
    {
        while (true)
        {
            Console.WriteLine(messages.GetMessage(10)); // 1. Language 2. Text color 3. Back
            string? choice = ReadInput();

            switch (choice)
            {
                case "1":
                    ChangeLanguage();
                    break;

                case "2":
                    ChangeTextColor();
                    break;

                case "3":
                case null:
                    return;

                default:
                    Console.WriteLine(messages.GetMessage(2)); // Invalid choice
                    break;
            }
        }
    }

    private void ChangeLanguage()
    {
        while (true)
        {
            Console.WriteLine(messages.GetMessage(1));
            string? choice = ReadInput();
            if (choice == null)
            {
                return;
            }

            Languages? language = ParseLanguageChoice(choice);
            if (language != null)
            {
                messages.SetCurrentLanguage(language.Value);
                Console.WriteLine(messages.GetMessage(11)); // "Language changed to ..." in the new language
                return;
            }

            Console.WriteLine(messages.GetMessage(2));
        }
    }

    private void ChangeTextColor()
    {
        while (true)
        {
            Console.WriteLine(messages.GetMessage(12)); // 1. Gray 2. White 3. Cyan 4. Yellow
            // Green and red are left out on purpose: they are the player's and dragon's dialogue colors.
            string? choice = ReadInput();

            if (choice == null)
            {
                return;
            }

            ConsoleColor? color = choice switch
            {
                "1" => ConsoleColor.Gray,
                "2" => ConsoleColor.White,
                "3" => ConsoleColor.Cyan,
                "4" => ConsoleColor.Yellow,
                _ => null
            };

            if (color != null)
            {
                TextColor = color.Value;
                ApplyTextColor();
                Console.WriteLine(messages.GetMessage(13)); // Text color changed.
                return;
            }

            Console.WriteLine(messages.GetMessage(2));
        }
    }

    // TODO PBI Save/Load (optional, part of the Menu PBI): message ids 90-99 are reserved.
    // Idea: save the chosen language and TextColor (and later the Player) to a small file,
    // and add "Load game" to the main menu.

    // ------------------------------------------------------------------------
    // PBI Class Creation (Jonathan): Create Character Activity Diagram
    // ------------------------------------------------------------------------
    // Returns true when a character was created and the player chose to go on an adventure.
    private bool CreateCharacter()
    {
        // Select race
        Race race;

        while (true)
        {
            Console.WriteLine(messages.GetMessage(100)); // 1. Human 2. Elf 3. Dwarf 4. Halfling

            string? choice = ReadInput();

            if (choice == null)
                return false;

            switch (choice)
            {
                case "1":
                    race = Race.Human;
                    break;
                case "2":
                    race = Race.Elf;
                    break;
                case "3":
                    race = Race.Dwarf;
                    break;
                case "4":
                    race = Race.Halfling;
                    break;
                default:
                    Console.WriteLine(messages.GetMessage(2));
                    continue;
            }

            break;
        }

        // Enter player name (keeps the player's capital letters)
        string name;

        while (true)
        {
            Console.WriteLine(messages.GetMessage(101));

            string? input = ReadInputKeepCase();

            if (input == null)
                return false;

            if (string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine(messages.GetMessage(2));
                continue;
            }

            name = input;
            break;
        }

        // Select occupation
        Occupation occupation;

        while (true)
        {
            Console.WriteLine(messages.GetMessage(102)); // 1. Fighter 2. Magician 3. Thief 4. Archer

            string? choice = ReadInput();

            if (choice == null)
                return false;

            switch (choice)
            {
                case "1":
                    occupation = Occupation.Fighter;
                    break;
                case "2":
                    occupation = Occupation.Magician;
                    break;
                case "3":
                    occupation = Occupation.Thief;
                    break;
                case "4":
                    occupation = Occupation.Archer;
                    break;
                default:
                    Console.WriteLine(messages.GetMessage(2));
                    continue;
            }

            break;
        }

        // Create the player using the existing Player class
        player = new Player(name, race, occupation, die);

        // Roll Strength
        if (!WaitForRoll(103))
            return false;

        player.RollStrength(die);
        ShowRoll(1);

        // Roll Agility
        if (!WaitForRoll(104))
            return false;

        player.RollAgility(die);
        ShowRoll(2);

        // Roll Health
        if (!WaitForRoll(105))
            return false;

        player.RollHealth(die);
        ShowRoll(3);

        // Display character statistics or continue
        while (true)
        {
            Console.WriteLine(messages.GetMessage(107));

            string? choice = ReadInput();

            switch (choice)
            {
                case "1":
                    Console.WriteLine(player.DisplayStats(messages));
                    break;

                case "2":
                    return true;

                case null:
                    return false;

                default:
                    Console.WriteLine(messages.GetMessage(2));
                    break;
            }
        }
    }

    // Keeps asking until the player types "roll". Returns false if input ends.
    private bool WaitForRoll(int messageId)
    {
        while (true)
        {
            Console.WriteLine(messages.GetMessage(messageId));

            string? input = ReadInput();

            if (input == null)
                return false;

            if (input == "roll")
                return true;

            Console.WriteLine(messages.GetMessage(106)); // Please type 'roll'.
        }
    }

    // Shows the roll that was just made, the racial modifier, and the stats rolled so far:
    //
    //      +-----+
    //      | 14  |  d20
    //      +-----+
    //   You rolled a 14 on the 20-sided die.
    //   Elf racial modifier: +3
    //
    //   Your stats so far:
    //     Strength: 17
    private void ShowRoll(int statsRolled)
    {
        Player p = player!;

        Console.WriteLine("   +-----+");
        Console.WriteLine($"   | {p.LastDieRoll,2}  |  d20");
        Console.WriteLine("   +-----+");
        Console.WriteLine(messages.GetMessage(108, p.LastDieRoll)); // You rolled a {0}

        if (p.LastModifier != 0)
        {
            string modifier = p.LastModifier.ToString("+0;-0"); // always show the sign: +3 or -2
            Console.WriteLine(messages.GetMessage(109, messages.GetRaceName(p.Race), modifier));
        }
        else
        {
            Console.WriteLine(messages.GetMessage(118)); // No racial modifier.
        }

        Console.WriteLine();
        Console.WriteLine(messages.GetMessage(119)); // Your stats so far:
        Console.WriteLine("  " + messages.GetMessage(133, p.Strength));
        if (statsRolled >= 2)
        {
            Console.WriteLine("  " + messages.GetMessage(134, p.Agility));
        }
        if (statsRolled >= 3)
        {
            Console.WriteLine("  " + messages.GetMessage(135, p.Health));
        }
        Console.WriteLine();
    }

    // ------------------------------------------------------------------------
    // PBI Story (John): rest of the Start Game Activity Diagram
    // ------------------------------------------------------------------------

    // The adventure menu: North, South, or Exit. Loops until the game ends.
    private void StartAdventure()
    {
        while (true)
        {
            Console.WriteLine(messages.GetMessage(200)); // walking in the mountains... n / s / e
            string? choice = ReadInput();

            switch (choice)
            {
                case "s":
                    Console.WriteLine(messages.GetMessage(201)); // South: bored, return home
                    break; // back to the adventure menu

                case "n":
                    if (MeetTheDragon())
                    {
                        return; // the game ended
                    }
                    break; // retreated mid-combat: back to the adventure menu

                case "e":
                case null:
                    EndGame(EndCondition.Quit);
                    return;

                default:
                    Console.WriteLine(messages.GetMessage(2));
                    break;
            }
        }
    }

    // North path: the dragon encounter. Returns true when the game has ended.
    private bool MeetTheDragon()
    {
        Console.WriteLine(messages.GetMessage(202)); // You take the North path... confronted with a dragon

        dragon.Name = messages.GetMessage(264); // "Fire Dragon" in the current language

        // Appendix B: the dragon's line in red, then back to the narration color
        Dialogue.Say(Dialogue.DragonColor, messages.GetMessage(203, dragon.Name)); // I am {name}. Leave or perish!

        Console.WriteLine(messages.GetMessage(204)); // Your magic ring gives you the stats on the dragon.
        Console.WriteLine(dragon.DisplayStats(messages));

        while (true)
        {
            Console.WriteLine(messages.GetMessage(205)); // retreat (r) or attack (a)?
            string? choice = ReadInput();

            switch (choice)
            {
                case "r":
                    EndGame(EndCondition.Retreat);
                    return true;

                case "a":
                    return StartCombat();

                case null:
                    EndGame(EndCondition.Quit);
                    return true;

                default:
                    Console.WriteLine(messages.GetMessage(2));
                    break;
            }
        }
    }

    // Attack Activity Diagram: rounds of player attack + dragon attack, then retreat or attack again.
    // Returns true when the game has ended, false when the player retreated back to the adventure menu.
    private bool StartCombat()
    {
        // Sequence Diagram: new Combat(player, dragon)
        CombatSystem combat = new CombatSystem(player!, dragon, die, messages);

        while (true)
        {
            combat.PlayerAttacksDragonSequence();
            if (!dragon.IsAlive())
            {
                EndGame(EndCondition.DragonDefeated);
                return true;
            }

            combat.DragonAttacksPlayerSequence();
            if (!player!.IsAlive())
            {
                EndGame(EndCondition.PlayerDefeated);
                return true;
            }

            // Ask until the player gives a valid answer
            string? choice;
            while (true)
            {
                Console.WriteLine(messages.GetMessage(312)); // r = retreat, a = attack
                choice = ReadInput();
                if (choice is "a" or "r" or null)
                {
                    break;
                }
                Console.WriteLine(messages.GetMessage(2));
            }

            if (choice == "r")
            {
                // Appendix D: retreating mid-fight goes back to the adventure menu, not End Game.
                Console.WriteLine(messages.GetMessage(313)); // You hastily retreat licking your wounds.
                return false;
            }

            if (choice == null)
            {
                EndGame(EndCondition.Quit);
                return true;
            }
            // "a": next round
        }
    }

    // ------------------------------------------------------------------------
    // Helpers shared by everyone
    // ------------------------------------------------------------------------

    // Reads one line of player input: trimmed and lower-case, so "N", " n " and "n" all match "n".
    // Returns null when input has ended. Treat that as "quit" so the game never loops forever.
    private static string? ReadInput()
    {
        return ReadInputKeepCase()?.ToLowerInvariant();
    }

    // Same as ReadInput() but keeps capital letters (used for the character's name).
    // (Screen clearing after each choice is currently disabled; see "SCREEN CLEAR (disabled)".)
    private static string? ReadInputKeepCase()
    {
        string? input = Console.ReadLine()?.Trim();

        // SCREEN CLEAR (disabled): clear the screen after each choice and show the choice at the top
        // if (input != null && ClearScreen())
        // {
        //     Console.WriteLine($"> {input}");
        //     Console.WriteLine();
        // }

        return input;
    }

    // SCREEN CLEAR (disabled): to turn screen clearing back on, uncomment this method
    // and the two other spots marked "SCREEN CLEAR (disabled)" in this file (lines 64 and 593-597).
    // Clears the console window. Returns false (and does nothing) when output isn't a real
    // screen, e.g. in unit tests, where Console.Clear() would throw an error.
    // private static bool ClearScreen()
    // {
    //     if (Console.IsOutputRedirected)
    //     {
    //         return false;
    //     }
    //
    //     try
    //     {
    //         Console.Clear();
    //         return true;
    //     }
    //     catch (IOException)
    //     {
    //         return false;
    //     }
    // }

    private void ApplyTextColor()
    {
        Console.ForegroundColor = TextColor;
    }

    private static Languages? ParseLanguageChoice(string choice)
    {
        return choice switch
        {
            "1" => Languages.English,
            "2" => Languages.Spanish,
            "3" => Languages.French,
            _ => null
        };
    }
}

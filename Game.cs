using System.Text;

// Game (Class Diagram: class Game)
// Runs the whole game: language picker -> main menu -> character creation -> adventure -> combat -> end.
//
// STATUS
//   DONE  (PBI Menu - John):  language picker, main menu, settings (language + text color), exit.
//   TODO  (other PBIs):       look for "TODO PBI" below. Each stub says which diagram to follow
//                              and which message ids in LanguageFiles/*.txt are already written for it.
//
// TEAM RULES FOR THIS FILE
//   - Get every player-facing string from messages.GetMessage(id). Never hard-code text.
//   - Read player input with ReadInput(). It trims and lower-cases, and returns null if input ends.
//   - After printing something in another color (e.g. the red dragon line), restore TextColor.
public class Game
{
    // Class Diagram: Game "rolls" Die. Sequence Diagram: Game creates Die first.
    private readonly Die die;

    // Class Diagram: Game "Displays" Messages.
    private readonly Messages messages;

    // TODO PBI Dragon (Ryan): Class Diagram says Game "Creates" Dragon at startup.
    // private Dragon dragon;

    // TODO PBI Class Creation (Jonathan): Class Diagram says Game "Creates" Player.
    // private Player? player;

    // Settings: the normal text color. Stakeholder (Appendix B) wants green by default.
    public ConsoleColor TextColor { get; private set; } = ConsoleColor.Green;

    public Game()
    {
        // Start Game Activity Diagram: "Instantiate Die object", then "Instantiate Dragon object".
        die = new Die();

        // TODO PBI Dragon (Ryan): create the dragon here, before the menu shows.
        // Sequence Diagram: new Dragon(name) rolls strength, agility and health with die.Roll(20)
        // and gets new Weapon("claws", 12, "##").
        // dragon = new Dragon("<dragon name>");

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
        ChooseLanguage();
        Console.WriteLine(messages.GetMessage(3)); // Welcome to Choose Your Adventure Game
        ShowMainMenu();
        Console.ResetColor(); // leave the user's terminal the way we found it
    }

    // Class Diagram: + void EndGame()
    // TODO PBI Combat / Story: End Game Activity Diagram has three endings.
    // EndGame needs to know which one happened. Suggested approach: add an enum and a parameter,
    // e.g. EndGame(EndCondition condition) with Retreat, DragonDefeated, PlayerDefeated, then:
    //     Retreat         -> Console.WriteLine(messages.GetMessage(400));
    //     DragonDefeated  -> Console.WriteLine(messages.GetMessage(401));
    //     PlayerDefeated  -> Console.WriteLine(messages.GetMessage(402));
    // Note: 'e' on the adventure menu also points to End Game, but none of the three messages fits.
    // Ask the professor (stakeholder) what to show there; until then, message 5 ("Adventure ends. Bye.") works.
    public void EndGame()
    {
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
            Console.WriteLine(messages.GetMessage(12)); // 1. Green 2. White 3. Cyan 4. Yellow
            string? choice = ReadInput();

            ConsoleColor? color = choice switch
            {
                "1" => ConsoleColor.Green,
                "2" => ConsoleColor.White,
                "3" => ConsoleColor.Cyan,
                "4" => ConsoleColor.Yellow,
                _ => null
            };

            if (choice == null)
            {
                return;
            }

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
    // TODO PBI Class Creation (Jonathan): Create Character Activity Diagram
    // ------------------------------------------------------------------------
    // Returns true when a character was created and the adventure should start.
    // Messages already written: 100-107 prompts, 110-113 race names, 114-117 occupation names,
    // 120-124 weapon names, 130-137 stat labels for DisplayStats(), 140-143 player taunts.
    //
    // Suggested flow:
    //   1. Ask race (100), name (101), occupation (102). Re-prompt on bad input with message 2.
    //      Tip: numbered choices (1-4) work the same in all three languages; typed words don't.
    //   2. player = new Player(name, race, occupation);  // sets weapon from occupation:
    //         Fighter  -> "long sword", 12      Magician -> "lightning bolt spell", 12
    //         Thief    -> "dagger", 6           Archer   -> "long bow", 8
    //   3. Strength: show 103, wait for "roll", die.Roll(20).
    //         Human/Elf/Dwarf: + die.Roll(4)      Halfling: - die.Roll(4)
    //   4. Agility: show 104, wait for "roll", die.Roll(20).  Halfling/Elf: + die.Roll(4)
    //   5. Health: show 105, wait for "roll", die.Roll(20) + the strength modifier from step 3.
    //         Open question: the diagram skips the modifier for Halflings, but the dialog says to add it.
    //   6. Loop on 107: "1" -> Console.WriteLine(player.DisplayStats()); "2" -> return true.
    private bool CreateCharacter()
    {
        Console.WriteLine(messages.GetMessage(6)); // "still being built" - delete when implemented
        return false;
    }

    // ------------------------------------------------------------------------
    // TODO PBI Story (John) + PBI Dragon (Ryan): rest of Start Game Activity Diagram
    // ------------------------------------------------------------------------
    // Messages already written: 200-205.
    //
    // Suggested flow (loop until the game ends):
    //   Show 200 (n / s / e).
    //     "s" -> show 201, loop back to 200.
    //     "e" -> EndGame(...) and return.
    //     "n" -> show 202
    //            Console.ForegroundColor = ConsoleColor.Red;
    //            show GetMessage(203, dragon.Name)
    //            ApplyTextColor();                        // back to the player's color
    //            show 204, then Console.WriteLine(dragon.DisplayStats());
    //            show 205 (r / a):
    //               "r" -> EndGame(Retreat) and return
    //               "a" -> StartCombat() (below)
    private void StartAdventure()
    {
    }

    // ------------------------------------------------------------------------
    // TODO PBI Combat (Sina): Attack Activity Diagram
    // ------------------------------------------------------------------------
    // Messages already written: 300-313 combat, 250-257 dragon taunts, 260-263 dragon replies.
    //
    // Sequence Diagram: Combat combat = new Combat(player, dragon);
    // Each round:
    //   combat.PlayerAttacksDragonSequence();   // show 300 + weapon ASCII art, roll vs strength/agility
    //   if dragon health <= 0 -> show 305, EndGame(DragonDefeated), return
    //   combat.DragonAttacksPlayerSequence();
    //   if player health <= 0 -> show 311, EndGame(PlayerDefeated), return
    //   show 312 (r / a):
    //      "a" -> next round
    //      "r" -> show 313, then go back to the adventure menu (message 200), NOT End Game
    //             (dialog Appendix D; the diagram says "main menu" but means the n/s/e menu).
    // Combat will need `die` and `messages`. Pass them into the Combat constructor or as parameters.
    // private void StartCombat()
    // {
    // }

    // ------------------------------------------------------------------------
    // Helpers shared by everyone
    // ------------------------------------------------------------------------

    // Reads one line of player input: trimmed and lower-case, so "N", " n " and "n" all match "n".
    // Returns null when input has ended. Treat that as "quit" so the game never loops forever.
    private static string? ReadInput()
    {
        return Console.ReadLine()?.Trim().ToLowerInvariant();
    }

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

// Messages (Class Diagram: class Messages)
// Every piece of text the player sees comes from here, so the game can be played
// in any language in the Languages enum. Never hard-code player-facing text in C#.
//
// Usage:
//     Messages messages = new Messages();                   // loads English by default
//     Console.WriteLine(messages.GetMessage(3));             // "Welcome to ..."
//     Console.WriteLine(messages.GetMessage(203, "Fafnir")); // fills in {0}
//
// Message ids and their ranges per PBI are listed at the top of LanguageFiles/English.txt.
public class Messages
{
    // Class Diagram: - string currentLanguage {get; set}
    public string CurrentLanguage { get; private set; } = string.Empty;

    // Class Diagram: - Dictionary languageDictionary
    private readonly Dictionary<int, string> languageDictionary = new Dictionary<int, string>();

    public Messages()
    {
        SetCurrentLanguage(Languages.English);
    }

    // Sequence Diagram: setCurrentLanguage("English") -> readDictionary()
    public void SetCurrentLanguage(Languages language)
    {
        ReadDictionary(GetLanguageFilePath(language));
        CurrentLanguage = language.ToString();
    }

    // The language files are copied next to the .exe at build time (see the .csproj).
    public static string GetLanguageFilePath(Languages language)
    {
        return Path.Combine(AppContext.BaseDirectory, "LanguageFiles", $"{language}.txt");
    }

    // Class Diagram: + void ReadDictionary(languageFilePath)
    // Loads every "id=text" line of a language file into the dictionary.
    public void ReadDictionary(string languageFilePath)
    {
        if (!File.Exists(languageFilePath))
        {
            throw new FileNotFoundException($"Language file not found: {languageFilePath}", languageFilePath);
        }

        Dictionary<int, string> loaded = new Dictionary<int, string>();
        string[] lines = File.ReadAllLines(languageFilePath);

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];
            if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith('#'))
            {
                continue;
            }

            int separator = line.IndexOf('=');
            if (separator < 1 || !int.TryParse(line[..separator].Trim(), out int id))
            {
                throw new InvalidDataException($"{languageFilePath} line {i + 1}: expected 'id=text' but found '{line}'");
            }

            if (loaded.ContainsKey(id))
            {
                throw new InvalidDataException($"{languageFilePath} line {i + 1}: message id {id} is used twice");
            }

            loaded[id] = line[(separator + 1)..].Replace("\\n", Environment.NewLine);
        }

        // Only replace the current dictionary once the whole file loaded successfully.
        languageDictionary.Clear();
        foreach (KeyValuePair<int, string> entry in loaded)
        {
            languageDictionary[entry.Key] = entry.Value;
        }
    }

    // Class Diagram: + string GetMessage(int whichMessage)
    // A missing id returns a visible marker instead of crashing, so gaps are easy to spot while playing.
    public string GetMessage(int whichMessage)
    {
        if (languageDictionary.TryGetValue(whichMessage, out string? message))
        {
            return message;
        }

        return $"[missing message {whichMessage} in {CurrentLanguage}]";
    }

    // Same as GetMessage(int), but fills in {0}, {1}, ... placeholders.
    // Example: GetMessage(304, damage, dragon.Health)
    public string GetMessage(int whichMessage, params object[] args)
    {
        return string.Format(GetMessage(whichMessage), args);
    }

    // All message ids currently loaded (used by the unit tests to keep translations in sync).
    public IReadOnlyCollection<int> GetMessageIds()
    {
        return languageDictionary.Keys;
    }
}

// Languages the player can choose (Class Diagram: enum Languages).
// Each value must have a matching file in LanguageFiles/, e.g. English -> LanguageFiles/English.txt.
// To add a language: add a value here, add LanguageFiles/<Value>.txt with every message id,
// and add it to the language picker in Game.ChooseLanguage() and message 1.
public enum Languages
{
    English,
    Spanish,
    French
}

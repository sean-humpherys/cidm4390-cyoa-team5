namespace CYOA.Tests;

[TestClass]
public sealed class MessagesTests
{
    [TestMethod]
    public void NewMessages_LoadsEnglishByDefault()
    {
        Messages messages = new Messages();

        Assert.AreEqual("English", messages.CurrentLanguage);
        Assert.AreEqual("Adventure ends. Bye.", messages.GetMessage(5));
    }

    [TestMethod]
    [DataRow(Languages.English, "Adventure ends. Bye.")]
    [DataRow(Languages.Spanish, "La aventura termina. Adiós.")]
    [DataRow(Languages.French, "L'aventure se termine. Au revoir.")]
    public void SetCurrentLanguage_SwitchesMessageText(Languages language, string expected)
    {
        Messages messages = new Messages();

        messages.SetCurrentLanguage(language);

        Assert.AreEqual(language.ToString(), messages.CurrentLanguage);
        Assert.AreEqual(expected, messages.GetMessage(5));
    }

    [TestMethod]
    public void GetMessage_WithArguments_FillsPlaceholders()
    {
        Messages messages = new Messages();

        string result = messages.GetMessage(203, "Fafnir");

        Assert.AreEqual("The dragon says 'I am Fafnir. Leave or perish!'", result);
    }

    [TestMethod]
    public void GetMessage_UnknownId_ReturnsMissingMarker()
    {
        Messages messages = new Messages();

        string result = messages.GetMessage(99999);

        Assert.AreEqual("[missing message 99999 in English]", result);
    }

    [TestMethod]
    public void GetMessage_BackslashN_BecomesNewLine()
    {
        Messages messages = new Messages();

        string languagePrompt = messages.GetMessage(1);

        Assert.Contains(Environment.NewLine + "1. English", languagePrompt);
    }

    // Keeps the translations in sync: if someone adds a message to one language file
    // but forgets the others, this test fails and names the missing ids.
    [TestMethod]
    [DataRow(Languages.Spanish)]
    [DataRow(Languages.French)]
    public void EveryLanguage_HasTheSameMessageIdsAsEnglish(Languages language)
    {
        Messages english = new Messages();
        Messages other = new Messages();
        other.SetCurrentLanguage(language);

        HashSet<int> englishIds = english.GetMessageIds().ToHashSet();
        HashSet<int> otherIds = other.GetMessageIds().ToHashSet();

        string missing = string.Join(", ", englishIds.Except(otherIds).Order());
        string extra = string.Join(", ", otherIds.Except(englishIds).Order());

        Assert.AreEqual(string.Empty, missing, $"{language}.txt is missing ids: {missing}");
        Assert.AreEqual(string.Empty, extra, $"{language}.txt has ids that English.txt doesn't: {extra}");
    }

    [TestMethod]
    public void EveryLanguageFile_ExistsInTheBuildOutput()
    {
        foreach (Languages language in Enum.GetValues<Languages>())
        {
            Assert.IsTrue(File.Exists(Messages.GetLanguageFilePath(language)), $"Missing file for {language}");
        }
    }

    [TestMethod]
    public void ReadDictionary_IgnoresCommentsAndBlankLines_AndKeepsEqualsSignsInText()
    {
        string path = Path.GetTempFileName();
        try
        {
            File.WriteAllLines(path, ["# a comment", "", "1=Hello", "2=Sword: -)=====>"]);
            Messages messages = new Messages();

            messages.ReadDictionary(path);

            Assert.AreEqual("Hello", messages.GetMessage(1));
            Assert.AreEqual("Sword: -)=====>", messages.GetMessage(2));
            Assert.HasCount(2, messages.GetMessageIds());
        }
        finally
        {
            File.Delete(path);
        }
    }

    [TestMethod]
    public void ReadDictionary_DuplicateId_Throws()
    {
        string path = Path.GetTempFileName();
        try
        {
            File.WriteAllLines(path, ["1=First", "1=Second"]);
            Messages messages = new Messages();

            Assert.ThrowsExactly<InvalidDataException>(() => messages.ReadDictionary(path));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [TestMethod]
    public void ReadDictionary_MissingFile_Throws()
    {
        Messages messages = new Messages();

        Assert.ThrowsExactly<FileNotFoundException>(() => messages.ReadDictionary("does-not-exist.txt"));
    }
}

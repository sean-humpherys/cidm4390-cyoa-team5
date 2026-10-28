namespace CYOA.Tests;

// Tests for CombatSystem, following the Attack Activity Diagram:
//   hit      = d20 <= attacker's Strength
//   defend   = d20 <= defender's Agility
//   damage   = roll up to the weapon's damage
// ScriptedDie controls every roll, so each test knows exactly what should happen.
// Combat prints to the console, so these tests capture Console output (and can't run in parallel).
[TestClass]
[DoNotParallelize]
public sealed class CombatSystemTests
{
    private TextWriter originalOut = null!;
    private StringWriter output = null!;
    private Messages messages = null!;
    private ScriptedDie dice = null!;

    [TestInitialize]
    public void Setup()
    {
        originalOut = Console.Out;
        output = new StringWriter();
        Console.SetOut(output);
        messages = new Messages();
        dice = new ScriptedDie();
    }

    [TestCleanup]
    public void RestoreConsole()
    {
        Console.SetOut(originalOut);
    }

    // A Human player with exact stats.
    // Human rolls: d4 then d20 for Strength, d20 for Agility, d20 + the d4 for Health.
    private Player CreatePlayer(int strength, int agility, int health, Occupation occupation = Occupation.Fighter)
    {
        dice.Add(1, strength - 1, agility, health - 1);
        Player player = new Player("Aria", Race.Human, occupation, dice);
        player.RollStrength(dice);
        player.RollAgility(dice);
        player.RollHealth(dice);
        return player;
    }

    // A dragon with exact stats (the constructor rolls Strength, Agility, Health in that order).
    private Dragon CreateDragon(int strength, int agility, int health)
    {
        dice.Add(strength, agility, health);
        return new Dragon("Fafnir", dice);
    }

    private CombatSystem CreateCombat(Player player, Dragon dragon)
    {
        return new CombatSystem(player, dragon, dice, messages);
    }

    // ---------- Player attacks dragon ----------

    [TestMethod]
    public void PlayerAttack_HitAndDragonFailsDefense_DamagesDragon()
    {
        CombatSystem combat = CreateCombat(CreatePlayer(10, 10, 20), CreateDragon(10, 5, 20));
        dice.Add(5, 15, 7, 1); // attack 5 <= 10 hits, defense 15 > 5 fails, 7 damage, taunt #1

        combat.PlayerAttacksDragonSequence();

        Assert.AreEqual(13, combat.Dragon.Health);
        Assert.Contains("You hit Fafnir.", output.ToString());
        Assert.Contains("Your weapon delivers 7 damage points. The dragon's health points is now 13.", output.ToString());
        Assert.Contains("Fafnir: \"Ow! That actually stung a bit!\"", output.ToString()); // dragon speaks, labeled
        Assert.AreEqual(0, dice.RollsLeft);
    }

    [TestMethod]
    public void PlayerAttack_RollAboveStrength_Misses()
    {
        CombatSystem combat = CreateCombat(CreatePlayer(10, 10, 20), CreateDragon(10, 5, 20));
        dice.Add(11); // 11 > 10 misses

        combat.PlayerAttacksDragonSequence();

        Assert.AreEqual(20, combat.Dragon.Health);
        Assert.Contains("You missed Fafnir.", output.ToString());
    }

    [TestMethod]
    public void PlayerAttack_RollEqualToStrength_Hits()
    {
        CombatSystem combat = CreateCombat(CreatePlayer(10, 10, 20), CreateDragon(10, 5, 20));
        dice.Add(10, 20, 1, 1); // 10 <= 10 hits (boundary), defense fails, 1 damage, taunt

        combat.PlayerAttacksDragonSequence();

        Assert.AreEqual(19, combat.Dragon.Health);
    }

    [TestMethod]
    public void PlayerAttack_DragonDefends_NoDamageAndDragonTaunts()
    {
        CombatSystem combat = CreateCombat(CreatePlayer(10, 10, 20), CreateDragon(10, 5, 20));
        dice.Add(1, 5, 3); // hit, defense 5 <= 5 succeeds, taunt #3 uses the race

        combat.PlayerAttacksDragonSequence();

        Assert.AreEqual(20, combat.Dragon.Health);
        Assert.Contains("Fafnir successfully defends your attack.", output.ToString());
        Assert.Contains("Fafnir: \"Try harder, Human! You're making me yawn!\"", output.ToString());
    }

    [TestMethod]
    public void PlayerAttack_FinalBlow_DefeatsDragonAtZeroHealth()
    {
        CombatSystem combat = CreateCombat(CreatePlayer(10, 10, 20), CreateDragon(10, 5, 3));
        dice.Add(1, 20, 12); // hit, defense fails, 12 damage vs 3 health. No taunt roll after death.

        combat.PlayerAttacksDragonSequence();

        Assert.AreEqual(0, combat.Dragon.Health);
        Assert.IsTrue(combat.IsOver);
        Assert.Contains("With the last words of 'Arggg!', you have defeated the dragon.", output.ToString());
    }

    [TestMethod]
    [DataRow(Occupation.Fighter, "-)=====>")]
    [DataRow(Occupation.Magician, "zap~~~~~~")]
    [DataRow(Occupation.Thief, "-)==>")]
    [DataRow(Occupation.Archer, "}     -->")]
    public void PlayerAttack_ShowsTheWeaponAsciiArt(Occupation occupation, string art)
    {
        CombatSystem combat = CreateCombat(CreatePlayer(10, 10, 20, occupation), CreateDragon(10, 5, 20));
        dice.Add(20); // miss

        combat.PlayerAttacksDragonSequence();

        Assert.Contains(art, output.ToString());
    }

    [TestMethod]
    public void PlayerAttack_IntroNamesOccupationAndWeapon()
    {
        CombatSystem combat = CreateCombat(CreatePlayer(10, 10, 20, Occupation.Thief), CreateDragon(10, 5, 20));
        dice.Add(20);

        combat.PlayerAttacksDragonSequence();

        Assert.Contains("As a Thief you withdraw your dagger. You attack.", output.ToString());
    }

    // ---------- Dragon attacks player ----------

    [TestMethod]
    public void DragonAttack_HitAndPlayerFailsDefense_DamagesPlayer()
    {
        CombatSystem combat = CreateCombat(CreatePlayer(10, 5, 20), CreateDragon(10, 5, 20));
        dice.Add(5, 15, 8, 2); // hit, defense 15 > 5 fails, 8 damage, sarcastic reply #2

        combat.DragonAttacksPlayerSequence();

        Assert.AreEqual(12, combat.Player.Health);
        Assert.Contains("Dragon's turn to attack.", output.ToString());
        Assert.Contains("Fafnir hits you.", output.ToString());
        Assert.Contains("The dragon's attack delivers 8 damage points. Your health points is now 12.", output.ToString());
        Assert.Contains("Fafnir: \"You're still standing?", output.ToString());
    }

    [TestMethod]
    public void DragonAttack_RollAboveStrength_Misses()
    {
        CombatSystem combat = CreateCombat(CreatePlayer(10, 5, 20), CreateDragon(10, 5, 20));
        dice.Add(11);

        combat.DragonAttacksPlayerSequence();

        Assert.AreEqual(20, combat.Player.Health);
        Assert.Contains("The dragon missed you.", output.ToString());
    }

    [TestMethod]
    public void DragonAttack_PlayerDefends_NoDamageAndPlayerTaunts()
    {
        CombatSystem combat = CreateCombat(CreatePlayer(10, 5, 20), CreateDragon(10, 5, 20));
        dice.Add(1, 5, 1); // hit, defense 5 <= 5 succeeds, player taunt #1

        combat.DragonAttacksPlayerSequence();

        Assert.AreEqual(20, combat.Player.Health);
        Assert.Contains("You successfully defend the attack.", output.ToString());
        Assert.Contains("Aria: \"Ha! Too slow, you overgrown lizard!\"", output.ToString()); // player speaks, labeled
    }

    [TestMethod]
    public void DragonAttack_FinalBlow_DefeatsPlayerAtZeroHealth()
    {
        CombatSystem combat = CreateCombat(CreatePlayer(10, 5, 2), CreateDragon(10, 5, 20));
        dice.Add(1, 20, 12);

        combat.DragonAttacksPlayerSequence();

        Assert.AreEqual(0, combat.Player.Health);
        Assert.IsTrue(combat.IsOver);
        Assert.Contains("With the last words of 'Oh no!', you have been defeated by the dragon.", output.ToString());
    }

    // ---------- Rules for both sides ----------

    [TestMethod]
    public void NoOneCanAttack_AfterSomeoneIsDefeated()
    {
        CombatSystem combat = CreateCombat(CreatePlayer(10, 5, 2), CreateDragon(10, 5, 20));
        dice.Add(1, 20, 12);
        combat.DragonAttacksPlayerSequence(); // player defeated

        // No rolls are queued, so ScriptedDie would fail the test if either attack rolled.
        combat.PlayerAttacksDragonSequence();
        combat.DragonAttacksPlayerSequence();

        Assert.AreEqual(20, combat.Dragon.Health);
        Assert.AreEqual(0, combat.Player.Health);
    }

    [TestMethod]
    public void CombatText_UsesTheChosenLanguage()
    {
        CombatSystem combat = CreateCombat(CreatePlayer(10, 10, 20), CreateDragon(10, 5, 20));
        messages.SetCurrentLanguage(Languages.Spanish);
        dice.Add(11);

        combat.PlayerAttacksDragonSequence();

        Assert.Contains("No le diste a Fafnir.", output.ToString());
    }

    [TestMethod]
    public void Constructor_NullArguments_Throw()
    {
        Player player = CreatePlayer(10, 10, 20);
        Dragon dragon = CreateDragon(10, 5, 20);

        Assert.ThrowsExactly<ArgumentNullException>(() => new CombatSystem(null!, dragon, dice, messages));
        Assert.ThrowsExactly<ArgumentNullException>(() => new CombatSystem(player, null!, dice, messages));
        Assert.ThrowsExactly<ArgumentNullException>(() => new CombatSystem(player, dragon, null!, messages));
        Assert.ThrowsExactly<ArgumentNullException>(() => new CombatSystem(player, dragon, dice, null!));
    }
}

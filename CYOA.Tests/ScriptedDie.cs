namespace CYOA.Tests;

// A Die for tests that returns the rolls you give it, in order, instead of random numbers.
// That makes combat and character creation predictable, so tests can check exact results.
//
//     ScriptedDie dice = new ScriptedDie(10, 5, 20);  // first three rolls
//     dice.Add(1, 15, 7);                             // queue more rolls later
//
// If the code rolls more times than you planned, or a roll doesn't fit the die
// (e.g. 7 on a 4-sided die), the test fails with a clear message.
public class ScriptedDie : Die
{
    private readonly Queue<int> rolls = new Queue<int>();

    public ScriptedDie(params int[] rolls)
    {
        Add(rolls);
    }

    public void Add(params int[] moreRolls)
    {
        foreach (int roll in moreRolls)
        {
            rolls.Enqueue(roll);
        }
    }

    public int RollsLeft => rolls.Count;

    public override int Roll(int sides = 20)
    {
        if (rolls.Count == 0)
        {
            throw new InvalidOperationException($"ScriptedDie ran out of rolls (asked for a d{sides}).");
        }

        int roll = rolls.Dequeue();
        if (roll < 1 || roll > sides)
        {
            throw new InvalidOperationException($"ScriptedDie roll {roll} is not possible on a d{sides}.");
        }

        return roll;
    }
}

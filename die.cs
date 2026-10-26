public class Die
{
    private readonly Random _random = new Random();

    public virtual int Roll(int sides = 20)
    {
        if (sides < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(sides));
        }

        return _random.Next(1, sides + 1);
    }
}
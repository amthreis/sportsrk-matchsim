namespace SRkMatchSimAPI.Framework;

public static class RNG
{
    static Random rng;

    static RNG()
    {
        rng = new Random();
    }

    public static int RandiRange(int from, int to)
    {
        return rng.Next(from, to + 1);
    }

    public static float RandfRange(float from, float to)
    {
        return from + rng.NextSingle() * (to - from);
    }

    public static void SetSeed(int seed)
    {
        rng = new Random(seed);
    }
}

using Godot;
using System.Collections.Generic;
using System.Linq;

public static class BlockRandomizer
{
    public static List<T> Shuffle<T>(IEnumerable<T> source)
    {
        var list = source.ToList();

        var random = new RandomNumberGenerator();
        random.Randomize();

        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = random.RandiRange(0, i);
            (list[i], list[j]) = (list[j], list[i]);
        }

        return list;
    }
}

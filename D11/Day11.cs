using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AdventOfCode;

public class Day11
{
    public void Execute()
    {
        var (width, height, galaxies) = ParseInput("D11/input.txt");

        var space1 = ExpandSpace(width, height, galaxies, 2);
        var sum = GetAllCombinations(space1.Count)
            .Select(pair =>
            {
                var g1 = space1[pair.Item1];
                var g2 = space1[pair.Item2];
                return ManhattanDistance(g1, g2);
            }).Sum();

        var space2 = ExpandSpace(width, height, galaxies, 1_000_000);
        var sum2 = GetAllCombinations(space2.Count)
            .Select(pair =>
            {
                var g1 = space2[pair.Item1];
                var g2 = space2[pair.Item2];
                return ManhattanDistance(g1, g2);
            }).Sum();

        Console.WriteLine($"Day 11 I : {sum}");
        Console.WriteLine($"Day 11 II: {sum2}");
    }

    private (long width, long height, List<Pos> galaxies) ParseInput(string path)
    {
        var stream = File.OpenText(path);
        var galaxies = new List<Pos>();

        long maxX = 0;
        long y = 0;

        while (!stream.EndOfStream)
        {
            var line = stream.ReadLine()!;
            maxX = line.Length;

            for (int x = 0; x < line.Length; x++)
            {
                if (line[x] == '#')
                {
                    galaxies.Add(new(x, y));
                }
            }

            y++;
        }

        return (maxX, y, galaxies);
    }

    private List<Pos> ExpandSpace(long width, long height, List<Pos> galaxies, int expansionRate)
    {
        var newGalaxies = new List<Pos>(galaxies);

        for (long x = width - 1; x >= 0; x--)
        {
            if (newGalaxies.All(g => g.X != x))
            {
                // We found a vertical line with no galaxies 
                for (int i = 0; i < newGalaxies.Count; i++)
                {
                    var g = newGalaxies[i];
                    if (g.X > x)
                    {
                        newGalaxies[i] = new(g.X + expansionRate - 1, g.Y);
                    }
                }
            }
        }

        for (long y = height - 1; y >= 0; y--)
        {
            if (newGalaxies.All(g => g.Y != y))
            {
                // We found a horizontal line with no galaxies 
                for (int i = 0; i < newGalaxies.Count; i++)
                {
                    var g = newGalaxies[i];
                    if (g.Y > y)
                    {
                        newGalaxies[i] = new(g.X, g.Y + expansionRate - 1);
                    }
                }
            }
        }

        return newGalaxies;
    }

    private IEnumerable<(int, int)> GetAllCombinations(int length)
    {
        for (int i = 0; i < length; i++)
        {
            for (int j = 0; j < i; j++)
            {
                yield return (i, j);
            }
        }
    }

    private long ManhattanDistance(Pos a, Pos b) => Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);

    private record struct Pos(long X, long Y);
}


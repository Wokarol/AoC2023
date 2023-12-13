using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AdventOfCode;

public class Day13
{
    public void Execute()
    {
        var patterns = ParseInput("D13/input.txt");

        var sum = patterns
            .Select(x => FindMirror(x, withSmudge: false))
            .Select(x => x.horizontal ? 100 * x.toLeftAbove : x.toLeftAbove)
            .Sum();

        var sum2 = patterns
            .Select(x => FindMirror(x, withSmudge: true))
            .Select(x => x.horizontal ? 100 * x.toLeftAbove : x.toLeftAbove)
            .Sum();

        Console.WriteLine($"Day 13 I : {sum}");
        Console.WriteLine($"Day 13 II: {sum2}");
    }

    private List<Pattern> ParseInput(string path)
    {
        var stream = File.OpenText(path);
        var patterns = new List<Pattern>();

        List<string> patternLines = new();
        while (!stream.EndOfStream)
        {
            patternLines.Clear();
            while(true)
            {
                var line = stream.ReadLine();

                if (string.IsNullOrEmpty(line)) break;

                patternLines.Add(line);
            }

            bool[,] map = new bool[patternLines[0].Length, patternLines.Count];

            for (int x = 0; x < map.GetLength(0); x++)
            {
                for (int y = 0; y < map.GetLength(1); y++)
                {
                    map[x, y] = patternLines[y][x] == '#';
                }
            }

            patterns.Add(new(string.Join('\n', patternLines), map));
        }

        return patterns;
    }

    (bool horizontal, int toLeftAbove) FindMirror(Pattern pattern, bool withSmudge)
    {
        var map = pattern.Map;

        for (int testedX = 0; testedX < map.GetLength(0) - 1; testedX++)
        {
            if (CheckVerticalMirror(map, testedX, withSmudge))
            {
                return (false, testedX + 1);
            }
        }



        for (int testedY = 0; testedY < map.GetLength(1) - 1; testedY++)
        {
            if (CheckHorizontalMirror(map, testedY, withSmudge))
            {
                return (true, testedY + 1);
            }
        }

        return (false, -1000000);
    }

    private static bool CheckVerticalMirror(bool[,] map, int testedX, bool withSmudge)
    {
        int width = map.GetLength(0);
        int mistakesFound = 0;

        for (int y = 0; y < map.GetLength(1); y++)
        {
            for (int x = 0; x <= testedX; x++)
            {
                int otherXToTest = testedX + (testedX - x) + 1;

                if (x >= width) continue;
                if (otherXToTest >= width) continue;
                if (map[x, y] != map[otherXToTest, y])
                {
                    if (!withSmudge) return false;

                    mistakesFound += 1;
                    if (mistakesFound >= 2) return false;
                }
            }
        }

        return !withSmudge || mistakesFound == 1;
    }

    private static bool CheckHorizontalMirror(bool[,] map, int testedY, bool withSmudge)
    {
        int height = map.GetLength(1);
        int mistakesFound = 0;

        for (int x = 0; x < map.GetLength(0); x++)
        {
            for (int y = 0; y <= testedY; y++)
            {
                int otherYToTest = testedY + (testedY - y) + 1;

                if (y >= height) continue;
                if (otherYToTest >= height) continue;
                if (map[x, y] != map[x, otherYToTest])
                {
                    if (!withSmudge) return false;

                    mistakesFound += 1;
                    if( mistakesFound >= 2) return false;
                }
            }
        }

        return !withSmudge || mistakesFound == 1;
    }

    record Pattern(string Raw, bool[,] Map);
}


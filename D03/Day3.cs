using System.Collections;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace AdventOfCode;

public class Day3
{
    public void Execute()
    {
        var lines = File.ReadAllLines("D03/input.txt");
        var (map, digitGroups, stars) = ParseInput(lines);


        var validGroups = digitGroups.Where(g =>
        {
            var left = IsSymbol(map[g.Line, g.Index - 1]);
            var right = IsSymbol(map[g.Line, g.Index + g.Length]);
            var under = Enumerable.Range(g.Index - 1, g.Length + 2).Any(i => IsSymbol(map[g.Line + 1, i]));
            var over = Enumerable.Range(g.Index - 1, g.Length + 2).Any(i => IsSymbol(map[g.Line - 1, i]));

            return left || right || under || over;
        });

        List<int> cogs = new();

        foreach (var s in stars)
        {
            var adjacentGroups = digitGroups.Where(g => g.IsAdjacentTo(s)).ToList();
            if (adjacentGroups.Count() == 2)
            {
                cogs.Add(adjacentGroups[0].Value * adjacentGroups[1].Value);
            }
        }

        Console.WriteLine($"Day  3 I : {validGroups.Sum(g => g.Value)}");
        Console.WriteLine($"Day  3 II: {cogs.Sum()}");
    }

    private static (MapWrapper map, IEnumerable<DigitGroup> groups, List<Cell>) ParseInput(string[] lines)
    {
        List<DigitGroup> digitGroups = new();
        List<Cell> stars = new();

        for (int i = 0; i < lines.Length; i++)
        {
            string? line = lines[i];
            var matches = Regex.Matches(line, @"\d+");

            foreach (var m in matches.AsEnumerable())
            {
                digitGroups.Add(new(i, m.Index, m.Length, int.Parse(m.Value)));
            }

            var foundStars = line
                .Select((c, i) => (c, i))
                .Where(x => x.c is '*')
                .Select(x => new Cell(i, x.i));

            stars.AddRange(foundStars);
        }

        var map = new MapWrapper(lines);

        return (map, digitGroups, stars);
    }

    record struct DigitGroup(int Line, int Index, int Length, int Value)
    {
        public bool IsAdjacentTo(Cell cell)
        {
            if (cell.Line < Line - 1) return false;
            if (cell.Line > Line + 1) return false;
            if (cell.Index < Index - 1) return false;
            if (cell.Index > Index + Length) return false;

            return true;
        }
    }

    record struct Cell(int Line, int Index);

    private static bool IsSymbol(char c)
    {
        return c is not ('.' or (> '0' and < '9'));
    }

    struct MapWrapper
    {
        private readonly string[] lines;

        public readonly char this[int line, int pos]
        {
            get
            {
                if (line < 0 || line >= lines.Length) return '.';
                var l = lines[line];

                if (pos < 0 || pos >= l.Length) return '.';

                return l[pos];
            }
        }

        public MapWrapper(string[] lines)
        {
            this.lines = lines;
        }
    }
}
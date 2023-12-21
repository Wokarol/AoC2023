using System.Collections;
using System.Collections.Concurrent;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AdventOfCode;

public class Day21
{

    public void Execute()
    {
        var map = ParseInput("D21/input.txt");

        var part1 = Solve(map, 6);
        //var part2 = Solve(map, 26501365);

        Console.WriteLine($"Day 21 I : {part1}");
        Console.WriteLine($"Day 21 II: [TAKES TOO LONG]");
    }

    private int Solve(Map map, int steps)
    {
        HashSet<(int, int)> consideredTiles = new(); // All tiles we visited or added to the queues
        Queue<(int x, int y, int stepsLeft)> tilesToCheck = new();
        int totalVisitableTiles = 0;

        consideredTiles.Add((map.startX, map.startY));
        tilesToCheck.Enqueue((map.startX, map.startY, steps));

        bool evenAreVisitable = Repeat(map.startX, 2) == Repeat(map.startY, 2);
        if (steps % 2 != 0)
            evenAreVisitable = !evenAreVisitable;

        int checkedTiles = 0;
        long allExpectedTilesToCheck = steps * (long)steps;

        while (tilesToCheck.Count > 0)
        {
            var (x, y, stepsLeft) = tilesToCheck.Dequeue();

            if ((Repeat(x, 2) == Repeat(y, 2)) == evenAreVisitable)
            {
                totalVisitableTiles++;
            }

            checkedTiles++;

            if (checkedTiles % 10_000_000 == 0) Console.WriteLine($"[{DateTime.Now}] {(checkedTiles / (double)allExpectedTilesToCheck) * 100:F10}%");

            if (stepsLeft <= 0) continue;

            MaybeAdd(x + 1, y, stepsLeft - 1);
            MaybeAdd(x - 1, y, stepsLeft - 1);
            MaybeAdd(x, y + 1, stepsLeft - 1);
            MaybeAdd(x, y - 1, stepsLeft - 1);
        }

        return totalVisitableTiles;


        void MaybeAdd(int x, int y, int stepsLeft)
        {
            if (map[x, y] != '#' && !consideredTiles.Contains((x, y)))
            {
                consideredTiles.Add((x, y));
                tilesToCheck.Enqueue((x, y, stepsLeft));
            }
        }
    }

    private Map ParseInput(string path)
    {
        var lines = File.ReadAllLines(path);

        int startX = -1;
        int startY = -1;

        for (int y = 0; y < lines.Length; y++)
        {
            for (int x = 0; x < lines[0].Length; x++)
            {
                if (lines[y][x] == 'S')
                {
                    startX = x;
                    startY = y;
                    break;
                }
            }
        }

        return new Map(lines, startX, startY);
    }

    static int Repeat(int dividend, int divisor)
    {
        int remainder = dividend % divisor;
        return (remainder < 0) ? remainder + divisor : remainder;
    }

    class Map
    {
        public readonly string[] lines;
        public readonly int startX;
        public readonly int startY;

        public int Width => lines[0].Length;
        public int Height => lines.Length;
        public char this[int x, int y] => lines[Repeat(y, lines.Length)][Repeat(x, lines[0].Length)];

        public Map(string[] lines, int startX, int startY)
        {
            this.lines = lines;
            this.startX = startX;
            this.startY = startY;
        }
    }
}


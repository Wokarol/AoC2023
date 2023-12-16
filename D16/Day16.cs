using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AdventOfCode;

public class Day16
{

    public void Execute()
    {
        var map = ParseInput("D16/input.txt");

        var energizedTiles = GetEnergizedTiles(map, new(-1, 0, 1, 0));
        var maxEnergizedTiles = GetAllWalkers(map.Width, map.Height).Select(w => GetEnergizedTiles(map, w)).Max();

        Console.WriteLine($"Day 16 I : {energizedTiles}");
        Console.WriteLine($"Day 16 II: {maxEnergizedTiles}");
    }

    private int GetEnergizedTiles(Map map, Walker startingWalker)
    {
        HashSet<(int x, int y, int xd, int yd)> energized = new();

        List<Walker> walkers = new List<Walker>
        {
            startingWalker
        };

        while (walkers.Count > 0)
        {
            for (int i = walkers.Count - 1; i >= 0; i--)
            {
                var w = walkers[i];
                w.X += w.XDir;
                w.Y += w.YDir;

                if (!map.IsInBounds(w.X, w.Y))
                {
                    walkers.RemoveAt(i);
                    continue;
                }

                if (energized.Contains((w.X, w.Y, w.XDir, w.YDir)))
                {
                    walkers.RemoveAt(i);
                    continue;
                }
                else
                {
                    energized.Add((w.X, w.Y, w.XDir, w.YDir));
                }

                var tile = map[w.X, w.Y];

                switch (tile)
                {
                    case Tile.VSplitter when w.XDir != 0: // We hit vertical splitter while moving horizontally
                        w.YDir = 1;
                        w.XDir = 0;
                        walkers.Add(new(w.X, w.Y, 0, -1));
                        break;
                    case Tile.HSplitter when w.YDir != 0: // We hit horizontal splitter while moving vertically
                        w.YDir = 0;
                        w.XDir = 1;
                        walkers.Add(new(w.X, w.Y, -1, 0));
                        break;
                    case Tile.RisingMirror:
                        (w.XDir, w.YDir) = (-w.YDir, -w.XDir);
                        break;
                    case Tile.FallingMirror:
                        (w.XDir, w.YDir) = (w.YDir, w.XDir);
                        break;
                }
            }
        }

        return energized.DistinctBy(x => (x.x, x.y)).Count();
    }

    private Map ParseInput(string path)
    {
        var lines = File.ReadAllLines(path);
        return new Map(lines);
    }

    private IEnumerable<Walker> GetAllWalkers(int width, int height)
    {
        for (int i = 0; i < height; i++)
        {
            yield return new Walker(-1, i, 1, 0);
            yield return new Walker(width, i, -1, 0);
        }

        for (int i = 0; i < width; i++)
        {
            yield return new Walker(i, -1, 0, 1);
            yield return new Walker(i, height, 0, -1);
        }
    }

    //   0 1 2  
    // 0 o-->
    // 1 |
    // 2 v
    class Map
    {
        private readonly string[] lines;

        public Map(string[] lines)
        {
            this.lines = lines;
        }

        public Tile this[int x, int y] => lines[y][x] switch
        {
            '.' => Tile.Empty,
            '|' => Tile.VSplitter,
            '-' => Tile.HSplitter,
            '\\' => Tile.FallingMirror,
            '/' => Tile.RisingMirror,
            _ => throw new NotImplementedException()
        };

        public int Width => lines[0].Length;
        public int Height => lines.Length;

        public bool IsInBounds(int x, int y)
        {
            if (y < 0 || y >= lines.Length) return false;
            if (x < 0 || x >= lines[0].Length) return false;

            return true;
        }
    }

    class Walker
    {
        public int X, Y;
        public int XDir, YDir;

        public Walker(int x, int y, int xDir, int yDir)
        {
            X = x;
            Y = y;
            XDir = xDir;
            YDir = yDir;
        }

        public override string ToString() => $"[{X}, {Y} | {XDir}, {YDir}]";
    }

    enum Tile { Empty, VSplitter, HSplitter, RisingMirror, FallingMirror }
}


using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AdventOfCode;

public class Day10
{
    public void Execute()
    {
        var (startX, startY, startDirections, map) = ParseInput("D10/input.txt");
        var (cells, sideTiles) = CalculateLoop(startX, startY, startDirections, map);

        var insideTiles = Floodfill(cells, sideTiles);

        Console.WriteLine($"Day 10 I : {cells.Count / 2}");
        Console.WriteLine($"Day 10 II: {insideTiles}");
    }

    private (HashSet<Pos> cells, List<Pos> sideTiles) CalculateLoop(int startX, int startY, Direction startDirections, List<List<Direction>> map)
    {
        var currentX = startX;
        var currentY = startY;
        var nextDir = GetOneDirection(startDirections);

        HashSet<Pos> cells = new HashSet<Pos>();
        List<Pos> potentialLeftSide = new();
        List<Pos> potentialRightSide = new();

        int turns = 0;

        while (true)
        {
            cells.Add(new(currentX, currentY));

            (currentX, currentY) = nextDir switch
            {
                Direction.Left => (currentX - 1, currentY),
                Direction.Right => (currentX + 1, currentY),
                Direction.Up => (currentX, currentY - 1),
                Direction.Down => (currentX, currentY + 1),
                _ => throw new NotImplementedException(),
            };

            if (currentX == startX && currentY == startY)
            {
                break;
            }

            var nextJunction = map[currentY][currentX];
            var lastDir = nextDir;
            nextDir = nextJunction & ~Inverse(lastDir);

            turns += (lastDir, nextDir) switch
            {
                (Direction.Up, Direction.Right) => 1,
                (Direction.Up, Direction.Left) => -1,
                (Direction.Right, Direction.Down) => 1,
                (Direction.Right, Direction.Up) => -1,
                (Direction.Down, Direction.Left) => 1,
                (Direction.Down, Direction.Right) => -1,
                (Direction.Left, Direction.Up) => 1,
                (Direction.Left, Direction.Down) => -1,
                _ => 0,
            };

            AddSeeds(lastDir, nextDir, true, potentialRightSide, currentX, currentY);
            AddSeeds(lastDir, nextDir, false, potentialLeftSide, currentX, currentY);
        }
        
        return (cells, turns < 0 ? potentialLeftSide : potentialRightSide);
    }

    private int Floodfill(HashSet<Pos> cells, List<Pos> seeds)
    {
        HashSet<Pos> insideTiles = new HashSet<Pos>();
        Stack<Pos> tilesToCheck = new Stack<Pos>(seeds);

        while(tilesToCheck.Count > 0)
        {
            var t = tilesToCheck.Pop();
            if (cells.Contains(t)) continue;
            if (insideTiles.Contains(t)) continue;

            insideTiles.Add(t);

            tilesToCheck.Push(new(t.x + 1, t.y));
            tilesToCheck.Push(new(t.x - 1, t.y));
            tilesToCheck.Push(new(t.x, t.y + 1));
            tilesToCheck.Push(new(t.x, t.y - 1));
        }

        return insideTiles.Count;
    }

    private Direction GetOneDirection(Direction dir)
    {
        if ((dir & Direction.Up) != 0) return Direction.Up;
        if ((dir & Direction.Right) != 0) return Direction.Right;
        if ((dir & Direction.Down) != 0) return Direction.Down;
        if ((dir & Direction.Left) != 0) return Direction.Left;

        return Direction.None;
    }

    private Direction Inverse(Direction dir) => dir switch
    {
        Direction.Left => Direction.Right,
        Direction.Right => Direction.Left,
        Direction.Up => Direction.Down,
        Direction.Down => Direction.Up,
        _ => dir,
    };

    private void AddSeeds(Direction previous, Direction next, bool rightSide, List<Pos> currentSeed, int x, int y)
    {
        // Y- Up, X+ Right

        switch ((previous, next, rightSide))
        {
            // | from below
            case (Direction.Up, Direction.Up, _):
                currentSeed.Add(new(x + (rightSide ? 1 : -1), y));
                break;

            // | from above
            case (Direction.Down, Direction.Down, _):
                currentSeed.Add(new(x + (rightSide ? -1 : 1), y));
                break;

            // - from below
            case (Direction.Right, Direction.Right, _):
                currentSeed.Add(new(x, y + (rightSide ? 1 : -1)));
                break;

            // - from above
            case (Direction.Left, Direction.Left, _):
                currentSeed.Add(new(x, y + (rightSide ? -1 : 1)));
                break;

            // L from above
            case (Direction.Down, Direction.Right, true):
                currentSeed.Add(new(x - 1, y));
                currentSeed.Add(new(x, y + 1));
                break;

            // L from right
            case (Direction.Left, Direction.Up, false):
                currentSeed.Add(new(x - 1, y));
                currentSeed.Add(new(x, y + 1));
                break;

            // J from left
            case (Direction.Right, Direction.Up, true):
                currentSeed.Add(new(x + 1, y));
                currentSeed.Add(new(x, y + 1));
                break;

            // J from above
            case (Direction.Down, Direction.Left, false):
                currentSeed.Add(new(x + 1, y));
                currentSeed.Add(new(x, y + 1));
                break;

            // 7 from left
            case (Direction.Right, Direction.Down, false):
                currentSeed.Add(new(x + 1, y));
                currentSeed.Add(new(x, y - 1));
                break;

            // 7 from below
            case (Direction.Up, Direction.Left, true):
                currentSeed.Add(new(x + 1, y));
                currentSeed.Add(new(x, y - 1));
                break;

            // F from below
            case (Direction.Up, Direction.Right, false):
                currentSeed.Add(new(x - 1, y));
                currentSeed.Add(new(x, y - 1));
                break;

            // F from right
            case (Direction.Left, Direction.Down, true):
                currentSeed.Add(new(x - 1, y));
                currentSeed.Add(new(x, y - 1));
                break;
        }
    }

    private (int startX, int startY, Direction startDirections, List<List<Direction>> map) ParseInput(string path)
    {
        var stream = File.OpenText(path);
        var map = new List<List<Direction>>();

        while (!stream.EndOfStream)
        {
            var line = stream.ReadLine()!;

            map.Add(line.Select(c =>
            {
                return c switch
                {
                    '|' => Direction.Up | Direction.Down,
                    '-' => Direction.Left | Direction.Right,
                    'L' => Direction.Up | Direction.Right,
                    'J' => Direction.Left | Direction.Up,
                    '7' => Direction.Left | Direction.Down,
                    'F' => Direction.Down | Direction.Right,
                    'S' => Direction.Start,
                    '.' => Direction.None,
                    _ => throw new Exception(),
                };
            }).ToList());
        }

        int startX = -1;
        int startY = -1;
        Direction startDirections = Direction.None;

        for (int x = 0; x < map[0].Count; x++)
        {
            for (int y = 0; y < map.Count; y++)
            {
                if (map[y][x] == Direction.Start)
                {
                    startX = x;
                    startY = y;

                    if (x >= 0 && x < map[0].Count - 1 && (map[y][x + 1] & Direction.Left) != 0) startDirections |= Direction.Right;
                    if (x >= 1 && x < map[0].Count && (map[y][x - 1] & Direction.Right) != 0) startDirections |= Direction.Left;

                    if (y >= 0 && y < map.Count - 1 && (map[y + 1][x] & Direction.Up) != 0) startDirections |= Direction.Down;
                    if (y >= 1 && y < map.Count && (map[y - 1][x] & Direction.Down) != 0) startDirections |= Direction.Up;
                }
            }
        }

        return (startX, startY, startDirections, map);
    }

    [Flags]
    enum Direction
    {
        None = 0,
        Start = 1,
        Left = 2,
        Right = 4,
        Up = 8,
        Down = 16,
    }

    record struct Pos(int x, int y);
}


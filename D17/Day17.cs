using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AdventOfCode;

public class Day17
{

    public void Execute()
    {
        var map = ParseInput("D17/input.txt");

        var lowestLoss = ComputeLowestHeatLoss(map);
        var superLowestLoss = ComputeSuperLowestHeatLoss(map);

        Console.WriteLine($"Day 17 I : {lowestLoss}");
        Console.WriteLine($"Day 17 II: {superLowestLoss}");
    }

    private Map ParseInput(string path)
    {
        var lines = File.ReadAllLines(path);

        var tiles = new int[lines[0].Length, lines.Length];

        for (int y = 0; y < lines.Length; y++)
        {
            for (int x = 0; x < lines[0].Length; x++)
            {
                tiles[x, y] = lines[y][x] - '0';
            }
        }

        return new Map(tiles);
    }

    private int ComputeLowestHeatLoss(Map map)
    {
        int[,,] lowestHeatLossPerTile = new int[4 * 3, map.Width, map.Height];

        for (int i1 = 0; i1 < lowestHeatLossPerTile.GetLength(0); i1++)
            for (int i2 = 0; i2 < lowestHeatLossPerTile.GetLength(1); i2++)
                for (int i3 = 0; i3 < lowestHeatLossPerTile.GetLength(2); i3++)
                    lowestHeatLossPerTile[i1, i2, i3] = int.MaxValue;


        PriorityQueue<Walker, int> walkers = new();
        walkers.Enqueue(new Walker(0, 0, 1, 0, 0, 0, 0), GetHeuristic(0, 0, map));
        walkers.Enqueue(new Walker(0, 0, 0, 1, 0, 0, 0), GetHeuristic(0, 0, map));

        int lastX = map.Width - 1;
        int lastY = map.Height - 1;

        while (walkers.Count > 0)
        {
            var walker = walkers.Dequeue();

            int x = walker.X + walker.DirX;
            int y = walker.Y + walker.DirY;

            if (!map.IsInBounds(x, y))
                continue;

            int currentLoss = walker.CurrentLoss + map[x, y];
            int currentSteps = walker.StepsDone + 1;
            int lowestHeatLossForState = int.MaxValue;

            for (int i = 1; i <= currentSteps; i++)
            {
                var stateLoss = lowestHeatLossPerTile[GetCacheIndexFor(walker.DirX, walker.DirY, i), x, y];
                if (stateLoss < lowestHeatLossForState)
                    lowestHeatLossForState = stateLoss;
            }

            if (currentLoss >= lowestHeatLossForState)
            {
                // This walker run into the same state as another, but with worse result
                // This should also check lower? step counts as it's always better to have less steps?
                continue;
            }

            lowestHeatLossPerTile[GetCacheIndexFor(walker.DirX, walker.DirY, currentSteps), x, y] = currentLoss;

            var forwardWalker = new Walker(x, y, walker.DirX, walker.DirY, currentSteps, currentLoss, walker.AllSteps + 1);
            var leftWalker = new Walker(x, y, -walker.DirY, walker.DirX, 0, currentLoss, walker.AllSteps + 1);
            var rightWalker = new Walker(x, y, walker.DirY, -walker.DirX, 0, currentLoss, walker.AllSteps + 1);

            int priority = currentLoss + GetHeuristic(x, y, map);

            if (currentSteps < 3)
            {
                walkers.Enqueue(forwardWalker, priority);
            }

            walkers.Enqueue(rightWalker, priority);
            walkers.Enqueue(leftWalker, priority);

            if (x == lastX && y == lastY) break;
        }

        return Enumerable.Range(0, 3 * 4)
            .Min(i => lowestHeatLossPerTile[i, lastX, lastY]);
    }

    private int GetCacheIndexFor(int dirX, int dirY, int stepsDone)
    {
        return
            stepsDone - 1 + // 0 .. 2
            3 * (dirX, dirY) switch // 0 3 6 9
            {
                (-1, 0) => 0,
                (1, 0) => 1,
                (0, -1) => 2,
                (0, 1) => 3,
                _ => throw new Exception()
            };
    }



    private int ComputeSuperLowestHeatLoss(Map map)
    {
        int[,,] lowestHeatLossPerTile = new int[4 * 10, map.Width, map.Height];

        for (int i1 = 0; i1 < lowestHeatLossPerTile.GetLength(0); i1++)
            for (int i2 = 0; i2 < lowestHeatLossPerTile.GetLength(1); i2++)
                for (int i3 = 0; i3 < lowestHeatLossPerTile.GetLength(2); i3++)
                    lowestHeatLossPerTile[i1, i2, i3] = int.MaxValue;


        PriorityQueue<Walker, int> walkers = new();
        walkers.Enqueue(new Walker(0, 0, 1, 0, 0, 0, 0), GetHeuristic(0, 0, map));
        walkers.Enqueue(new Walker(0, 0, 0, 1, 0, 0, 0), GetHeuristic(0, 0, map));

        int lastX = map.Width - 1;
        int lastY = map.Height - 1;

        while (walkers.Count > 0)
        {
            var walker = walkers.Dequeue();

            int x = walker.X + walker.DirX;
            int y = walker.Y + walker.DirY;

            if (!map.IsInBounds(x, y))
                continue;

            int currentLoss = walker.CurrentLoss + map[x, y];
            int currentSteps = walker.StepsDone + 1;
            int lowestHeatLossForState = int.MaxValue;

            for (int i = 1; i <= currentSteps; i++)
            {
                var stateLoss = lowestHeatLossPerTile[GetSuperCacheIndexFor(walker.DirX, walker.DirY, i), x, y];
                if (stateLoss < lowestHeatLossForState)
                    lowestHeatLossForState = stateLoss;
            }

            if (currentLoss >= lowestHeatLossForState)
            {
                // This walker run into the same state as another, but with worse result
                // This should also check lower? step counts as it's always better to have less steps?
                continue;
            }

            if (currentSteps >= 4)
                lowestHeatLossPerTile[GetSuperCacheIndexFor(walker.DirX, walker.DirY, currentSteps), x, y] = currentLoss;

            var forwardWalker = new Walker(x, y, walker.DirX, walker.DirY, currentSteps, currentLoss, walker.AllSteps + 1);
            var leftWalker = new Walker(x, y, -walker.DirY, walker.DirX, 0, currentLoss, walker.AllSteps + 1);
            var rightWalker = new Walker(x, y, walker.DirY, -walker.DirX, 0, currentLoss, walker.AllSteps + 1);

            int priority = currentLoss + GetHeuristic(x, y, map);

            if (currentSteps < 10)
            {
                walkers.Enqueue(forwardWalker, priority);
            }

            if (currentSteps >= 4)
            {
                walkers.Enqueue(rightWalker, priority);
                walkers.Enqueue(leftWalker, priority);
            }

            if (x == lastX && y == lastY)
            {
                if (currentSteps >= 4)
                    break;
            }
        }

        return Enumerable.Range(0, 10 * 4)
            .Min(i => lowestHeatLossPerTile[i, lastX, lastY]);
    }

    private int GetSuperCacheIndexFor(int dirX, int dirY, int stepsDone)
    {
        return
            stepsDone - 1 + // 0 .. 9
            10 * (dirX, dirY) switch // 0 10 20 30
            {
                (-1, 0) => 0,
                (1, 0) => 1,
                (0, -1) => 2,
                (0, 1) => 3,
                _ => throw new Exception()
            };
    }

    private int GetHeuristic(int x, int y, Map map)
    {
        return 0; // (map.Width - x) + (map.Height - y);
    }

    record struct Walker(int X, int Y, int DirX, int DirY, int StepsDone, int CurrentLoss, int AllSteps);

    class Map
    {
        private readonly int[,] cost;

        public Map(int[,] costs)
        {
            this.cost = costs;
        }

        public int Width => cost.GetLength(0);
        public int Height => cost.GetLength(1);
        public int this[int x, int y] => cost[x, y];

        public bool IsInBounds(int x, int y)
        {
            if (y < 0 || y >= Height) return false;
            if (x < 0 || x >= Width) return false;

            return true;
        }
    }
}


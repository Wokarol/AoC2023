using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AdventOfCode;

public class Day14
{
    public void Execute()
    {
        var map = ParseInput("D14/input.txt");
        var map2 = ParseInput("D14/input.txt");

        TiltNorth(map);

        List<int> loads = new(100);

        for (int i = 0; i < 1000; i++)
        {
            TiltNorth(map2);
            TiltWest(map2);
            TiltSouth(map2);
            TiltEast(map2);

            int load = CalculateLoad(map2);
            loads.Add(load);
        }

        Console.WriteLine($"Day 14 I : {CalculateLoad(map)}");
        Console.WriteLine($"Day 14 II: {EstimateFromPattern(loads, 700, 1000000000)}");
    }

    private List<List<TileType>> ParseInput(string path)
    {
        var stream = File.OpenText(path);
        var map = new List<List<TileType>>();

        while (!stream.EndOfStream)
        {
            var line = stream.ReadLine()!;

            map.Add(line.Select(c => c switch
            {
                'O' => TileType.RoundRock,
                '#' => TileType.SquareRock,
                '.' => TileType.Empty,
                _ => throw new Exception()
            }).ToList());
        }

        return map;
    }

    private int CalculateLoad(List<List<TileType>> map)
    {
        return map
            .Select((l, i) =>
            {
                return l.Where(t => t == TileType.RoundRock).Select(t => map.Count - i).Sum();
            })
            .Sum();
    }

    private int EstimateFromPattern(List<int> loads, int safeMargin, int targetTick)
    {
        // The pattern at some point (after safety margin) will start looping like 10 9 7 6 4 5 6 4 5 6 4 5 ...
        int maxLoopLength = (loads.Count - safeMargin) / 2;
        for (int loopLength = 1; loopLength < maxLoopLength; loopLength++)
        {
            if (CheckIfLoopExists(loads, safeMargin, loopLength))
            {
                var remainingTicks = targetTick - safeMargin;
                var offset = remainingTicks % loopLength;

                return loads[safeMargin + offset - 1];
            }
        }

        return -1;
    }

    private static bool CheckIfLoopExists(List<int> loads, int offset, int loopLength)
    {
        for (int i = 0; i < loopLength; i++)
        {
            if (loads[offset + i] != loads[offset + i + loopLength]) return false;
        }

        return true;
    }

    private void TiltNorth(List<List<TileType>> map)
    {
        for (int x = 0; x < map[0].Count; x++)
        {
            bool packed = true;
            int firstEmptyY = -1;
            for (int y = 0; y < map.Count; y++)
            {
                if (map[y][x] == TileType.Empty)
                {
                    packed = false;
                    if (firstEmptyY == -1)
                        firstEmptyY = y;

                    continue;
                }

                if (map[y][x] == TileType.SquareRock)
                {
                    packed = true;
                    firstEmptyY = -1;
                    continue;
                }

                if (!packed && map[y][x] == TileType.RoundRock)
                {
                    map[firstEmptyY][x] = TileType.RoundRock;
                    map[y][x] = TileType.Empty;
                    firstEmptyY++;
                }
            }
        }
    }

    private void TiltSouth(List<List<TileType>> map)
    {
        for (int x = 0; x < map[0].Count; x++)
        {
            bool packed = true;
            int firstEmptyY = -1;
            for (int y = map.Count - 1; y >= 0; y--)
            {
                if (map[y][x] == TileType.Empty)
                {
                    packed = false;
                    if (firstEmptyY == -1)
                        firstEmptyY = y;

                    continue;
                }

                if (map[y][x] == TileType.SquareRock)
                {
                    packed = true;
                    firstEmptyY = -1;
                    continue;
                }

                if (!packed && map[y][x] == TileType.RoundRock)
                {
                    map[firstEmptyY][x] = TileType.RoundRock;
                    map[y][x] = TileType.Empty;
                    firstEmptyY--;
                }
            }
        }
    }

    private void TiltWest(List<List<TileType>> map)
    {
        for (int y = 0; y < map.Count; y++)
        {
            bool packed = true;
            int firstEmptyX = -1;
            for (int x = 0; x < map[0].Count; x++)
            {
                if (map[y][x] == TileType.Empty)
                {
                    packed = false;
                    if (firstEmptyX == -1)
                        firstEmptyX = x;

                    continue;
                }

                if (map[y][x] == TileType.SquareRock)
                {
                    packed = true;
                    firstEmptyX = -1;
                    continue;
                }

                if (!packed && map[y][x] == TileType.RoundRock)
                {
                    map[y][firstEmptyX] = TileType.RoundRock;
                    map[y][x] = TileType.Empty;
                    firstEmptyX++;
                }
            }
        }
    }
    private void TiltEast(List<List<TileType>> map)
    {
        for (int y = 0; y < map.Count; y++)
        {
            bool packed = true;
            int firstEmptyX = -1;
            for (int x = map[0].Count - 1; x >= 0; x--)
            {
                if (map[y][x] == TileType.Empty)
                {
                    packed = false;
                    if (firstEmptyX == -1)
                        firstEmptyX = x;

                    continue;
                }

                if (map[y][x] == TileType.SquareRock)
                {
                    packed = true;
                    firstEmptyX = -1;
                    continue;
                }

                if (!packed && map[y][x] == TileType.RoundRock)
                {
                    map[y][firstEmptyX] = TileType.RoundRock;
                    map[y][x] = TileType.Empty;
                    firstEmptyX--;
                }
            }
        }
    }

    enum TileType { SquareRock, RoundRock, Empty }
}


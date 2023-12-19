using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AdventOfCode;

public class Day18
{

    public void Execute()
    {
        var (simpleInstructions, complexInstructions) = ParseInput("D18/input.txt");

        int volume = ComputeVolume(simpleInstructions);
        Console.WriteLine($"Day 18 I : {volume}");

        //int complexVolume = ComputeVolume(complexInstructions);
        //Console.WriteLine($"Day 18 II: {complexVolume}");
        Console.WriteLine($"Day 18 II: [TAKES TOO LONG]");
    }

    private int ComputeVolume(List<Instruction> instructions)
    {
        HashSet<(int, int)> map = new(); // Y+ up, X+ right

        int x = 0;
        int y = 0;
        int turns = 0;
        char lastDirection = '\0';

        map.Add((0, 0));

        foreach (var instruction in instructions)
        {
            var dirX = instruction.Direction switch
            {
                'R' => 1,
                'L' => -1,
                _ => 0
            };

            var dirY = instruction.Direction switch
            {
                'U' => 1,
                'D' => -1,
                _ => 0
            };

            turns += (lastDirection, instruction.Direction) switch
            {
                ('R', 'D') => 1,
                ('D', 'L') => 1,
                ('L', 'U') => 1,
                ('U', 'R') => 1,
                ('R', 'U') => -1,
                ('D', 'R') => -1,
                ('L', 'D') => -1,
                ('U', 'L') => -1,
                _ => 0
            };
            lastDirection = instruction.Direction;

            for (int i = 0; i < instruction.Count; i++)
            {
                x += dirX;
                y += dirY;

                if (x == 0 && y == 0) break;

                map.Add((x, y));
            }
        }

        var floodFillSeed = FindFloodFillSeed(instructions[0], turns > 0);
        Floodfill(map, floodFillSeed.x, floodFillSeed.y);

        int volume = map.Count();
        return volume;
    }

    private void Floodfill(HashSet<(int, int)> map, int startX, int startY)
    {
        Stack<(int x, int y)> pointsToCheck = new();
        pointsToCheck.Push((startX, startY));
        
        while (pointsToCheck.Count > 0)
        {
            var p = pointsToCheck.Pop();
            if (map.Contains(p)) continue;

            map.Add(p);

            pointsToCheck.Push((p.x + 1, p.y));
            pointsToCheck.Push((p.x - 1, p.y));
            pointsToCheck.Push((p.x, p.y + 1));
            pointsToCheck.Push((p.x, p.y - 1));
        }
    }

    private (int x, int y) FindFloodFillSeed(Instruction instruction, bool isOnRight)
    {
        return (instruction.Direction, isOnRight) switch
        {
            ('U', false) => (-1, 1),
            ('U', true) => (1, 1),

            ('R', false) => (1, 1),
            ('R', true) => (1, -1),

            ('D', false) => (1, -1),
            ('D', true) => (-1, -1),

            ('L', false) => (-1, -1),
            ('L', true) => (-1, 1),
            _ => throw new NotImplementedException()
        };
    }

    private (List<Instruction> simple, List<Instruction> complex) ParseInput(string path)
    {
        var stream = File.OpenText(path);

        List<Instruction> simpleInstructions = new();
        List<Instruction> complexInstructions = new();
        while (!stream.EndOfStream)
        {
            var line = stream.ReadLine()!;

            var matches = Regex.Match(line, @"(\w)\s(\d+)\s\(#(.+?)(\d)\)");
            var distance = matches.Groups[3].Value;
            var direction = matches.Groups[4].Value switch
            {
                "0" => 'R',
                "1" => 'D',
                "2" => 'L',
                "3" => 'U',
                _ => throw new NotImplementedException()
            };

            simpleInstructions.Add(new Instruction(
                matches.Groups[1].Value[0],
                int.Parse(matches.Groups[2].Value)
            ));

            complexInstructions.Add(new Instruction(
                direction,
                int.Parse(distance, System.Globalization.NumberStyles.HexNumber)
            ));
        }

        return (simpleInstructions, complexInstructions);
    }

    record Instruction(char Direction, int Count);
}


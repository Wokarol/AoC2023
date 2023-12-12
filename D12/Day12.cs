using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AdventOfCode;

public class Day12
{
    public void Execute()
    {
        var rows = ParseInput("D12/input.txt");

        var combinations = rows.Sum(x => CalculateCombinations(x));
        Console.WriteLine($"Day 12 I : {combinations}");

        //var combinations2 = rows.Select((x, i) =>
        //{
        //    if (i % 1 == 0)
        //    {
        //        Console.WriteLine($"{(i / (float)rows.Count) * 100:F2}%");
        //    }

        //    return CalculateCombinations2(x);
        //}).Sum();
        //Console.WriteLine($"Day 12 II: {combinations2}");
        Console.WriteLine($"Day 12 II: [TAKES TO LONG]");
    }

    private int CalculateCombinations(Row row)
    {
        Span<SpringState> springs = stackalloc SpringState[row.Springs.Count];
        for (int i = 0; i < row.Springs.Count; i++)
        {
            springs[i] = row.Springs[i];
        }

        return TryCombinations(springs, row.Groups, row.Groups.Sum());
    }

    private int CalculateCombinations2(Row row)
    {
        Span<SpringState> springs = stackalloc SpringState[row.Springs.Count * 5 + row.Springs.Count - 1];
        int offset = 0;
        for (int j = 0; j < 5; j++)
        {
            for (int i = 0; i < row.Springs.Count; i++)
            {
                springs[i + offset] = row.Springs[i];
            }

            offset += row.Springs.Count;

            springs[offset] = SpringState.Unknown;
            offset += 1;
        }

        var groupsCopy = Enumerable.Repeat(row.Groups, 5).SelectMany(x => x).ToList();

        return TryCombinations(springs, groupsCopy, groupsCopy.Sum());
    }

    private int TryCombinations(ReadOnlySpan<SpringState> springs, IReadOnlyList<int> groups, int groupsSum)
    {
        bool noUnkowns = true;
        int firstUnkownIndex = -1;
        int brokenSpringsSoFar = 0;
        for (int i = 0; i < springs.Length; i++)
        {
            if (springs[i] == SpringState.Unknown)
            {
                noUnkowns = false;
                firstUnkownIndex = i;
                break;
            }

            if (springs[i] == SpringState.Damaged)
            {
                brokenSpringsSoFar += 1;
                if (brokenSpringsSoFar > groupsSum)
                {
                    return 0;
                }
            }
        }

        if (noUnkowns)
        {
            // We have a filled combination to test
            return IsCombinationValid(springs, groups) ? 1 : 0;
        }

        // There are unkowns to fill yet
        int count = 0;
        Span<SpringState> newSprings = stackalloc SpringState[springs.Length];
        springs.CopyTo(newSprings);

        checked
        {
            newSprings[firstUnkownIndex] = SpringState.Working;
            count += TryCombinations(newSprings, groups, groupsSum);

            newSprings[firstUnkownIndex] = SpringState.Damaged;
            count += TryCombinations(newSprings, groups, groupsSum);

            return count;
        }
    }

    private bool IsCombinationValid(ReadOnlySpan<SpringState> springs, IReadOnlyList<int> groups)
    {
        Span<int> generatedGroups = stackalloc int[groups.Count];
        int currentGroupIndex = -1;
        SpringState lastState = SpringState.Working; // We extend the sequence with a new . for ease of math

        for (int i = 0; i < springs.Length; i++)
        {
            var state = springs[i];
            if (lastState == SpringState.Working && state == SpringState.Damaged)
            {
                currentGroupIndex += 1;

                if (currentGroupIndex >= groups.Count) return false;
            }

            if (state == SpringState.Damaged)
            {
                generatedGroups[currentGroupIndex] += 1;
            }

            lastState = state;
        }

        if (currentGroupIndex != generatedGroups.Length - 1) return false;


        for (int i = 0; i < groups.Count; i++)
        {
            if (generatedGroups[i] != groups[i]) return false;
        }

        return true;
    }

    private List<Row> ParseInput(string path)
    {
        var stream = File.OpenText(path);
        var rows = new List<Row>();

        while (!stream.EndOfStream)
        {
            var line = stream.ReadLine()!;
            var lineSplit = line.Split(' ');

            rows.Add(new(
                lineSplit[0].Select<char, SpringState>(c => c switch
                {
                    '?' => SpringState.Unknown,
                    '#' => SpringState.Damaged,
                    '.' => SpringState.Working,
                    _ => throw new Exception()
                }).ToList(),
                lineSplit[1].Split(',').Select(x => int.Parse(x)).ToList()
            ));
        }

        return rows;
    }

    private record Row(List<SpringState> Springs, List<int> Groups);
    enum SpringState { Unknown, Working, Damaged }
}


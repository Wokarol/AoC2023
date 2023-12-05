using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace AdventOfCode;

public class Day5
{
    public void Execute()
    {
        var stream = File.OpenText("D5/input.txt");
        var (seeds, seedsPart2, maps) = ParseInput(stream);

        var stopwatch = new Stopwatch();
        stopwatch.Start();
        var mappedValues = seeds.AsEnumerable();
        foreach (var map in maps)
        {
            mappedValues = mappedValues.Select(map.MapValue);
        }
        stopwatch.Stop();
        Console.WriteLine($"Executed first part in {stopwatch.Elapsed}");

        Console.WriteLine($"[{DateTime.Now}] Preparring to process: {seedsPart2.Sum(s => s.length)}, biggest group is {seedsPart2.Max(s => s.length)}");

        stopwatch.Restart();
        var bag = new ConcurrentBag<long>();
        Parallel.ForEach(seedsPart2, (pair, state, i) =>
            {
                Console.WriteLine($"    ({i,-1}) Starting next {pair.length} seeds");

                var seedsToCheck = LongRange(pair.start, pair.length, i);
                foreach (var map in maps)
                {
                    seedsToCheck = seedsToCheck.Select(map.MapValue);
                }

                long min = seedsToCheck.Min();

                Console.WriteLine($"    ({i,-1}) Finished");
                bag.Add(min);
            });
        Console.WriteLine($"[{DateTime.Now}] Finished processing");

        stopwatch.Stop();
        Console.WriteLine($"Executed second part in {stopwatch.Elapsed}");

        Console.WriteLine($"Day  5 I : {mappedValues.Min()}");
        Console.WriteLine($"Day  5 II: {bag.Min()}");
    }


    private static (List<long> seeds, List<(long start, long length)> seedsPart2, List<Map> maps) ParseInput(StreamReader stream)
    {
        var seeds = Regex.Matches(stream.ReadLine()![7..], @"\d+")
            .Select(m => long.Parse(m.Value))
            .ToList();

        var seedsPart2 = seeds
            .Chunk(2)
            .Select(x => (start: x[0], length: x[1]))
            .ToList();

        stream.ReadLine();

        List<Map> maps = new();

        while (!stream.EndOfStream)
        {
            string title = stream.ReadLine()!;

            var map = new Map(title);
            maps.Add(map);

            while (true)
            {
                var line = stream.ReadLine()!;
                if (string.IsNullOrWhiteSpace(line))
                    break;

                var match = Regex.Match(line, @"(\d+)\s(\d+)\s(\d+)");
                map.AddMapping(long.Parse(match.Groups[1].Value), long.Parse(match.Groups[2].Value), long.Parse(match.Groups[3].Value));
            }
        }

        return (seeds, seedsPart2, maps);
    }

    private static IEnumerable<long> LongRange(long start, long length, long group)
    {
        for (long i = 0; i < length; i++)
        {
            if (i != 0 && i % 10000000 == 0) Console.WriteLine($"   [{DateTime.Now}] ({group,-1}) Processed 10 000 000 seeds ({(i / (double)length) * 100:F2}%)");

            yield return i + start;
        }
    }

    class Map
    {
        private readonly string title;
        private readonly List<Mapping> mappings = new();

        public Map(string title)
        {
            this.title = title;
        }

        public void AddMapping(long destinationStart, long sourceStart, long length)
        {
            mappings.Add(new(destinationStart, sourceStart, length));
        }

        public long MapValue(long source)
        {
            foreach (var mapping in mappings)
            {
                if (mapping.IsInRange(source)) return mapping.MapValue(source);
            }

            return source;
        }

        record struct Mapping(long DestinationStart, long SourceStart, long Lenght)
        {
            public readonly bool IsInRange(long value)
            {
                return SourceStart <= value && value < SourceStart + Lenght;
            }

            public readonly long MapValue(long value)
            {
                return DestinationStart + (value - SourceStart);
            }
        }
    }
}
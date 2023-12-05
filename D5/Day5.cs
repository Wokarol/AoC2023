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

        stopwatch.Restart();

        var bag = new ConcurrentBag<long>();
        LongClass minimalValue = new LongClass();
        minimalValue.Value = long.MaxValue;

        LongClass offset = new LongClass();
        int valuesPerThread = 10_000;
        int threads = 10;

        Parallel.For(0, threads, (i) =>
        {
            while (true)
            {
                long start;
                lock (offset)
                {
                    start = offset.Value;
                    offset.Value += valuesPerThread;
                }

                if (LookForSeed(start, valuesPerThread, i, seedsPart2, maps, bag, minimalValue))
                {
                    return;
                } 
            }
        });

        Console.WriteLine($"Finished batch (checked {offset} values)");

        Console.WriteLine($"[{DateTime.Now}] Finished processing");

        stopwatch.Stop();
        Console.WriteLine($"Executed second part in {stopwatch.Elapsed}");

        Console.WriteLine($"Day  5 I : {mappedValues.Min()}");
        Console.WriteLine($"Day  5 II: {bag.Min()}");
    }

    private static bool LookForSeed(long start, long length, long i, List<(long start, long length)> seedsPart2, List<Map> maps, ConcurrentBag<long> bag, LongClass minimalValue)
    {
        Console.WriteLine($"    ({i,2}) Trying next {length} locations starting at {start}");

        var potentialSeeds = LongRange(start, length).Select(l => (l, s: l));

        for (int j = maps.Count - 1; j >= 0; j--)
        {
            int index = j;
            potentialSeeds = potentialSeeds.Select(x => (x.l, maps[index].MapValueInverse(x.s)));
        }

        foreach (var ps in potentialSeeds)
        {
            if (ps.l > minimalValue.Value)
            {
                Console.WriteLine($"    ({i,2}) Killed");
                return true;
            }

            for (int si = 0; si < seedsPart2.Count; si++)
            {
                if (ps.s >= seedsPart2[si].start && ps.s < seedsPart2[si].start + seedsPart2[si].length)
                {
                    Console.WriteLine($"    ({i,2}) Finished");
                    bag.Add(ps.l);

                    lock (minimalValue)
                    {
                        if (minimalValue.Value > ps.l)
                        {
                            minimalValue.Value = ps.l;
                        }
                    }

                    return true;
                }
            }
        }

        return false;
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

    private static IEnumerable<long> LongRange(long start, long count)
    {
        for (long i = 0; i < count; i++)
        {
            yield return i + start;
        }
    }

    class LongClass
    {
        public long Value;
    }

    class Map
    {
        private readonly string title;
        public readonly List<Mapping> Mappings = new();

        public Map(string title)
        {
            this.title = title;
        }

        public void AddMapping(long destinationStart, long sourceStart, long length)
        {
            Mappings.Add(new(destinationStart, sourceStart, length));
        }

        public long MapValue(long source)
        {
            foreach (var mapping in Mappings)
            {
                if (mapping.IsInRange(source)) return mapping.MapValue(source);
            }

            return source;
        }

        public long MapValueInverse(long destination)
        {
            foreach (var mapping in Mappings)
            {
                if (mapping.IsInRangeInverse(destination)) return mapping.MapValueInverse(destination);
            }

            return destination;
        }

        public record struct Mapping(long DestinationStart, long SourceStart, long Lenght)
        {
            public readonly bool IsInRange(long value)
            {
                return SourceStart <= value && value < SourceStart + Lenght;

            }

            public readonly bool IsInRangeInverse(long value)
            {
                return DestinationStart <= value && value < DestinationStart + Lenght;
            }

            public readonly long MapValue(long value)
            {
                return DestinationStart + (value - SourceStart);
            }

            public readonly long MapValueInverse(long value)
            {
                return SourceStart + (value - DestinationStart);
            }

            public readonly IEnumerable<long> IterateAllDestinations()
            {
                for (long i = 0; i < Lenght; i++)
                {
                    yield return i + DestinationStart;
                }
            }
        }
    }
}
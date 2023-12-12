using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace AdventOfCode;

public class Day6
{
    public void Execute()
    {
        List<Race> races = new List<Race>
        {
            new Race(60, 475),
            new Race(94, 2138),
            new Race(78, 1015),
            new Race(82, 1650),
        };

        Race megaRace = new Race(60947882, 475213810151650L);

        var marginOfError = races
            .Select(r =>
            {
                return Enumerable.Range(0, r.Time)
                    .Count(t => (r.Time - t) * t > r.Record);
            })
            .Aggregate(1, (a, b) => a * b);

        var megaRaceMargin = Enumerable
            .Range(0, megaRace.Time)
            .Count(t =>
            {
                return (megaRace.Time - (long)t) * (long)t > megaRace.Record;
            });

        Console.WriteLine($"Day  6 I : {marginOfError}");
        Console.WriteLine($"Day  6 II: {megaRaceMargin}");
    }

    record Race(int Time, long Record);
}

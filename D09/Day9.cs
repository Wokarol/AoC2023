using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AdventOfCode;

public class Day9
{
    public void Execute()
    {
        var data = ParseInput("D09/input.txt");

        var extrapolation = data.Select(d =>
            {
                List<List<int>> derivatives = new() { d };
                while (true)
                {
                    var derivative = GetDerivative(derivatives[^1]);
                    if (derivative.All(x => x == 0))
                    {
                        var next = derivatives
                            .Sum(d => d[^1]);

                        var previous = derivatives
                            .Select(d => d[0])
                            .Select((x, i) => (i % 2 == 0) ? x : -x)
                            .Sum();

                        return (next, previous);
                    }
                    else
                    {
                        derivatives.Add(derivative);
                    }
                }
            })
            .ToList();

        Console.WriteLine($"Day  9 I : {extrapolation.Sum(x => x.next)}");
        Console.WriteLine($"Day  9 II: {extrapolation.Sum(x => x.previous)}");
    }

    private List<int> GetDerivative(List<int> source)
    {
        List<int> result = new List<int>(source.Count - 1);

        for (int i = 1; i < source.Count; i++)
        {
            result.Add(source[i] - source[i - 1]);
        }

        return result;
    }

    private List<List<int>> ParseInput(string path)
    {
        var stream = File.OpenText(path);
        var result = new List<List<int>>();

        while (!stream.EndOfStream)
        {
            var line = stream.ReadLine()!;

            result.Add(line.Split(' ').Select(x => int.Parse(x)).ToList());
        }

        return result;
    }
}

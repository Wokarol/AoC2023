using System.Diagnostics;
using System.Text.RegularExpressions;

namespace AdventOfCode;

public class Day1
{
    public void Execute()
    {
        var stream = File.OpenText("D1/input.txt");

        int sum = 0;
        int sumFull = 0;

        while (!stream.EndOfStream)
        {
            var line = stream.ReadLine();
            var firstChar = Regex.Match(line, @"\d").Value;
            var lastChar = Regex.Match(line, @"(?:^.*)(\d)").Groups[1].Value;

            var firstCharFull = Regex.Match(line, @"one|two|three|four|five|six|seven|eight|nine|\d").Value;
            var lastCharFull = Regex.Match(line, @"(?:^.*)(one|two|three|four|five|six|seven|eight|nine|\d)").Groups[1].Value;

            int firstCharFullTransformed = TransformToDigit(firstCharFull);
            int lastCharFullTransformed = TransformToDigit(lastCharFull);

            var number = int.Parse(firstChar + lastChar);
            var numberFull = firstCharFullTransformed * 10 + lastCharFullTransformed;

            sum += number;
            sumFull += numberFull;
        }

        Console.WriteLine($"Day  1 I : {sum}");
        Console.WriteLine($"Day  1 II: {sumFull}");
    }

    private static int TransformToDigit(string firstCharFull)
    {
        return firstCharFull switch
        {
            "one" => 1,
            "two" => 2,
            "three" => 3,
            "four" => 4,
            "five" => 5,
            "six" => 6,
            "seven" => 7,
            "eight" => 8,
            "nine" => 9,
            _ => int.Parse(firstCharFull)
        };
    }
}

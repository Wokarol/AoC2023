using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AdventOfCode;

public class Day15
{
    public void Execute()
    {
        var instructions = ParseInput("D15/input.txt");

        List<Lens>[] boxes = new List<Lens>[256];
        for (int i = 0; i < boxes.Length; i++)
        {
            boxes[i] = new();
        }

        foreach (var instruction in instructions)
        {
            var hash = GetHash(instruction.Label);
            var box = boxes[hash];

            var lens = box.FirstOrDefault(l => l.Label == instruction.Label);
            if (instruction.Operation == Operation.Replace)
            {
                if (lens == null)
                {
                    box.Add(new Lens(instruction.Label, instruction.Focus));
                }
                else
                {
                    lens.Focus = instruction.Focus;
                }
            }
            else
            {
                if (lens != null)
                    box.Remove(lens);
            }
        }

        var sum = boxes.Select((b, boxI) =>
        {
            return b.Select((l, lensI) =>
            {
                return (boxI + 1) * (lensI + 1) * l.Focus;
            }).Sum();
        }).Sum();

        Console.WriteLine($"Day 15 I : {instructions.Select(i => GetHash(i.Full)).Sum()}");
        Console.WriteLine($"Day 15 II: {sum}");
    }

    private Instruction[] ParseInput(string path)
    {
        var stream = File.ReadAllText(path);

        return stream.Split(',').Select(s =>
        {
            var match = Regex.Match(s, @"(\w+)([=-])(\d?)");

            var op = match.Groups[2].Value switch
            {
                "=" => Operation.Replace,
                "-" => Operation.Remove,
                _ => throw new Exception(),
            };

            return new Instruction(
                s,
                match.Groups[1].Value,
                op,
                match.Groups[3].Value.Length > 0 ? int.Parse(match.Groups[3].Value) : 0
            );
        }).ToArray();
    }

    private int GetHash(string value)
    {
        int c = 0;
        for (int i = 0; i < value.Length; i++)
        {
            c += value[i];
            c *= 17;
            c &= 255;
        }
        return c;
    }

    record class Instruction(string Full, string Label, Operation Operation, int Focus);

    class Lens
    {
        public string Label;
        public int Focus;

        public Lens(string label, int focus)
        {
            Label = label;
            Focus = focus;
        }
    }

    enum Operation { Replace, Remove }
}


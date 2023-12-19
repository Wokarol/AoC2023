using System.Collections;
using System.Collections.Concurrent;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AdventOfCode;

public class Day19
{

    public void Execute()
    {
        var (parts, workflows) = ParseInput("D19/input.txt");


        var sum = ProcessPartsNaive(parts, workflows)
            .Select(p => p['x'] + p['m'] + p['a'] + p['s'])
            .Sum();

        Console.WriteLine($"Day 19 I : {sum}");

        var sum2 = ProcessPartRanges(workflows);

        Console.WriteLine($"Day 19 II: {sum2}");
    }

    private (List<Part> parts, Dictionary<string, Workflow> workflows) ParseInput(string path)
    {
        var stream = File.OpenText(path);

        List<Part> parts = new();
        Dictionary<string, Workflow> workflows = new();

        while (!stream.EndOfStream)
        {
            while (!stream.EndOfStream)
            {
                var line = stream.ReadLine()!;

                if (string.IsNullOrWhiteSpace(line))
                    break;

                var workflowMatch = Regex.Match(line, @"(\w+){(.+?),(\w+)}");
                var rules = workflowMatch.Groups[2].Value
                    .Split(',')
                    .Select(r =>
                    {
                        var ruleMatch = Regex.Match(r, @"(\w)(.)(\d+):(\w+)");
                        return new Rule(
                            ruleMatch.Groups[1].Value[0],
                            ruleMatch.Groups[2].Value[0],
                            int.Parse(ruleMatch.Groups[3].Value),
                            ruleMatch.Groups[4].Value
                        );
                    })
                    .ToList();

                workflows.Add(
                    workflowMatch.Groups[1].Value,
                    new Workflow(
                        rules,
                        workflowMatch.Groups[3].Value
                    )
                );
            }

            while (!stream.EndOfStream)
            {
                var line = stream.ReadLine()!;
                var partMatch = Regex.Match(line, @"{x=(\d+),m=(\d+),a=(\d+),s=(\d+)}");

                parts.Add(new Part()
                {
                    { 'x', int.Parse(partMatch.Groups[1].Value) },
                    { 'm', int.Parse(partMatch.Groups[2].Value) },
                    { 'a', int.Parse(partMatch.Groups[3].Value) },
                    { 's', int.Parse(partMatch.Groups[4].Value) },
                });
            }
        }

        return (parts, workflows);
    }

    private IEnumerable<Part> ProcessPartsNaive(IEnumerable<Part> parts, Dictionary<string, Workflow> workflows)
    {
        foreach (var part in parts)
        {
            Workflow workflow = workflows["in"];

            // Iterate until we hit R or A
            while (true)
            {
                string target = workflow.FallbackTarget;

                // Check for every rule and set a new target (workflow or R/A)
                var rules = workflow.Rules;
                for (int i = 0; i < rules.Count; i++)
                {
                    var op = rules[i].Operation;

                    if (op == '>')
                    {
                        if (part[rules[i].Property] > rules[i].Threshold)
                        {
                            target = rules[i].Target;
                            break;
                        }
                    }
                    else if (op == '<')
                    {
                        if (part[rules[i].Property] < rules[i].Threshold)
                        {
                            target = rules[i].Target;
                            break;
                        }
                    }
                }

                if (target == "A")
                {
                    yield return part;
                    break;
                }
                else if (target == "R")
                {
                    break;
                }
                else
                {
                    workflow = workflows[target];
                }
            }
        }
    }

    private long ProcessPartRanges(Dictionary<string, Workflow> workflows)
    {
        var all = new PartRanges(1, 4000, 1, 4000, 1, 4000, 1, 4000);
        var workflow = workflows["in"];

        return ProcessRangeRecursively(all, workflow, workflows);
    }

    private long ProcessRangeRecursively(PartRanges ranges, Workflow workflow, Dictionary<string, Workflow> workflows)
    {
        var current = ranges;
        var accepted = 0L;
        foreach (var rule in workflow.Rules)
        {
            var op = rule.Operation;

            (var partsToTake, current) = (rule.Property, op) switch
            {
                ('x', '>') => (current with { XLower = rule.Threshold + 1 }, current with { XUpper = rule.Threshold }),
                ('x', '<') => (current with { XUpper = rule.Threshold - 1 }, current with { XLower = rule.Threshold }),
                ('m', '>') => (current with { MLower = rule.Threshold + 1 }, current with { MUpper = rule.Threshold }),
                ('m', '<') => (current with { MUpper = rule.Threshold - 1 }, current with { MLower = rule.Threshold }),
                ('a', '>') => (current with { ALower = rule.Threshold + 1 }, current with { AUpper = rule.Threshold }),
                ('a', '<') => (current with { AUpper = rule.Threshold - 1 }, current with { ALower = rule.Threshold }),
                ('s', '>') => (current with { SLower = rule.Threshold + 1 }, current with { SUpper = rule.Threshold }),
                ('s', '<') => (current with { SUpper = rule.Threshold - 1 }, current with { SLower = rule.Threshold }),
                _ => throw new Exception()
            };

            if (partsToTake.Count != 0)
            {
                // There is something matching the rule
                var target = rule.Target;

                if (target == "A")
                {
                    // The parts are accepted by this rule, so we just add them to our total for this workflow
                    accepted += partsToTake.Count;
                }
                else if (target == "R")
                {
                    // The parts are rejected, so we do not care
                }
                else
                {
                    // The parts get a new worklow
                    var newWorkflow = workflows[target];
                    accepted += ProcessRangeRecursively(partsToTake, newWorkflow, workflows);
                }
            }

            if (current.Count == 0)
            {
                // There is no more parts left to traverse this workflow's rules
                break;
            }
        }

        var fallback = workflow.FallbackTarget;

        if (fallback == "R" || current.Count == 0)
        {
            // No parts fell into the fallback or we do not accept leftover parts, so we return the total
            return accepted;
        }

        if (fallback == "A")
        {
            // The leftover are accepted by this rule, so we just add them to our total for this workflow and exit the method
            return current.Count + accepted;
        }
        else
        {
            // The leftover parts get a new worklow
            var newWorkflow = workflows[fallback];
            return accepted + ProcessRangeRecursively(current, newWorkflow, workflows);
        }
    }

    class Part : Dictionary<char, int> { }
    record struct PartRanges(int XLower, int XUpper, int MLower, int MUpper, int ALower, int AUpper, int SLower, int SUpper)
    {
        public readonly long Count
        {
            get
            {
                checked
                {
                    return (long)(XUpper - XLower + 1) * (MUpper - MLower + 1) * (AUpper - ALower + 1) * (SUpper - SLower + 1);
                }
            }
        }
    }

    record Workflow(List<Rule> Rules, string FallbackTarget);
    record Rule(char Property, char Operation, int Threshold, string Target);
}


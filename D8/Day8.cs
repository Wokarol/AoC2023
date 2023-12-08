using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AdventOfCode;

public class Day8
{
    public void Execute()
    {
        var (moves, nodes) = ParseInput("D8/input.txt");

        var nodesByName = nodes.ToDictionary(n => n.Name);

        int steps1 = 0; // Part1(moves, nodesByName);
        int steps2 = Part2(moves, nodes, nodesByName);

        Console.WriteLine($"Day  8 I : {steps1}");
        Console.WriteLine($"Day  8 II: {steps2}");
    }

    private static int Part1(List<Move> moves, Dictionary<string, Node> nodesByName)
    {
        Queue<Move> movesQueue = new Queue<Move>(moves);
        int steps = 0;
        string currentNode = "AAA";

        while (true)
        {
            steps++;
            var move = movesQueue.Dequeue();
            movesQueue.Enqueue(move);

            var current = nodesByName[currentNode];
            currentNode = move switch
            {
                Move.Left => current.Left,
                Move.Right => current.Right,
                _ => throw new Exception(),
            };

            if (currentNode == "ZZZ")
                break;
        }

        return steps;
    }

    private static int Part2(List<Move> moves, List<Node> nodes, Dictionary<string, Node> nodesByName)
    {
        Queue<Move> movesQueue = new Queue<Move>(moves);
        int steps = 0;
        var ghosts = nodes.Where(n => n.Name[2] == 'A').Select(n => new Ghost(nodesByName, n.Name)).ToList();

        Console.WriteLine($"Started {ghosts.Count} ghosts");

        while (true)
        {
            steps++;
            var move = movesQueue.Dequeue();
            movesQueue.Enqueue(move);

            bool allGhostsFinished = true;

            for (int i = 0; i < ghosts.Count; i++)
            {
                var finished = ghosts[i].MakeAStep(move);

                if (!finished)
                    allGhostsFinished = false;
            }

            if (steps % 10_000_000 == 0)
                Console.WriteLine($"Finished next 10 000 000 steps, currently on: {steps}");

            if (allGhostsFinished)
                break;
        }

        return steps;
    }

    private (List<Move>, List<Node>) ParseInput(string path)
    {
        List<Node> nodes = new();
        List<Move> moves;
        var stream = File.OpenText(path);

        var firstLine = stream.ReadLine()!;
        moves = firstLine.Select(c => c switch
        {
            'L' => Move.Left,
            'R' => Move.Right,
            _ => throw new Exception()
        }).ToList();

        stream.ReadLine();

        while (!stream.EndOfStream)
        {
            var line = stream.ReadLine()!;
            var match = Regex.Match(line, @"(\w{3})\s=\s\((\w{3}),\s(\w{3})\)");

            nodes.Add(new(match.Groups[1].Value, match.Groups[2].Value, match.Groups[3].Value));
        }

        return (moves, nodes);
    }

    class Ghost
    {
        private readonly Dictionary<string, Node> nodesByName;
        private string currentNode;

        public Ghost(Dictionary<string, Node> nodes, string startingNode)
        {
            this.nodesByName = nodes;
            this.currentNode = startingNode;
        }

        public bool MakeAStep(Move move)
        {
            var current = nodesByName[currentNode];
            currentNode = move switch
            {
                Move.Left => current.Left,
                Move.Right => current.Right,
                _ => throw new Exception(),
            };

            if (currentNode[2] == 'Z')
                return true;

            return false;
        }
    }

    record struct Node(string Name, string Left, string Right);
    enum Move { Left, Right }
}

using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AdventOfCode;

public class Day8
{
    public void Execute()
    {
        var (moves, nodes) = ParseInput("D08/input.txt");

        var nodesByName = nodes.ToDictionary(n => n.Name);

        int steps1 = Part1(moves, nodesByName);
        BigInteger steps2 = Part2(moves, nodes, nodesByName);

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

    private static BigInteger Part2(List<Move> moves, List<Node> nodes, Dictionary<string, Node> nodesByName)
    {
        var ghosts = nodes.Where(n => n.Name[2] == 'A').Select(n => new Ghost(nodesByName, n.Name)).ToList();
        var loopCounts = new int[ghosts.Count];

        for (int i = 0; i < ghosts.Count; i++)
        {
            var g = ghosts[i];

            int loops = 0;
            while (true)
            {
                loops++;

                for (int j = 0; j < moves.Count; j++)
                {
                    var move = moves[j];
                    g.MakeAStep(move);
                }

                if (g.Current[2] == 'Z')
                {
                    break;
                }
            }

            loopCounts[i] = loops;
        }

        checked
        {
            return loopCounts
                .Skip(1)
                .Select(x => new BigInteger(x))
                .Aggregate(new BigInteger(loopCounts[0]), (a, b) => lcm(a, b))
                * moves.Count;
        }
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

        public string Current => currentNode;

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

    static BigInteger gcf(BigInteger a, BigInteger b)
    {
        checked
        {
            while (b != 0)
            {
                BigInteger temp = b;
                b = a % b;
                a = temp;
            }
        }
        return a;
    }

    static BigInteger lcm(BigInteger a, BigInteger b)
    {
        checked
        {
            return (a / gcf(a, b)) * b; 
        }
    }

    record struct Node(string Name, string Left, string Right);
    enum Move { Left, Right }
}

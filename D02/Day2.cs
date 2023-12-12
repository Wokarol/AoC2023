using System.Diagnostics;
using System.Text.RegularExpressions;

namespace AdventOfCode;

public class Day2
{
    public void Execute()
    {
        var stream = File.OpenText("D02/input.txt");
        var games = ParseInput(stream);

        var validGames = games.Where(g =>
        {
            foreach (var r in g.Rounds)
            {
                if (r.R > 12) return false;
                if (r.G > 13) return false;
                if (r.B > 14) return false;
            }

            return true;
        });

        var gamesPower = games.Select(g =>
        {
            var maxR = g.Rounds.Max(r => r.R);
            var maxG = g.Rounds.Max(r => r.G);
            var maxB = g.Rounds.Max(r => r.B);

            return maxR * maxG * maxB;
        });

        Console.WriteLine($"Day  2 I : {validGames.Sum(g => g.Id)}");
        Console.WriteLine($"Day  2 II: {gamesPower.Sum()}");
    }

    private static List<Game> ParseInput(StreamReader stream)
    {
        var games = new List<Game>();

        while (!stream.EndOfStream)
        {
            var line = stream.ReadLine() ?? throw new NullReferenceException();
            var idAndRounds = Regex.Match(line, @"Game\s(\d+):\s+(.+)");
            var id = int.Parse(idAndRounds.Groups[1].Value);

            var g = new Game(id);
            games.Add(g);

            foreach (var roundRaw in idAndRounds.Groups[2].Value.Split(';'))
            {
                Round round = new();
                g.Rounds.Add(round);

                var cubes = Regex.Matches(roundRaw, @"(\d+)\s(\w+)");

                foreach (var c in cubes.Select(x => x))
                {
                    var number = int.Parse(c.Groups[1].Value);
                    var color = c.Groups[2].Value;

                    switch (color)
                    {
                        case "red":
                            round.R = number;
                            break;

                        case "green":
                            round.G = number;
                            break;

                        case "blue":
                            round.B = number;
                            break;
                    }
                }
            }
        }

        return games;
    }

    class Game
    {
        public readonly int Id;
        public readonly List<Round> Rounds = new();

        public Game(int id)
        {
            Id = id;
        }
    }

    class Round
    {
        public int R;
        public int G;
        public int B;
    }
}
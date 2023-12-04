using System.Collections;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace AdventOfCode;

public class Day4
{
    public void Execute()
    {
        var stream = File.OpenText("D4/input.txt");
        var cards = ParseInput(stream);

        var scores = cards.Select(c =>
            {
                int overlap = FindOverlap(c);
                return OverlapToScore(overlap);
            });

        var cachedCards = cards.Select(c =>
            {
                return new CachedCard(FindOverlap(c));
            })
            .ToList();

        var cardCounters = Enumerable.Repeat(1, cachedCards.Count).ToArray();

        for (int i = 0; i < cachedCards.Count; i++)
        {
            var howManyCards = cardCounters[i];
            var howManyWins = cachedCards[i].Overlap;
            var cardsLeft = cachedCards.Count - i - 1;

            for (int j = 0; j < Math.Min(howManyWins, cardsLeft); j++)
            {
                cardCounters[j + i + 1] += howManyCards;
            }
        }


        Console.WriteLine($"Day  4 I : {scores.Sum()}");
        Console.WriteLine($"Day  4 II: {cardCounters.Sum()}");
    }

    private static int FindOverlap(Card c)
    {
        return c.YourNumbers.Intersect(c.WinningNumbers).Count();
    }

    private static int OverlapToScore(int overlap)
    {
        return overlap == 0 
            ? 0 
            : 1 << overlap - 1;
    }

    private List<Card> ParseInput(StreamReader stream)
    {
        List<Card> cards = new List<Card>();

        while (!stream.EndOfStream)
        {
            var line = stream.ReadLine() ?? throw new NullReferenceException();

            var match = Regex.Match(line, @"Card\s+\d+?:([\d\s]+)\|([\d\s]+)");

            var winningNumbers = Regex.Matches(match.Groups[1].Value, @"\d+")
                .Select(m => int.Parse(m.Value))
                .ToList();

            var yourNumbers = Regex.Matches(match.Groups[2].Value, @"\d+")
                .Select(m => int.Parse(m.Value))
                .ToList();

            cards.Add(new(winningNumbers, yourNumbers));
        }

        return cards;
    }

    record Card(List<int> WinningNumbers, List<int> YourNumbers);
    record CachedCard(int Overlap);
}
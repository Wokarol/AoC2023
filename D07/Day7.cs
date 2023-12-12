using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace AdventOfCode;

public class Day7
{
    public void Execute()
    {
        var hands = ParseInput("D07/input.txt");

        var orderedHands = hands
            .OrderByDescending(h => (10 - h.type, h.normalizedCards));

        var part1 = orderedHands
            .Select((h, i) => h.bid * (i + 1))
            .Sum();

        var orderedHandsAlt = hands
            .OrderByDescending(h => (10 - h.typeAlternative, h.normalizedCardsAlternative));

        var part2 = orderedHandsAlt
            .Select((h, i) => h.bid * (i + 1))
            .Sum();

        Console.WriteLine($"Day  7 I : {part1}");
        Console.WriteLine($"Day  7 II: {part2}");
    }

    private List<Hand> ParseInput(string path)
    {
        List<Hand> result = new();
        var stream = File.OpenText(path);

        while (!stream.EndOfStream)
        {
            var line = stream.ReadLine()!;
            // ABCD5 183

            var match = Regex.Match(line, @"(.{5})\s(\d+)");

            string cards = match.Groups[1].Value;
            int bid = int.Parse(match.Groups[2].Value);

            // AKQJT98765432 -> ABCDEF...
            string normalizedCards = new string(cards.Select(x => x switch {
                'A' => 'A',
                'K' => 'B',
                'Q' => 'C',
                'J' => 'D',
                'T' => 'E',
                >= '2' and <= '9' => (char)('F' + 7 - (x - '2')),
                _ => '0'
            }).ToArray());

            string normalizedCardsAlternative = new string(cards.Select(x => x switch {
                'A' => 'A',
                'K' => 'B',
                'Q' => 'C',
                'T' => 'D',
                >= '2' and <= '9' => (char)('E' + 7 - (x - '2')),
                'J' => 'N',
                _ => '0'
            }).ToArray());

            result.Add(new(
                cards: cards,
                bid: bid,
                type: GetHandType(cards),
                typeAlternative: GetHandTypeAlternative(cards),
                normalizedCards: normalizedCards,
                normalizedCardsAlternative: normalizedCardsAlternative
            ));
        }
        
        return result;
    }

    private int GetHandType(string cards)
    {
        var groups = cards
            .GroupBy(c => c)
            .Select(g => (card: g.Key, count: g.Count()))
            .OrderByDescending(p => p.count)
            .ToList();

        if (groups.Count == 1) 
            return 6; // AAAAA

        if (groups.Count == 2)
        {
            if (groups[0].count == 4) return 5; // AAAAB
            if (groups[0].count == 3) return 4; // AAABB
        }

        if (groups.Count == 3)
        {
            if (groups[0].count == 3) return 3; // AAABC
            if (groups[0].count == 2 && groups[1].count == 2) return 2; // AABBC
        }

        if (groups.Count == 4)
        {
            if (groups[0].count == 2) return 1; // AABCD
        }

        if (groups.Count == 5)
            return 0; // ABCDEF

        throw new Exception();
    }

    private int GetHandTypeAlternative(string cards)
    {
        var jokers = cards.Count(c => c == 'J');

        if (jokers == 0) return GetHandType(cards);

        var groups = cards
            .Where(c => c != 'J')
            .GroupBy(c => c)
            .Select(g => (card: g.Key, count: g.Count()))
            .OrderByDescending(p => p.count)
            .ToList();

        if (groups.Count == 1 || jokers == 5)
            return 6; // AAAAA

        if (groups.Count == 2)
        {
            if (groups[0].count + jokers == 4) return 5; // AAABJ /  ABBJJ / ABJJJ
            if (groups[0].count + jokers == 3) return 4; // AABBJ 
        }

        if (groups.Count == 3) // ABCJJ / AABCJ
        {
            if (groups[0].count + jokers == 3) return 3; // ABCJJ
        }

        if (groups.Count == 4) // ABCDJ
            return 1;

        throw new Exception();
    }

    record struct Hand(string cards, int bid, int type, int typeAlternative, string normalizedCards, string normalizedCardsAlternative);
}

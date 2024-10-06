using System;
using System.Collections.Generic;
using System.Linq;

public static class HandEvaluator
{
    public static CardSet EvaluateHand(IReadOnlyList<Card> hand)
    {
        if (hand == null || hand.Count < 5)
            return CardSet.None;

        var groupedHand = hand.GroupBy(c => c.Value)
                              .OrderByDescending(g => g.Count())
                              .ThenByDescending(g => g.Key)
                              .ToList();

        var isFlush = IsFlush(hand);
        var isStraight = IsStraight(hand);

        // Straight flush: גם רצף וגם פלוש
        if (isFlush && isStraight)
            return CardSet.StraightFlush;

        // Four of a kind: 4 קלפים בעלי אותו ערך
        if (groupedHand[0].Count() == 4)
            return CardSet.FourOfAKind;

        // Full house: 3 קלפים זהים ו-2 קלפים זהים נוספים
        if (groupedHand[0].Count() == 3 && groupedHand[1].Count() == 2)
            return CardSet.FullHouse;

        // Flush: כל הקלפים הם מאותו צבע, לא חשוב מה הערך
        if (isFlush)
            return CardSet.Flush;

        // Straight: סדרת קלפים עוקבת
        if (isStraight)
            return CardSet.Straight;

        // Three of a kind: 3 קלפים זהים
        if (groupedHand[0].Count() == 3)
            return CardSet.ThreeOfAKind;

        // Two pair: שני זוגות קלפים זהים
        if (groupedHand[0].Count() == 2 && groupedHand[1].Count() == 2)
            return CardSet.TwoPair;

        // Pair: זוג קלפים זהים
        if (groupedHand[0].Count() == 2)
            return CardSet.Pair;

        return CardSet.None;  // ברירת המחדל במידה ולא נמצאה יד
    }

    private static bool IsFlush(IEnumerable<Card> hand)
    {
        return hand.GroupBy(c => c.Color).Any(g => g.Count() >= 5);
    }

    private static bool IsStraight(IReadOnlyList<Card> hand)
    {
        var distinctValues = hand.Select(c => (int)c.Value)
                                 .Distinct()
                                 .OrderBy(v => v)
                                 .ToList();

        if (distinctValues.Count < 5)
            return false;

        // בדיקה עבור רצף רגיל
        for (int i = 0; i <= distinctValues.Count - 5; i++)
        {
            if (IsConsecutive(distinctValues.GetRange(i, 5)))
                return true;
        }

        // בדיקה עבור רצף Ace-low (למשל A-2-3-4-5)
        return distinctValues.Contains((int)CardValue.Ace) &&
               distinctValues.Contains(2) &&
               distinctValues.Contains(3) &&
               distinctValues.Contains(4) &&
               distinctValues.Contains(5);
    }

    private static bool IsConsecutive(IReadOnlyList<int> values)
    {
        for (int i = 1; i < values.Count; i++)
        {
            if (values[i] - values[i - 1] != 1)
                return false;
        }
        return true;
    }
}

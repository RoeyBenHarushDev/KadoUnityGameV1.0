using System;
using UnityEngine;

public enum CardColor { Green, Pink, Blue, Orange }
public enum CardValue { Ace, Two, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Queen, King }
public enum CardSet { None, Pair, TwoPair, ThreeOfAKind, Straight, Flush, FullHouse, FourOfAKind, StraightFlush }
public enum SpecialCardType { None, Reveal, Skip, DoubleSwap, Joker }

public class Card : MonoBehaviour
{
    [SerializeField] private CardColor color;
    [SerializeField] private CardValue value;
    [SerializeField] private bool isSpecial;
    [SerializeField] private SpecialCardType specialType;

    public CardColor Color => color;
    public CardValue Value => value;
    public bool IsSpecial => isSpecial;
    public SpecialCardType SpecialType => specialType;

    // פונקציה לאתחול הקלף
    public void Initialize(CardColor color, CardValue value, SpecialCardType specialType = SpecialCardType.None)
    {
        this.color = color;
        this.value = value;
        this.specialType = specialType;
        this.isSpecial = specialType != SpecialCardType.None;
    }

    // פונקציה סטטית ליצירת קלף משם הספירייט
    public static Card CreateFromSpriteName(string spriteName)
    {
        SpecialCardType specialType = SpecialCardType.None;
        CardColor specialCardColor = CardColor.Blue;  // צבע ברירת מחדל עבור קלפים מיוחדים
        CardValue specialCardValue = CardValue.Ten;   // ערך ברירת מחדל עבור קלפים מיוחדים

        // זיהוי קלפים מיוחדים לפי שם הספירייט
        if (spriteName.StartsWith("Reveal"))
        {
            specialType = SpecialCardType.Reveal;
            specialCardColor = CardColor.Pink;  // צבע מותאם לקלף Reveal
            specialCardValue = CardValue.Five;
        }
        else if (spriteName.StartsWith("Skip"))
        {
            specialType = SpecialCardType.Skip;
            specialCardColor = CardColor.Orange;  // צבע מותאם לקלף Skip
            specialCardValue = CardValue.Two;
        }
        else if (spriteName.StartsWith("DoubleSwap"))
        {
            specialType = SpecialCardType.DoubleSwap;
            specialCardColor = CardColor.Green;  // צבע מותאם לקלף DoubleSwap
            specialCardValue = CardValue.Seven;
        }
        else if (spriteName.StartsWith("Joker"))
        {
            specialType = SpecialCardType.Joker;
            specialCardColor = CardColor.Blue;  // צבע מותאם לקלף Joker
            specialCardValue = CardValue.Jack;
        }

        // אם הקלף הוא מיוחד, ניצור אותו עם הצבע והערך המותאמים
        if (specialType != SpecialCardType.None)
        {
            Card card = new GameObject("SpecialCard").AddComponent<Card>();
            card.Initialize(specialCardColor, specialCardValue, specialType);
            return card;
        }
        else
        {
            // קלף רגיל
            string[] parts = spriteName.Split('_');
            if (parts.Length != 2)
            {
                Debug.LogError($"Invalid sprite name format: {spriteName}. Expected format is 'Color_Value'.");
                return null;
            }

            try
            {
                // הפקת הצבע והערך מהשם
                CardColor color = (CardColor)Enum.Parse(typeof(CardColor), parts[0], true);
                CardValue value = (CardValue)Enum.Parse(typeof(CardValue), parts[1], true);

                // יצירת קלף רגיל
                Card card = new GameObject("NormalCard").AddComponent<Card>();
                card.Initialize(color, value);
                return card;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to parse sprite name: {spriteName}. Error: {e.Message}");
                return null;
            }
        }
    }
    // פונקציה סטטית ליצירת קלף ממחרוזת
    public static Card CreateFromString(string cardInfo)
    {
        // חלוקה של המחרוזת למרכיבים
        string[] parts = cardInfo.Split('_');

        if (parts.Length < 2)
        {
            Debug.LogError($"Invalid card string format: {cardInfo}. Expected format is 'Color_Value' or 'Color_Value_SpecialType'.");
            return null;
        }

        try
        {
            // ניתוח הצבע והערך מתוך המחרוזת
            CardColor color = (CardColor)Enum.Parse(typeof(CardColor), parts[0], true);
            CardValue value = (CardValue)Enum.Parse(typeof(CardValue), parts[1], true);

            // בדיקה אם הקלף הוא מיוחד
            SpecialCardType specialType = SpecialCardType.None;
            if (parts.Length == 3)
            {
                specialType = (SpecialCardType)Enum.Parse(typeof(SpecialCardType), parts[2], true);
            }

            // יצירת הקלף
            Card card = new GameObject("CardFromString").AddComponent<Card>();
            card.Initialize(color, value, specialType);
            return card;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to create card from string: {cardInfo}. Error: {e.Message}");
            return null;
        }
    }


    // הפונקציה שמחזירה את תיאור הקלף
    public override string ToString()
    {
        return $"{Color}_{Value}" + (SpecialType != SpecialCardType.None ? $"_{SpecialType}" : "");
    }
}

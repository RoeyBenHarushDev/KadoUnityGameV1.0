using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AIPlayer
{
    private List<Card> hand;
    private CardManager gameManager;
    private List<Card> revealedPlayerCards;

    public AIPlayer(CardManager manager)
    {
        gameManager = manager;
        hand = new List<Card>();
        revealedPlayerCards = new List<Card>();
    }

    public void PlayTurn()
    {
        CardSet currentHandStrength = HandEvaluator.EvaluateHand(hand);
        if (currentHandStrength <= CardSet.Pair)
        {
            if (Random.value < 0.7f)
            {
                DrawCardAndDiscard();
            }
            else
            {
                gameManager.ProposeTradeForAI();
            }
        }
        else if (currentHandStrength <= CardSet.ThreeOfAKind)
        {
            if (Random.value < 0.5f)
            {
                DrawCardAndDiscard();
            }
            else
            {
                gameManager.ProposeTradeForAI();
            }
        }
        else
        {
            if (Random.value < 0.2f)
            {
                DrawCardAndDiscard();
            }
            else
            {
                gameManager.ProposeTradeForAI();
            }
        }
        UseSpecialCardIfPossible();
    }

    private void DrawCardAndDiscard()
    {
        gameManager.DrawCardForAI();
        if (hand.Count > 7)
        {
            Card cardToDiscard = ChooseCardToDiscard();
            DiscardCard(cardToDiscard);
        }
    }

    private Card ChooseCardToDiscard()
    {
        var cardGroups = hand.GroupBy(c => c.Value).OrderBy(g => g.Key);
        foreach (var group in cardGroups)
        {
            if (group.Count() == 1)
            {
                return group.First();
            }
        }
        return hand.OrderBy(c => c.Value).First();
    }

    private void DiscardCard(Card card)
    {
        int cardIndex = hand.IndexOf(card);
        if (cardIndex != -1)
        {
            hand.RemoveAt(cardIndex);
            gameManager.DiscardAICard(cardIndex);
        }
    }

    private void UseSpecialCardIfPossible()
    {
        var specialCard = hand.FirstOrDefault(c => c.SpecialType != SpecialCardType.None);
        if (specialCard != null)
        {
            switch (specialCard.SpecialType)
            {
                case SpecialCardType.Reveal:
                    UseRevealCard();
                    break;
                case SpecialCardType.Skip:
                    gameManager.UseSkipCard();
                    break;
                case SpecialCardType.DoubleSwap:
                    UseDoubleSwapCard();
                    break;
                case SpecialCardType.Joker:
                    UseJokerCard(specialCard);  // מעבירים את specialCard לפונקציה
                    break;
            }
            hand.Remove(specialCard);
        }
    }

    private void UseJokerCard(Card jokerCard)
    {
        var handWithoutJoker = hand.Where(c => c.SpecialType != SpecialCardType.Joker).ToList();
        var bestSet = HandEvaluator.EvaluateHand(handWithoutJoker);
        CardValue bestValue = CardValue.Ace;
        for (CardValue value = CardValue.Ace; value <= CardValue.King; value++)
        {
            var testHand = new List<Card>(handWithoutJoker)
            {
                gameManager.CreateCard(CardColor.Green, value)
            };
            var testSet = HandEvaluator.EvaluateHand(testHand);
            if (testSet > bestSet)
            {
                bestSet = testSet;
                bestValue = value;
            }
        }
    }

    public void UseRevealCard()
    {
        gameManager.UseRevealCard(null);
        AnalyzeRevealedCards();
    }

    private void UseDoubleSwapCard()
    {
        List<Card> cardsToSwap = ChooseCardsForDoubleSwap();
        gameManager.UseDoubleSwapCard(cardsToSwap);
    }

    private List<Card> ChooseCardsForDoubleSwap()
    {
        return hand.OrderBy(c => GetCardValue(c)).Take(2).ToList();
    }

    private int GetCardValue(Card card)
    {
        return (int)card.Value + (int)card.Color * 13;
    }

    private void AnalyzeRevealedCards()
    {
        CardSet opponentHandStrength = HandEvaluator.EvaluateHand(revealedPlayerCards);
        CardSet aiHandStrength = HandEvaluator.EvaluateHand(hand);

        if (opponentHandStrength > aiHandStrength)
        {
            // אם היד של היריב חזקה יותר, נסה לשפר את היד שלנו
            if (Random.value < 0.7f)
            {
                DrawCardAndDiscard();
            }
            else
            {
                gameManager.ProposeTradeForAI();
            }
        }
        else if (opponentHandStrength < aiHandStrength)
        {
            // אם היד שלנו חזקה יותר, נשמור על הקלפים שלנו ונשתמש בקלף מיוחד אם אפשר
            UseSpecialCardIfPossible();
        }
        else
        {
            // אם היד שווה, ננסה לשפר את היד שלנו בזהירות
            if (Random.value < 0.5f)
            {
                DrawCardAndDiscard();
            }
            else
            {
                UseSpecialCardIfPossible();
            }
        }
    }

    public void UpdateHand(List<Card> newHand)
    {
        hand = newHand;
    }

    public void SetRevealedPlayerCards(List<Card> cards)
    {
        revealedPlayerCards = cards;
    }
}

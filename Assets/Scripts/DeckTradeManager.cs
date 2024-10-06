using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class DeckTradeManager : MonoBehaviour
{
    public GameObject deckTradePanel; // הפאנל להצגת הקלפים
    public GameObject cardPrefab; // Prefab של קלף
    public Transform deckTradeParent; // האזור בתוך הפאנל שבו יוצגו הקלפים
    public List<GameObject> playerCards; // רשימת הקלפים של השחקן
    public List<Sprite> deckCards; // קלפים מהחפיסה
    private Sprite selectedDeckCard; // הקלף שנבחר מהחפיסה

    void Start()
    {
        // הסתרת הפאנל בהתחלה
        deckTradePanel.SetActive(false);
    }

    // פונקציה לפתיחת הפאנל ולהצגת הקלפים של השחקן
    public void OpenDeckTradePanel()
    {
        // מחיקה של קלפים קודמים בפאנל
        foreach (Transform child in deckTradeParent)
        {
            Destroy(child.gameObject);
        }

        // יצירת הקלפים של השחקן בפאנל
        foreach (var playerCard in playerCards)
        {
            GameObject cardButton = Instantiate(cardPrefab, deckTradeParent);
            cardButton.GetComponent<Image>().sprite = playerCard.GetComponent<Image>().sprite;

            // הגדרת פעולה של החלפת הקלף
            cardButton.GetComponent<Button>().onClick.AddListener(() => TradeCard(playerCard));
        }

        // הצגת הפאנל
        deckTradePanel.SetActive(true);
    }

    // פונקציה לסגירת הפאנל
    public void CloseDeckTradePanel()
    {
        deckTradePanel.SetActive(false);
    }

    // פונקציה להחלפת הקלף של השחקן עם הקלף מהחפיסה
    public void TradeCard(GameObject playerCard)
    {
        if (selectedDeckCard != null)
        {
            // החלפת הקלף בין השחקן לחפיסה
            Sprite tempSprite = playerCard.GetComponent<Image>().sprite;
            playerCard.GetComponent<Image>().sprite = selectedDeckCard;
            selectedDeckCard = tempSprite;

            // סגירת הפאנל אחרי ההחלפה
            CloseDeckTradePanel();
        }
    }

    // פונקציה לבחירת הקלף מהחפיסה
    public void SelectDeckCard(Sprite deckCard)
    {
        selectedDeckCard = deckCard;
    }
}

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using System.Linq;

public class CardManager : MonoBehaviour
{
    public List<Sprite> cardSprites;
    public GameObject cardPrefab;
    public GameObject opponentCardPrefab;
    public Transform playerHandParent;
    public Transform opponentHandParent;
    public GameObject tradePopupPanel;
    public GameObject restartConfirmPanel;
    public Transform tradePopupParent;
    public Button confirmButton;
    public Button cancelButton;
    public Button drawCardButton;
    public Button endTurnButton;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI statusText;
    public Sprite cardBackSprite;
    public Vector2 panelCardSize = new Vector2(260, 320);
    public float cardSpacing = 5f;
    public Canvas mainCanvas;
    public TextMeshProUGUI playerHandStrengthText;
    public TextMeshProUGUI deckCountText;
    public TextMeshProUGUI turnCountText;
    public TextMeshProUGUI turnDisplayText;
    public GameObject swapPanel;
    public Transform swapPanelContent;
    public Button confirmSwapButton;
    public Button closeSwapPanelButton;
    public GameObject revealPopup;
    public Button confirmRevealButton;
    public Button cancelRevealButton;
    public int playerWins = 0;
    public int opponentWins = 0;
    public TextMeshProUGUI playerWinsText;
    public TextMeshProUGUI opponentWinsText;
    public GameObject roundEndPanel;  // Panel שמכריז על המנצח בסיבוב
    public TextMeshProUGUI roundEndText;
    public Button nextRoundButton;
    public GameObject finalWinPanel;  // Panel שמכריז על המנצח הסופי
    public TextMeshProUGUI finalWinText;
    public int maxWins = 3;


    public List<GameObject> playerCards = new List<GameObject>();
    public List<GameObject> opponentCards = new List<GameObject>();
    public static CardManager Instance { get; private set; } // Singleton

    private bool isPlayerTurn = true;
    private List<Sprite> shuffledDeck;
    private List<Card> cardsInPlay = new List<Card>();
    private GameObject selectedPlayerCard;
    private GameObject revealCardUsed;
    private GameObject currentRevealCard;
    private GameObject selectedOpponentCard;
    private List<Card> tempRevealedCards = new List<Card>();
    private bool tradeAccepted = false;
    private Coroutine timerCoroutine;
    private const float tradeTime = 10f;
    private List<Transform> originalCardParents = new List<Transform>();
    private List<Vector2> originalCardSizes = new List<Vector2>();
    private List<GameObject> selectedCardsForSwap = new List<GameObject>();
    private GameObject doubleSwapCardUsed;
    private bool mustDiscardCard = false;
    private GameObject selectedJokerCard;
    private int turnCount = 0;
    private AIPlayer aiPlayer;
    private int playerTurns = 0;
    private int opponentTurns = 0;
    private int maxTurns = 5; // 5 turns for each player



    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        tradePopupPanel.SetActive(false);
        confirmButton.onClick.AddListener(ConfirmTrade);
        cancelButton.onClick.AddListener(CancelTrade);
        drawCardButton.onClick.AddListener(DrawCardFromDeck);
        endTurnButton.onClick.AddListener(EndTurn);
        if (cardSprites == null || cardSprites.Count == 0)
        {
            Debug.LogError("cardSprites is not assigned or is empty in the Inspector.");
            return;
        }
        nextRoundButton.onClick.AddListener(StartNewRound);
        roundEndPanel.SetActive(false);
        finalWinPanel.SetActive(false);
        ShuffleDeck();
        DealInitialCards();
        UpdateButtonStates();
        UpdateTurnDisplay();
        SetupRevealButtons();
        aiPlayer = new AIPlayer(this);
        tempRevealedCards = new List<Card>();
        UpdateUI();
    }

    void Update()
    {
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began || Input.GetMouseButtonDown(0))
        {
            Vector2 touchPosition = Input.touchCount > 0 ? Input.GetTouch(0).position : (Vector2)Input.mousePosition;
            if (!IsPointerOverUIObject(touchPosition))
            {
                DeselectCard();
            }
        }
    }

    private void UpdateTurnDisplay()
    {
        turnDisplayText.text = isPlayerTurn ? "Your Turn" : "Opponent's Turn";
    }

    public void UpdateUI()
    {
        UpdateScores();
        UpdateHandStrengths();
        UpdateDeckCount();
        UpdateTurnCount();
    }

    private void UpdateHandStrengths()
    {
        // Check if the TextMeshProUGUI fields are assigned
        if (playerHandStrengthText == null)
        {
            Debug.LogError("playerHandStrengthText is not assigned.");
            return;
        }
        // Check if the player and opponent card lists are initialized
        if (playerCards == null || playerCards.Count == 0)
        {
            Debug.LogError("Player cards are null or empty.");
            return;
        }

        if (opponentCards == null || opponentCards.Count == 0)
        {
            Debug.LogError("Opponent cards are null or empty.");
            return;
        }

        // Proceed to evaluate hand strengths
        CardSet playerStrength = HandEvaluator.EvaluateHand(ConvertToCardList(playerCards));

        playerHandStrengthText.text = $"Hand: {playerStrength}";
    }

    private void StartNewRound()
    {
        roundEndPanel.SetActive(false); // הסתרת הפאנל של סיום הסיבוב
        ResetGame(); // איפוס המשחק לסיבוב חדש
    }


    public void UpdateWinText()
    {
        playerWinsText.text = $"Player Wins: {playerWins}";
        opponentWinsText.text = $"Opponent Wins: {opponentWins}";
    }


    private void UpdateDeckCount()
    {
        deckCountText.text = $"{shuffledDeck.Count}";
    }

    private void UpdateTurnCount()
    {
        turnCount++;
        turnCountText.text = $"Turn: {turnCount}";
    }

    private void UpdateScores()
    {
        //players Scores
    }

    private void SetupRevealButtons()
    {
        if (confirmRevealButton != null)
        {
            confirmRevealButton.onClick.AddListener(ConfirmReveal);
            Debug.Log("Confirm Reveal button listener added");
        }
        else
        {
            Debug.LogError("Confirm Reveal Button is not assigned!");
        }

        if (cancelRevealButton != null)
        {
            cancelRevealButton.onClick.AddListener(CancelReveal);
            Debug.Log("Cancel Reveal button listener added");
        }
        else
        {
            Debug.LogError("Cancel Reveal Button is not assigned!");
        }

        if (revealPopup != null)
        {
            revealPopup.SetActive(false); // וודא שהפאנל לא פעיל כברירת מחדל
            Debug.Log("Reveal popup initialized and set to inactive");
        }
        else
        {
            Debug.LogError("Reveal Popup is not assigned!");
        }
    }

    public void ActivateDoubleSwap(GameObject doubleSwapCard)
    {
        Debug.Log("ActivateDoubleSwap called");
        doubleSwapCardUsed = doubleSwapCard;

        if (swapPanel == null)
        {
            Debug.LogError("Swap panel is not assigned in the CardManager.");
            return;
        }

        swapPanel.SetActive(true);
        Debug.Log("Swap panel activated");

        PopulateSwapPanel(); // Populate the panel with available cards to swap
        UpdateConfirmButton();
    }


    private void PopulateSwapPanel()
    {
        // Clear existing content
        foreach (Transform child in swapPanelContent)
        {
            Destroy(child.gameObject);
        }

        selectedCardsForSwap.Clear();

        // Populate with player's cards
        foreach (GameObject card in playerCards)
        {
            GameObject cardCopy = Instantiate(card, swapPanelContent);

            // Set the size of the card to match the panelCardSize
            RectTransform rectTransform = cardCopy.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.sizeDelta = panelCardSize;  // התאמת גודל הקלף לפאנל
            }

            Button cardButton = cardCopy.GetComponent<Button>();
            if (cardButton == null)
            {
                cardButton = cardCopy.AddComponent<Button>();
            }
            cardButton.onClick.AddListener(() => SelectCardForSwap(cardCopy));
        }

        UpdateConfirmButton();
    }


    private void UpdateConfirmButton()
    {
        if (confirmSwapButton != null)
        {
            confirmSwapButton.interactable = selectedCardsForSwap.Count == 2;
            Debug.Log($"Confirm button interactable: {confirmSwapButton.interactable}");
        }
        else
        {
            Debug.LogError("Confirm Swap Button is not assigned!");
        }
    }

    public void CloseSwapPanel()
    {
        Debug.Log("CloseSwapPanel called");
        if (swapPanel != null)
        {
            swapPanel.SetActive(false);
            selectedCardsForSwap.Clear(); // ניקוי הבחירה
        }
        else
        {
            Debug.LogError("Swap panel is not assigned!");
        }
    }


    private void ResetCardSelection()
    {
        selectedCardsForSwap.Clear();
        foreach (var card in playerCards)
        {
            card.GetComponent<Image>().color = Color.white;
        }
    }

    private void SelectCardForSwap(GameObject card)
    {
        Debug.Log($"Card selected: {card.name}");

        if (selectedCardsForSwap.Contains(card))
        {
            selectedCardsForSwap.Remove(card);
            card.GetComponent<Image>().color = Color.white;
        }
        else if (selectedCardsForSwap.Count < 2)
        {
            selectedCardsForSwap.Add(card);
            card.GetComponent<Image>().color = Color.yellow;
        }
        else
        {
            // אם כבר נבחרו 2 קלפים, החלף את הקלף הראשון שנבחר
            GameObject firstSelected = selectedCardsForSwap[0];
            firstSelected.GetComponent<Image>().color = Color.white;
            selectedCardsForSwap.RemoveAt(0);
            selectedCardsForSwap.Add(card);
            card.GetComponent<Image>().color = Color.yellow;
        }

        UpdateConfirmButton();
    }


    public void ConfirmDoubleSwap()
    {
        if (selectedCardsForSwap.Count == 2)
        {
            // ביצוע ההחלפה בין הקלפים
            List<GameObject> randomOpponentCards = GetRandomOpponentCards(2);
            for (int i = 0; i < 2; i++)
            {
                Card tempCard = selectedCardsForSwap[i].GetComponent<CardData>().card;
                selectedCardsForSwap[i].GetComponent<CardData>().card = randomOpponentCards[i].GetComponent<CardData>().card;
                randomOpponentCards[i].GetComponent<CardData>().card = tempCard;
            }

            // הסרת הקלף המיוחד והחלפתו
            ReplaceSpecialCard(doubleSwapCardUsed);

            // סגירת פאנל ההחלפה
            selectedCardsForSwap.Clear();
            swapPanel.SetActive(false);

            // עדכון הידיים
            UpdatePlayerHand();
            UpdateOpponentHand();
        }
    }




    public void ConfirmSwap()
    {
        Debug.Log("ConfirmSwap called");

        if (selectedCardsForSwap.Count != 2)
        {
            Debug.LogError("Invalid number of cards selected for swap");
            return;
        }

        List<GameObject> opponentCardsToSwap = GetRandomOpponentCards(2);

        for (int i = 0; i < 2; i++)
        {
            GameObject playerCardObj = playerCards.Find(card => card.GetComponent<Image>().sprite == selectedCardsForSwap[i].GetComponent<Image>().sprite);
            CardData playerCardData = playerCardObj.GetComponent<CardData>();
            CardData opponentCardData = opponentCardsToSwap[i].GetComponent<CardData>();

            // החלפת הקלפים
            Card tempCard = playerCardData.card;
            playerCardData.card = opponentCardData.card;
            opponentCardData.card = tempCard;

            // עדכון הספרייט של קלף השחקן
            playerCardObj.GetComponent<Image>().sprite = GetCardSprite(playerCardData.card);

            // שמירת הקלף של היריב כמוסתר
            opponentCardsToSwap[i].GetComponent<Image>().sprite = cardBackSprite;
        }

        // הסרת קלף ה-DoubleSwap ומשיכת קלף חדש
        RemoveDoubleSwapAndDrawNewCard();

        // סגירת הפאנל
        CloseSwapPanel();

        // עדכון הידיים
        UpdatePlayerHand();
        UpdateOpponentHand();
    }


    private void RemoveDoubleSwapAndDrawNewCard()
    {
        if (doubleSwapCardUsed != null)
        {
            playerCards.Remove(doubleSwapCardUsed);
            Destroy(doubleSwapCardUsed);

            // משיכת קלף חדש מהחפיסה
            if (shuffledDeck.Count > 0)
            {
                Sprite newCardSprite = shuffledDeck[0];
                shuffledDeck.RemoveAt(0);

                GameObject newCard = Instantiate(cardPrefab, playerHandParent);
                newCard.GetComponent<Image>().sprite = newCardSprite;
                newCard.tag = "PlayerCard";

                CardData newCardData = newCard.AddComponent<CardData>();
                newCardData.card = Card.CreateFromSpriteName(newCardSprite.name);

                playerCards.Add(newCard);
                AddDragHandlers(newCard);

                Debug.Log($"New card drawn: {newCardSprite.name}");
            }
            else
            {
                Debug.Log("Deck is empty. No new card drawn.");
            }

            doubleSwapCardUsed = null;
        }
    }



    private List<GameObject> GetRandomOpponentCards(int count)
    {
        List<GameObject> randomCards = new List<GameObject>();
        List<GameObject> availableCards = new List<GameObject>(opponentCards);

        for (int i = 0; i < count && availableCards.Count > 0; i++)
        {
            int randomIndex = Random.Range(0, availableCards.Count);
            randomCards.Add(availableCards[randomIndex]);
            availableCards.RemoveAt(randomIndex);
        }

        return randomCards;
    }


    private Sprite GetCardSprite(Card card)
    {
        // אם הקלף הוא Green_Ace, נבחר קלף אחר מהחפיסה
        if (card.Color == CardColor.Green && card.Value == CardValue.Ace)
        {
            Debug.LogWarning("Attempted to display Green_Ace card. Selecting a random card instead.");
            if (shuffledDeck.Count > 0)
            {
                // בוחרים קלף רנדומלי מהחפיסה, ואז מוצאים את הספרייט התואם לו
                Card randomCard = Card.CreateFromSpriteName(shuffledDeck[Random.Range(0, shuffledDeck.Count)].name);
                return cardSprites.Find(sprite => sprite.name == $"{randomCard.Color}_{randomCard.Value}");
            }
            else
            {
                Debug.LogError("Deck is empty. Returning default sprite.");
                return null; // אפשר להחזיר תמונה ברירת מחדל אם צריך
            }
        }

        string spriteName = $"{card.Color}_{card.Value}";
        return cardSprites.Find(sprite => sprite.name == spriteName);
    }







    private bool IsPointerOverUIObject(Vector2 position)
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = position;
        List<RaycastResult> results = new List<RaycastResult>();
        mainCanvas.GetComponent<GraphicRaycaster>().Raycast(eventDataCurrentPosition, results);
        return results.Count > 0;
    }

    void CreateUniqueDeck()
    {
        shuffledDeck = new List<Sprite>();

        foreach (CardColor color in System.Enum.GetValues(typeof(CardColor)))
        {
            foreach (CardValue value in System.Enum.GetValues(typeof(CardValue)))
            {
                string spriteName = $"{color}_{value}";
                Sprite cardSprite = cardSprites.Find(s => s.name == spriteName);

                if (cardSprite != null)
                {
                    shuffledDeck.Add(cardSprite); // הוספת קלף ייחודי לחפיסה
                }
            }
        }

        ShuffleDeck(); // ערבוב החפיסה
    }



    void ShuffleDeck()
    {
        if (cardSprites == null || cardSprites.Count == 0)
        {
            Debug.LogError("cardSprites is not assigned or is empty in the Inspector.");
            return;
        }

        shuffledDeck = new List<Sprite>(cardSprites);
        shuffledDeck.Remove(cardBackSprite); // מסירים את גב הקלף (אם יש כזה)

        // הסרת Green_Ace מהחפיסה
        shuffledDeck.RemoveAll(s => s.name == "Green_Ace");

        // ערבוב החפיסה
        for (int i = 0; i < shuffledDeck.Count; i++)
        {
            Sprite temp = shuffledDeck[i];
            int randomIndex = Random.Range(0, shuffledDeck.Count);
            shuffledDeck[i] = shuffledDeck[randomIndex];
            shuffledDeck[randomIndex] = temp;
        }

        Debug.Log($"Deck shuffled with {shuffledDeck.Count} cards. Green_Ace removed.");
    }



    // בפונקציה שמטפלת בקלפים עצמם, לוודא שהקלף שנבחר תואם לתמונה:
    void DealInitialCards()
    {
        ClearCards(playerHandParent, playerCards);
        ClearCards(opponentHandParent, opponentCards);

        for (int i = 0; i < 7; i++)
        {
            // Deal to player
            GameObject playerCard = Instantiate(cardPrefab, playerHandParent);
            Image cardImage = playerCard.GetComponent<Image>();
            cardImage.sprite = DrawUniqueCard(false); // קבלת קלף ייחודי רנדומלי

            CardData cardData = playerCard.GetComponent<CardData>() ?? playerCard.AddComponent<CardData>();
            cardData.card = Card.CreateFromSpriteName(cardImage.sprite.name); // וודא שהנתונים תואמים לספרייט
            playerCards.Add(playerCard);

            // Deal to opponent
            GameObject opponentCard = Instantiate(opponentCardPrefab, opponentHandParent);
            Image opponentCardImage = opponentCard.GetComponent<Image>();
            opponentCardImage.sprite = DrawUniqueCard(true); // קבלת קלף ייחודי ליריב

            CardData opponentCardData = opponentCard.GetComponent<CardData>() ?? opponentCard.AddComponent<CardData>();
            opponentCardData.card = Card.CreateFromSpriteName(opponentCardImage.sprite.name); // וודא שהנתונים תואמים לספרייט
            opponentCards.Add(opponentCard);
        }

        UpdatePlayerHand();
        UpdateOpponentHand();
    }



    private Sprite DrawUniqueCard(bool isForOpponent)
    {
        Sprite drawnCardSprite = null;
        while (shuffledDeck.Count > 0)
        {
            drawnCardSprite = shuffledDeck[0];
            shuffledDeck.RemoveAt(0);

            // אם נבחר Green_Ace, מחליפים אותו בקלף רנדומלי אחר
            if (drawnCardSprite.name == "Green_Ace")
            {
                Debug.LogWarning("Green_Ace was drawn. Selecting a different random card.");
                if (shuffledDeck.Count > 0)
                {
                    // בוחרים קלף רנדומלי מהחפיסה
                    drawnCardSprite = shuffledDeck[Random.Range(0, shuffledDeck.Count)];
                    shuffledDeck.Remove(drawnCardSprite); // מסירים את הקלף הנבחר
                }
            }

            // יצירת אובייקט הקלף כדי לוודא שהנתונים תואמים לתמונה
            Card cardData = Card.CreateFromSpriteName(drawnCardSprite.name);
            if (!cardsInPlay.Any(c => c.Color == cardData.Color && c.Value == cardData.Value))
            {
                cardsInPlay.Add(cardData);
                return drawnCardSprite; // מחזירים את התמונה התואמת לקלף
            }
            else
            {
                shuffledDeck.Add(drawnCardSprite); // מחזירים לחפיסה אם הקלף כבר במשחק
            }
        }

        Debug.LogError("No unique cards available in deck!");
        return null;
    }



    public void ShowRestartConfirmation()
    {
        if (restartConfirmPanel != null)
        {
            restartConfirmPanel.SetActive(true);
        }
        else
        {
            Debug.LogError("Restart Confirm Panel is not assigned!");
        }
    }

    public void ConfirmRestart()
    {
        // איפוס המשחק
        ResetGame();

        // סגירת פאנל האישור
        if (restartConfirmPanel != null)
        {
            restartConfirmPanel.SetActive(false);
        }
    }

    public void CancelRestart()
    {
        if (restartConfirmPanel != null)
        {
            restartConfirmPanel.SetActive(false);
        }
    }


    private void ResetGame()
    {
        // ניקוי הידיים של השחקנים
        ClearCards(playerHandParent, playerCards);
        ClearCards(opponentHandParent, opponentCards);

        // איפוס משתנים
        isPlayerTurn = true;
        turnCount = 0;
        playerTurns = 0;
        opponentTurns = 0;
        mustDiscardCard = false;

        // ערבוב מחדש של החפיסה
        ShuffleDeck();

        // חלוקת קלפים מחדש
        DealInitialCards();

        // עדכון הממשק
        UpdateUI();
        UpdateTurnDisplay();
        UpdateButtonStates();

        // איפוס ה-AI
        aiPlayer = new AIPlayer(this);

        Debug.Log("Game has been reset.");
    }


    public void UseDoubleSwapCard()
    {
        GameObject doubleSwapCard = playerCards.Find(card =>
            card.GetComponent<CardData>()?.card.SpecialType == SpecialCardType.DoubleSwap);

        if (doubleSwapCard != null)
        {
            Debug.Log("DoubleSwap card found. Activating swap panel.");
            ActivateDoubleSwap(doubleSwapCard);
        }
        else
        {
            Debug.Log("No DoubleSwap card in player's hand.");
            statusText.text = "You don't have a DoubleSwap card.";
        }
    }

    public void UseDoubleSwapCard(List<Card> cardsToSwap = null)
    {
        if (cardsToSwap == null)
        {
            // לוגיקה עבור השחקן
            ActivateDoubleSwap(null);
        }
        else
        {
            // לוגיקה עבור ה-AI
            List<GameObject> aiCardsToSwap = opponentCards.Where(c => cardsToSwap.Contains(c.GetComponent<CardData>().card)).ToList();
            List<GameObject> playerCardsToSwap = GetRandomPlayerCards(2);
            for (int i = 0; i < aiCardsToSwap.Count && i < playerCardsToSwap.Count; i++)
            {
                SwapCards(aiCardsToSwap[i], playerCardsToSwap[i]);
            }
        }
    }
    public void DiscardAICard(int cardIndex)
    {
        if (cardIndex >= 0 && cardIndex < opponentCards.Count)
        {
            GameObject cardToDiscard = opponentCards[cardIndex];
            opponentCards.RemoveAt(cardIndex);
            Destroy(cardToDiscard);
            UpdateOpponentHand();
        }
    }

    void AddDragHandlers(GameObject card)
    {
        CardDragHandler dragHandler = card.GetComponent<CardDragHandler>();
        if (dragHandler == null)
        {
            card.AddComponent<CardDragHandler>();
        }
    }

    void ClearCards(Transform parent, List<GameObject> cardList)
    {
        foreach (var card in cardList)
        {
            Destroy(card);
        }
        cardList.Clear();
    }

    public void ProposeTrade()
    {
        if (mustDiscardCard)
        {
            statusText.text = "You must discard a card before proposing a trade.";
            return;
        }
        tradeAccepted = true;
        if (tradeAccepted)
        {
            ShowTradePopup();
        }
    }
    public void UpdateCardOrder(GameObject movedCard, int newIndex)
    {
        if (playerCards == null)
        {
            Debug.LogError("playerCards list is null!");
            return;
        }

        int oldIndex = playerCards.IndexOf(movedCard);
        if (oldIndex != -1 && oldIndex != newIndex)
        {
            playerCards.RemoveAt(oldIndex);
            playerCards.Insert(newIndex, movedCard);
            Debug.Log($"Card {movedCard.name} moved from index {oldIndex} to {newIndex}");
        }
        else
        {
            Debug.LogWarning($"Failed to move card {movedCard.name}. Old index: {oldIndex}, New index: {newIndex}");
        }
    }

    public void ReorderPlayerCards(GameObject draggedCard, int newIndex)
    {
        int oldIndex = playerCards.IndexOf(draggedCard);
        if (oldIndex != -1 && oldIndex != newIndex)
        {
            playerCards.RemoveAt(oldIndex);
            playerCards.Insert(newIndex, draggedCard);
        }
    }

    public void UpdatePlayerHandVisually()
    {
        float totalWidth = (panelCardSize.x * playerCards.Count) + (cardSpacing * (playerCards.Count - 1));
        float startX = -totalWidth / 2 + panelCardSize.x / 2;

        for (int i = 0; i < playerCards.Count; i++)
        {
            GameObject card = playerCards[i];
            RectTransform cardRect = card.GetComponent<RectTransform>();
            float xPosition = GetCardPosition(i);
            cardRect.anchoredPosition = new Vector2(xPosition, 0);
            cardRect.sizeDelta = panelCardSize;

            // Ensure the card displays the correct sprite
            Image cardImage = card.GetComponent<Image>();
            CardData cardData = card.GetComponent<CardData>();
            if (cardData != null && cardImage != null)
            {
                cardImage.sprite = GetCardSprite(cardData.card);
            }
        }
    }

    public float GetCardPosition(int index)
    {
        float totalWidth = (panelCardSize.x * playerCards.Count) + (cardSpacing * (playerCards.Count - 1));
        float startX = -totalWidth / 2 + panelCardSize.x / 2;
        return startX + index * (panelCardSize.x + cardSpacing);
    }


    public void ShowTradePopup()
    {
        UpdateTradePopupCards();
        tradePopupPanel.SetActive(true);
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
        }
        timerCoroutine = StartCoroutine(TradeTimer());
    }

    public void HideTradePopup()
    {
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
        }

        tradePopupPanel.SetActive(false);
        timerText.text = "";
        DeselectCard();
    }

    public void UpdateTradePopupCards()
    {
        foreach (Transform child in tradePopupParent)
        {
            Destroy(child.gameObject);
        }

        originalCardParents.Clear();
        originalCardSizes.Clear();

        float totalWidth = (panelCardSize.x * playerCards.Count) + (cardSpacing * (playerCards.Count - 1));
        float startX = -totalWidth / 2 + panelCardSize.x / 2;

        for (int i = 0; i < playerCards.Count; i++)
        {
            GameObject card = playerCards[i];
            originalCardParents.Add(card.transform.parent);
            originalCardSizes.Add(card.GetComponent<RectTransform>().sizeDelta);

            GameObject panelCard = Instantiate(card, tradePopupParent);
            panelCard.transform.localScale = Vector3.one;
            RectTransform rectTransform = panelCard.GetComponent<RectTransform>();
            rectTransform.sizeDelta = panelCardSize;

            float xPosition = startX + i * (panelCardSize.x + cardSpacing);
            rectTransform.anchoredPosition = new Vector2(xPosition, 0);

            Button cardButton = panelCard.GetComponent<Button>();
            if (cardButton == null)
            {
                cardButton = panelCard.AddComponent<Button>();
            }
            int index = i;
            cardButton.onClick.AddListener(() => SelectPlayerCard(playerCards[index]));
        }

        selectedPlayerCard = null;
    }

    public void SelectPlayerCard(GameObject card)
    {
        // שימוש ישיר ב-Instance של CardManager
        CardManager cardManager = CardManager.Instance;

        if (cardManager == null)
        {
            Debug.LogError("CardManager instance not found!");
            return;
        }

        // אם יש צורך להשליך קלף, נבצע זאת קודם
        if (mustDiscardCard)
        {
            cardManager.DiscardCard(card);
            return;
        }

        // בדיקה אם יש קלף נבחר אחר, ונחזיר אותו למצב רגיל (לא צבוע)
        if (selectedPlayerCard != null)
        {
            selectedPlayerCard.GetComponent<Image>().color = Color.white;
        }

        // אם הקלף הנבחר הוא אותו קלף, נבטל את הבחירה
        if (selectedPlayerCard == card)
        {
            selectedPlayerCard = null;
        }
        else
        {
            // אם זה קלף חדש, נבחר אותו ונשנה את צבעו לצהוב כדי להדגיש אותו
            selectedPlayerCard = card;
            selectedPlayerCard.GetComponent<Image>().color = Color.yellow;
            Debug.Log("Player selected card for action: " + selectedPlayerCard.GetComponent<Image>().sprite.name);

            // נוודא שהקלף הוא קלף מיוחד ואז נפעיל את הפאנל המתאים
            CardData cardData = card.GetComponent<CardData>();
            if (cardData != null)
            {
                switch (cardData.card.SpecialType)
                {
                    case SpecialCardType.DoubleSwap:
                        Debug.Log("DoubleSwap card selected, activating swap panel.");
                        cardManager.ActivateDoubleSwap(card);
                        break;

                    case SpecialCardType.Reveal:
                        Debug.Log("Reveal card selected, activating reveal panel.");
                        cardManager.ActivateRevealPopup(card);
                        break;

                    case SpecialCardType.Skip:
                        Debug.Log("Skip card selected, skipping opponent's turn.");
                        cardManager.UseSkipCard();
                        cardManager.RemoveSpecialCard(card); // מסירים את הקלף מהיד אחרי השימוש
                        DrawCardFromDeck();
                        break;

                    case SpecialCardType.Joker:
                        Debug.Log("Joker card selected, prompting for card transformation.");
                        cardManager.UseJokerCard(card);  // מבקשים מהמשתמש לבחור ערך ל-Joker
                        break;

                    default:
                        Debug.LogWarning("No special action for this card.");
                        break;
                }
            }
        }
    }



    public void ActivateRevealPopup(GameObject revealCard)
    {
        if (revealPopup != null)
        {
            revealPopup.SetActive(true); // הפעלת הפאנל
            currentRevealCard = revealCard; // שמירת קלף ה-Reveal הנוכחי
            Debug.Log("Reveal popup activated");
        }
        else
        {
            Debug.LogError("Reveal popup is not assigned!");
        }
    }



    private void DeselectCard()
    {
        if (selectedPlayerCard != null)
        {
            selectedPlayerCard.GetComponent<Image>().color = Color.white;
            selectedPlayerCard = null;
        }
    }

    private IEnumerator TradeTimer()
    {
        float timeLeft = tradeTime;
        while (timeLeft > 0)
        {
            timerText.text = $"{timeLeft:F1}S";
            yield return new WaitForSeconds(0.1f);
            timeLeft -= 0.1f;
        }

        SelectRandomPlayerCard();
        ConfirmTrade();
        HideTradePopup();
    }

    private void SelectRandomPlayerCard()
    {
        if (playerCards.Count > 0)
        {
            int randomIndex = Random.Range(0, playerCards.Count);
            SelectPlayerCard(playerCards[randomIndex]);
        }
        else
        {
            Debug.LogWarning("No cards available to select.");
        }
    }

    public void ConfirmTrade()
    {
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
        }

        if (selectedPlayerCard != null)
        {
            if (opponentCards.Count > 0)
            {
                int randomIndex = Random.Range(0, opponentCards.Count);
                selectedOpponentCard = opponentCards[randomIndex];

                Debug.Log("Confirming trade between player and opponent.");

                Sprite tempSprite = selectedPlayerCard.GetComponent<Image>().sprite;
                selectedPlayerCard.GetComponent<Image>().sprite = selectedOpponentCard.GetComponent<Image>().sprite;
                selectedOpponentCard.GetComponent<Image>().sprite = tempSprite;

                UpdateCardDisplay(selectedPlayerCard, false);
                UpdateCardDisplay(selectedOpponentCard, true);

                Debug.Log("Trade completed!");
            }
            else
            {
                Debug.LogWarning("Unable to complete trade. No opponent cards available.");
            }
        }
        else
        {
            Debug.LogWarning("No card selected for trade.");
        }
        HideTradePopup();
    }

    void UpdateCardDisplay(GameObject card, bool isOpponentCard)
    {
        Image cardImage = card.GetComponent<Image>();
        if (isOpponentCard)
        {
            cardImage.sprite = cardBackSprite;
        }
        else
        {
            if (cardImage.sprite == cardBackSprite)
            {
                cardImage.sprite = shuffledDeck[Random.Range(0, shuffledDeck.Count)];
            }
        }
    }

    public void CancelTrade()
    {
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
        }

        DeselectCard();
        selectedOpponentCard = null;
        HideTradePopup();
        Debug.Log("Trade canceled");
    }


    public void DrawCardFromDeck()
    {
        if (shuffledDeck.Count > 0)
        {
            // שליפת קלף מהחפיסה
            Sprite drawnCardSprite = shuffledDeck[0];
            shuffledDeck.RemoveAt(0);

            // אם נבחר הקלף Green_Ace, נבחר קלף אחר מהחפיסה
            if (drawnCardSprite.name == "Green_Ace")
            {
                Debug.LogWarning("Green_Ace drawn. Selecting a random card instead.");
                if (shuffledDeck.Count > 0)
                {
                    drawnCardSprite = shuffledDeck[Random.Range(0, shuffledDeck.Count)];
                    shuffledDeck.Remove(drawnCardSprite);
                }
            }

            // יצירת אובייקט קלף חדש והוספתו ליד השחקן
            GameObject newCard = Instantiate(cardPrefab, playerHandParent);
            Image cardImage = newCard.GetComponent<Image>();
            cardImage.sprite = drawnCardSprite;

            newCard.tag = "PlayerCard";

            // הוספת הקלף החדש ליד של השחקן
            playerCards.Add(newCard);

            // עדכון התצוגה של יד השחקן
            UpdatePlayerHand();

            // פעולה להחזרת קלף לחפיסה באופן אוטומטי לאחר שליפת קלף חדש
            ReturnCardToDeck();
        }
        else
        {
            Debug.Log("Deck is empty.");
        }
    }


    private void ReturnCardToDeck()
    {
        if (playerCards.Count > 0)
        {
            Debug.Log("Please select a card from your hand to return to the deck.");

            // הוספת listener לכל קלף כדי לאפשר בחירת קלף להחזרה
            foreach (GameObject card in playerCards)
            {
                Button cardButton = card.GetComponent<Button>();
                if (cardButton == null)
                {
                    cardButton = card.AddComponent<Button>();
                }

                // הסרת מאזינים קודמים כדי למנוע לחיצות כפולות
                cardButton.onClick.RemoveAllListeners();

                // הוספת מאזין להחזרת הקלף הנבחר
                cardButton.onClick.AddListener(() =>
                {
                    // הסרת הקלף מהיד של השחקן
                    playerCards.Remove(card);
                    Destroy(card);

                    Debug.Log("Card returned to deck: " + card.name);

                    // עדכון התצוגה של היד
                    UpdatePlayerHand();

                    // בדיקה אם לשחקן יש בדיוק 7 קלפים אחרי החזרת הקלף
                    EnsurePlayerHasSevenCards();
                });
            }
        }
        else
        {
            Debug.LogWarning("No cards in hand to return.");
        }
    }




    public void UpdatePlayerHand(bool maintainPositions = true)
    {
        if (!maintainPositions)
        {
            float totalWidth = (panelCardSize.x * playerCards.Count) + (cardSpacing * (playerCards.Count - 1));
            float startX = -totalWidth / 2 + panelCardSize.x / 2;

            for (int i = 0; i < playerCards.Count; i++)
            {
                GameObject card = playerCards[i];
                RectTransform cardRect = card.GetComponent<RectTransform>();
                float xPosition = startX + i * (panelCardSize.x + cardSpacing);
                cardRect.anchoredPosition = new Vector2(xPosition, 0);
                cardRect.sizeDelta = panelCardSize;
            }
        }

        UpdateCardButtons();

        foreach (var card in playerCards)
        {
            card.GetComponent<Image>().color = Color.white;
        }

        if (selectedPlayerCard != null && playerCards.Contains(selectedPlayerCard))
        {
            selectedPlayerCard.GetComponent<Image>().color = Color.yellow;
        }
    }

    public void DiscardCard(GameObject card)
    {
        CardData cardData = card.GetComponent<CardData>();
        if (cardData != null)
        {
            cardsInPlay.Remove(cardData.card); // הסרת הקלף מרשימת הקלפים במשחק
        }

        playerCards.Remove(card);
        Destroy(card);
        mustDiscardCard = false;

        UpdatePlayerHand();

        // בדיקה אם יש בדיוק 7 קלפים ביד
        EnsurePlayerHasSevenCards();

        statusText.text = "Card discarded. You can continue your turn.";
        UpdateButtonStates();
    }


    public void StartNewTurn()
    {
        isPlayerTurn = true;
        UpdateTurnDisplay();
        // כאן תוכל להוסיף לוגיקה נוספת להתחלת תור חדש
    }

    public void EndTurn()
    {
        if (mustDiscardCard)
        {
            statusText.text = "You must discard a card before ending your turn.";
            return;
        }

        // בדיקה שהשחקן מחזיק בדיוק 7 קלפים
        EnsurePlayerHasSevenCards();

        // עדכון התורות לפי השחקן הנוכחי
        if (isPlayerTurn)
        {
            playerTurns++;
        }
        else
        {
            opponentTurns++;
        }

        // עדכון התצוגה והמשתנים הקשורים בתורות
        UpdateUI();

        // בדיקה האם המשחק הגיע לסיום (במקום להפסיק אחרי 2 סיבובים בלבד)
        if (playerTurns >= maxTurns && opponentTurns >= maxTurns)
        {
            EndGame();
            return;
        }

        // החלפת תור לשחקן הבא
        isPlayerTurn = !isPlayerTurn;
        UpdateTurnDisplay();

        // אם זה התור של ה-AI, נתחיל את הסיבוב של היריב
        if (!isPlayerTurn)
        {
            StartCoroutine(OpponentTurn());
        }
    }





    private IEnumerator OpponentTurn()
    {
        yield return new WaitForSeconds(2f);
        aiPlayer.PlayTurn();
        yield return new WaitForSeconds(2f);
        EndTurn();
    }


    public void ReplaceSpecialCard(GameObject specialCard)
    {
        // הסרת הקלף המיוחד מהיד של השחקן
        playerCards.Remove(specialCard);
        Destroy(specialCard);

        // משיכת קלף חדש מהחפיסה
        if (shuffledDeck.Count > 0)
        {
            Sprite newCardSprite = shuffledDeck[0];
            shuffledDeck.RemoveAt(0);

            GameObject newCard = Instantiate(cardPrefab, playerHandParent);
            newCard.GetComponent<Image>().sprite = newCardSprite;
            newCard.tag = "PlayerCard";

            CardData newCardData = newCard.AddComponent<CardData>();
            newCardData.card = Card.CreateFromSpriteName(newCardSprite.name);

            playerCards.Add(newCard);
            AddDragHandlers(newCard);

            Debug.Log($"New card drawn to replace special card: {newCardSprite.name}");
        }
        else
        {
            Debug.Log("Deck is empty. No new card drawn.");
        }

        // עדכון יד השחקן
        UpdatePlayerHand();
    }



    public void DrawCardForAI()
    {
        if (shuffledDeck.Count > 0)
        {
            Sprite drawnCardSprite = shuffledDeck[0];
            shuffledDeck.RemoveAt(0);

            // בדיקה נוספת: אם נבחר Green_Ace, נבחר קלף רנדומלי אחר
            if (drawnCardSprite.name == "Green_Ace")
            {
                Debug.LogWarning("Green_Ace drawn for AI. Selecting a random card instead.");
                if (shuffledDeck.Count > 0)
                {
                    drawnCardSprite = shuffledDeck[Random.Range(0, shuffledDeck.Count)];
                    shuffledDeck.Remove(drawnCardSprite); // מסירים את הקלף החדש שנבחר
                }
            }

            GameObject newCard = Instantiate(opponentCardPrefab, opponentHandParent);
            newCard.GetComponent<Image>().sprite = drawnCardSprite;
            newCard.tag = "OpponentCard";

            Card cardComponent = newCard.GetComponent<Card>();
            if (cardComponent == null)
            {
                cardComponent = newCard.AddComponent<Card>();
            }

            Card createdCard = Card.CreateFromSpriteName(drawnCardSprite.name);
            cardComponent.Initialize(createdCard.Color, createdCard.Value, createdCard.SpecialType);

            opponentCards.Add(newCard);

            Debug.Log("AI drew a card: " + drawnCardSprite.name);

            ReplaceDuplicateCards(opponentCards, true); // להבטיח שאין כפילויות ביד של היריב

            if (opponentCards.Count > 7)
            {
                DiscardAICard(Random.Range(0, opponentCards.Count)); // השלכת קלף אקראי מהיריב
            }

            UpdateOpponentHand();
        }
        else
        {
            Debug.Log("Deck is empty. AI cannot draw a card.");
        }
    }





    private void ReplaceDuplicateCards(List<GameObject> cards, bool isAI = false)
    {
        var cardValues = new HashSet<int>();
        for (int i = 0; i < cards.Count; i++)
        {
            CardData cardData = cards[i].GetComponent<CardData>();
            if (cardData == null) continue;

            int cardValue = (int)cardData.card.Value;

            // If the card value already exists, replace the card
            if (cardValues.Contains(cardValue))
            {
                Debug.Log($"{(isAI ? "AI" : "Player")} has duplicate {cardData.card.Value}, replacing it.");

                // Remove duplicate card
                Destroy(cards[i]);

                // Replace with a new card from the deck
                if (shuffledDeck.Count > 0)
                {
                    Sprite newCardSprite = shuffledDeck[0];
                    shuffledDeck.RemoveAt(0);

                    GameObject newCard = Instantiate(isAI ? opponentCardPrefab : cardPrefab, isAI ? opponentHandParent : playerHandParent);
                    newCard.GetComponent<Image>().sprite = newCardSprite;
                    newCard.tag = isAI ? "OpponentCard" : "PlayerCard";

                    CardData newCardData = newCard.AddComponent<CardData>();
                    newCardData.card = Card.CreateFromSpriteName(newCardSprite.name);

                    cards[i] = newCard;
                }
                else
                {
                    Debug.Log("Deck is empty. Unable to replace the card.");
                }
            }
            else
            {
                cardValues.Add(cardValue);
            }
        }

        if (isAI)
        {
            UpdateOpponentHand();
        }
        else
        {
            UpdatePlayerHand();
        }
    }



    public void ProposeTradeForAI()
    {
        if (opponentCards.Count > 0 && playerCards.Count > 0)
        {
            int randomPlayerCardIndex = Random.Range(0, playerCards.Count);
            int randomOpponentCardIndex = Random.Range(0, opponentCards.Count);

            // Get cards for the trade
            GameObject playerCard = playerCards[randomPlayerCardIndex];
            GameObject opponentCard = opponentCards[randomOpponentCardIndex];

            // Swap the card data
            Card playerCardData = playerCard.GetComponent<CardData>().card;
            Card opponentCardData = opponentCard.GetComponent<CardData>().card;

            // Perform the trade
            playerCard.GetComponent<CardData>().card = opponentCardData;
            opponentCard.GetComponent<CardData>().card = playerCardData;

            // Update the sprites
            playerCard.GetComponent<Image>().sprite = GetCardSprite(opponentCardData); // Ensure the player sees the actual card
            opponentCard.GetComponent<Image>().sprite = cardBackSprite; // Ensure the opponent's card remains hidden as BackCard

            Debug.Log("AI traded a card with the player.");

            // Update the hands
            UpdatePlayerHand();
            UpdateOpponentHand();
        }
        else
        {
            Debug.Log("AI could not propose a trade. Not enough cards.");
        }
    }


    private void UpdateOpponentHand()
    {
        // עדכון הממשק הגרפי של יד היריב (במקרה זה, הקלפים של ה-AI)
        foreach (var card in opponentCards)
        {
            card.GetComponent<Image>().sprite = cardBackSprite; // להציג גב הקלף
        }
    }

    private IEnumerator HideOpponentCardsAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        foreach (var opponentCard in opponentCards)
        {
            opponentCard.GetComponent<Image>().sprite = cardBackSprite;
        }

        Debug.Log("Opponent's cards are hidden again.");
    }

    public void UseSkipCard()
    {
        Debug.Log("Skip card used! Skipping opponent's turn.");

        // דילוג על תור היריב, כלומר מחזירים את התור לשחקן
        isPlayerTurn = true;  // התור חוזר לשחקן
        UpdateTurnDisplay();

        // עדכון הממשק והמשך המשחק מהתור של השחקן
        statusText.text = "Opponent's turn was skipped!";
    }


    private IEnumerator SelectTwoCardsForSwap()
    {
        selectedCardsForSwap.Clear();
        statusText.text = "Select two cards to swap.";

        while (selectedCardsForSwap.Count < 2)
        {
            yield return new WaitForSeconds(0.1f);
        }

        // כאן תבצע את ההחלפה בפועל
        SwapSelectedCards();
    }

    private void SwapSelectedCards()
    {
        if (selectedCardsForSwap.Count == 2)
        {
            // החלף את הקלפים הנבחרים עם קלפים אקראיים של היריב
            List<GameObject> opponentCardsToSwap = GetRandomOpponentCards(2);

            for (int i = 0; i < 2; i++)
            {
                Sprite tempSprite = selectedCardsForSwap[i].GetComponent<Image>().sprite;
                selectedCardsForSwap[i].GetComponent<Image>().sprite = opponentCardsToSwap[i].GetComponent<Image>().sprite;
                opponentCardsToSwap[i].GetComponent<Image>().sprite = tempSprite;
            }

            statusText.text = "Cards swapped!";
            UpdatePlayerHand();
            UpdateOpponentHand();
        }

        swapPanel.SetActive(false);
        selectedCardsForSwap.Clear();
    }


    // החלפת קלפים אקראיים עבור ה-AI
    private void SwapRandomOpponentCards()
    {
        List<GameObject> selectedAIcards = GetRandomOpponentCards(2);
        List<GameObject> randomPlayerCards = GetRandomPlayerCards(2);

        for (int i = 0; i < 2; i++)
        {
            Sprite tempSprite = selectedAIcards[i].GetComponent<Image>().sprite;
            selectedAIcards[i].GetComponent<Image>().sprite = randomPlayerCards[i].GetComponent<Image>().sprite;
            randomPlayerCards[i].GetComponent<Image>().sprite = tempSprite;
        }

        Debug.Log("AI swapped cards with the player.");
        UpdatePlayerHand();
        UpdateOpponentHand();
    }

    // החזרת שני קלפים אקראיים מהיד של השחקן
    private List<GameObject> GetRandomPlayerCards(int count)
    {
        List<GameObject> selectedPlayerCards = new List<GameObject>();

        for (int i = 0; i < count; i++)
        {
            int randomIndex = Random.Range(0, playerCards.Count);
            selectedPlayerCards.Add(playerCards[randomIndex]);
        }

        return selectedPlayerCards;
    }



    public void UseJokerCard(GameObject jokerCard)
    {
        selectedJokerCard = jokerCard; // שמירת הקלף הנבחר
        OpenJokerValueSelection(); // פתיחת חלון לבחירת ערך ה-Joker
    }

    public void RemoveSpecialCard(GameObject card)
    {
        playerCards.Remove(card);
        Destroy(card);
        Debug.Log("Special card removed from hand.");
    }




    private void OpenJokerValueSelection()
    {
        // כאן תציג חלון לבחירת הערך החדש ל-Joker, לדוגמה פאנל בחירה ב-UI.
        // לאחר שהשחקן בחר את הערך, נעדכן את הקלף.

        CardValue selectedValue = CardValue.King;  // דוגמה לערך שנבחר
        selectedJokerCard.GetComponent<CardData>().card.Initialize(CardColor.Orange, selectedValue, SpecialCardType.Joker);

        Debug.Log("Joker value set to: " + selectedValue);
    }


    private List<Card> ConvertToCardList(List<GameObject> cardObjects)
    {
        List<Card> cards = new List<Card>();

        foreach (var go in cardObjects)
        {
            // נוודא שיש ל-GameObject רכיב CardData
            CardData cardData = go.GetComponent<CardData>();

            if (cardData == null)
            {
                Debug.LogError($"GameObject {go.name} does not have a CardData component.");
                continue;
            }

            // נוסיף את הקלף לרשימה
            cards.Add(cardData.card);
        }

        return cards;
    }

    public Card CreateCard(CardColor color, CardValue value)
    {
        // יצירת אובייקט משחק חדש
        GameObject newCardObject = Instantiate(cardPrefab);

        // הוספת רכיב הקלף ל-GameObject
        Card card = newCardObject.GetComponent<Card>();
        if (card == null)
        {
            Debug.LogError("Card prefab missing Card component!");
            return null;
        }

        // אתחול הקלף
        card.Initialize(color, value);

        return card;
    }



    private void SwapCards(GameObject aiCard, GameObject playerCard)
    {
        Card tempCard = aiCard.GetComponent<CardData>().card;
        aiCard.GetComponent<CardData>().card = playerCard.GetComponent<CardData>().card;
        playerCard.GetComponent<CardData>().card = tempCard;

        aiCard.GetComponent<Image>().sprite = cardBackSprite;
        playerCard.GetComponent<Image>().sprite = GetCardSprite(playerCard.GetComponent<CardData>().card);

        UpdatePlayerHand();
        UpdateAIHand();
    }

    private void UpdateAIHand()
    {
        List<Card> aiHand = opponentCards.Select(c => c.GetComponent<CardData>().card).ToList();
        aiPlayer.UpdateHand(aiHand);
    }

    private void UpdateCardButtons()
    {
        for (int i = 0; i < playerCards.Count; i++)
        {
            GameObject card = playerCards[i];
            Button cardButton = card.GetComponent<Button>();
            if (cardButton == null)
            {
                cardButton = card.AddComponent<Button>();
            }
            int index = i;
            cardButton.onClick.RemoveAllListeners();
            cardButton.onClick.AddListener(() => SelectPlayerCard(playerCards[index]));
        }
    }

    private void UpdateButtonStates()
    {
        drawCardButton.interactable = !mustDiscardCard;
        endTurnButton.interactable = !mustDiscardCard;
    }

    public void UseRevealCard(GameObject revealCard = null)
    {
        Debug.Log($"UseRevealCard called for: {revealCard?.name ?? "AI"}");

        if (revealCard == null)
        {
            // זה קלף Reveal של ה-AI
            StartCoroutine(RevealPlayerCards());
        }
        else
        {
            // זה קלף Reveal של השחקן
            StartCoroutine(RevealOpponentCards());
        }
    }


    public void ConfirmReveal()
    {
        if (revealPopup != null)
        {
            revealPopup.SetActive(false); // כיבוי הפאנל
        }

        // חשיפת קלפי היריב
        StartCoroutine(RevealOpponentCards());

        // הסרת קלף ה-Reveal והחלפתו
        ReplaceSpecialCard(currentRevealCard);

        currentRevealCard = null;
    }


    public void CancelReveal()
    {
        Debug.Log("CancelReveal called");
        if (revealPopup != null)
        {
            revealPopup.SetActive(false);
            Debug.Log("Reveal popup deactivated");
        }
        currentRevealCard = null;
    }



    private IEnumerator RevealPlayerCards()
    {
        Debug.Log("Revealing player cards to AI");
        tempRevealedCards.Clear();

        foreach (var cardObject in playerCards)
        {
            CardData cardData = cardObject.GetComponent<CardData>();
            if (cardData != null && cardData.card != null)
            {
                tempRevealedCards.Add(cardData.card);
            }
        }

        aiPlayer.SetRevealedPlayerCards(tempRevealedCards);

        yield return new WaitForSeconds(2);

        Debug.Log("Hiding player cards from AI");
        aiPlayer.SetRevealedPlayerCards(new List<Card>());
    }

    // חשיפת קלפי היריב למשך 2 שניות
    private IEnumerator RevealOpponentCards()
    {
        Debug.Log("Revealing opponent (AI) cards to player");

        foreach (var card in opponentCards)
        {
            // וודא שהתמונה שנחשפת אינה Green_Ace
            CardData cardData = card.GetComponent<CardData>();
            if (cardData.card.Color == CardColor.Green && cardData.card.Value == CardValue.Ace)
            {
                Debug.LogWarning("Green_Ace found in opponent's hand during reveal. Replacing with a random card.");

                // החלפה בקלף רנדומלי מהחפיסה
                if (shuffledDeck.Count > 0)
                {
                    Sprite randomCardSprite = shuffledDeck[Random.Range(0, shuffledDeck.Count)];
                    card.GetComponent<Image>().sprite = randomCardSprite;

                    // עדכון המידע של הקלף החדש
                    cardData.card = Card.CreateFromSpriteName(randomCardSprite.name);
                }
            }
            else
            {
                // הצגת הקלף הנוכחי אם הוא לא Green_Ace
                card.GetComponent<Image>().sprite = GetCardSprite(cardData.card);
            }
        }

        // מחכים 2 שניות לפני החזרת הקלפים
        yield return new WaitForSeconds(2);

        Debug.Log("Hiding opponent (AI) cards from player");

        // מחזירים את כל הקלפים של היריב לתצוגת הגב שלהם
        foreach (var card in opponentCards)
        {
            card.GetComponent<Image>().sprite = cardBackSprite;
        }
    }


    private void RemoveRevealAndDrawNewCard()
    {
        if (currentRevealCard != null)
        {
            playerCards.Remove(currentRevealCard);
            Destroy(currentRevealCard);

            // משיכת קלף חדש מהחפיסה
            if (shuffledDeck.Count > 0)
            {
                Sprite newCardSprite = shuffledDeck[0];
                shuffledDeck.RemoveAt(0);

                GameObject newCard = Instantiate(cardPrefab, playerHandParent);
                newCard.GetComponent<Image>().sprite = newCardSprite;
                newCard.tag = "PlayerCard";

                CardData newCardData = newCard.AddComponent<CardData>();
                newCardData.card = Card.CreateFromSpriteName(newCardSprite.name);

                playerCards.Add(newCard);
                AddDragHandlers(newCard);

                Debug.Log($"New card drawn to replace Reveal: {newCardSprite.name}");
            }
            else
            {
                Debug.Log("Deck is empty. No new card drawn.");
            }

            currentRevealCard = null;
        }

        UpdatePlayerHand();
    }


    public void UseSpecialCard(GameObject specialCard)
    {
        // נשתמש בקלף המיוחד בהתאם לסוג שלו
        CardData cardData = specialCard.GetComponent<CardData>();
        if (cardData != null)
        {
            // בדיקת סוג הקלף המיוחד
            switch (cardData.card.SpecialType)
            {
                case SpecialCardType.DoubleSwap:
                    ActivateDoubleSwap(specialCard);
                    break;

                case SpecialCardType.Reveal:
                    ActivateRevealPopup(specialCard);
                    break;

                case SpecialCardType.Skip:
                    UseSkipCard();
                    break;

                case SpecialCardType.Joker:
                    UseJokerCard(specialCard);
                    break;

                default:
                    Debug.LogWarning("Unknown special card type.");
                    break;
            }

            // הסרת הקלף המיוחד מהיד של השחקן
            RemoveSpecialCard(specialCard);

            // בדיקה אם יש לשחקן פחות מ-7 קלפים
            EnsurePlayerHasSevenCards();
        }
    }
    public void EnsurePlayerHasSevenCards()
    {
        // אם יש יותר מ-7 קלפים ביד של השחקן, נסיר קלפים
        while (playerCards.Count > 7)
        {
            // כאן תוכל לבחור איזה קלף להסיר - לדוגמה, קלף רנדומלי
            GameObject cardToRemove = playerCards[0];  // במקרה הזה, מסירים את הקלף הראשון
            playerCards.RemoveAt(0);
            Destroy(cardToRemove);  // הסרת הקלף מהמשחק
        }

        // אם יש פחות מ-7 קלפים ביד של השחקן, נוסיף קלפים
        while (playerCards.Count < 7)
        {
            if (shuffledDeck.Count > 0)
            {
                // משיכת קלף מהחפיסה
                DrawCardFromDeck();
            }
            else
            {
                Debug.LogWarning("The deck is empty. No new cards to draw.");
                break;
            }
        }

        // עדכון היד של השחקן לאחר השינויים
        UpdatePlayerHand();
    }


    private void ShowRoundEndPanel(string winner)
    {
        roundEndText.text = $"{winner} wins this round!";
        roundEndPanel.SetActive(true); // פתיחת הפאנל של סיום הסיבוב
    }


    private void ShowFinalWinPanel(string finalWinner)
    {
        finalWinText.text = $"{finalWinner} wins the game!";
        finalWinPanel.SetActive(true);
    }


    private void EndGame()
    {
        // חישוב חוזק היד של השחקן והיריב
        CardSet playerHandStrength = HandEvaluator.EvaluateHand(ConvertToCardList(playerCards));
        CardSet opponentHandStrength = HandEvaluator.EvaluateHand(ConvertToCardList(opponentCards));

        string winner;

        // קביעת המנצח בסיבוב
        if (playerHandStrength > opponentHandStrength)
        {
            winner = "Player";
            playerWins++;
        }
        else if (opponentHandStrength > playerHandStrength)
        {
            winner = "Opponent";
            opponentWins++;
        }
        else
        {
            winner = "It's a tie!";
        }

        // עדכון הניצחונות בתצוגה
        UpdateWinText();

        // בדיקה אם מישהו הגיע לניצחון סופי
        if (playerWins >= maxWins || opponentWins >= maxWins)
        {
            // מישהו ניצח את המשחק כולו - מציגים את פאנל הניצחון הסופי
            string finalWinner = playerWins >= maxWins ? "Player" : "Opponent";
            ShowFinalWinPanel(finalWinner);
        }
        else
        {
            // אין עדיין ניצחון סופי - מציגים את פאנל סיום הסיבוב
            ShowRoundEndPanel(winner);

            //// איפוס המשחק והתחלת סיבוב חדש
            //StartNewRound();
        }
    }

    //===========================Multi-Player=====================================


        public void InitializeGame() { /* יישום */ }
        public void DrawCardForOpponent() { /* יישום */ }
        public void DiscardOpponentCard(int cardIndex) { /* יישום */ }
        public void UseOpponentSpecialCard(string cardType) { /* יישום */ }
        public void ExecuteTrade(string tradeData) { /* יישום */ }
        public List<Card> GetPlayerCards() { /* יישום */ return new List<Card>(); }
        public List<Card> GetOpponentCards() { /* יישום */ return new List<Card>(); }
        public void UpdateWinText(int playerWins, int opponentWins) { /* יישום */ }
   

}
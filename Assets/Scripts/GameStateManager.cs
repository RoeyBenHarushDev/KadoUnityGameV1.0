using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using com.shephertz.app42.gaming.multiplayer.client;
using com.shephertz.app42.gaming.multiplayer.client.events;
using AssemblyCSharp;

public class GameStateManager : MonoBehaviour
{
    private Listener myListener;
    private CardManager cardManager;

    public enum GameState
    {
        MainMenu,
        SinglePlayer,
        MultiPlayer,
        GameOver
    }

    public GameState currentState;
    private NetworkManager networkManager;
    private bool isPlayerTurn;
    private float turnTimer = 60f;

    // UI Elements
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI statusText;
    public Button endTurnButton;
    public Button drawCardButton;
    public GameObject gameOverPanel;
    public TextMeshProUGUI winnerText;
    public GameObject roundEndPanel;
    public TextMeshProUGUI roundEndText;
    public Button nextRoundButton;
    public GameObject finalWinPanel;
    public TextMeshProUGUI finalWinText;

    // Game variables
    private int playerWins = 0;
    private int opponentWins = 0;
    public int maxWins = 3;
    private int turnCount = 0;
    private int playerTurns = 0;
    private int opponentTurns = 0;
    private int maxTurns = 5;

    void Start()
    {
        networkManager = NetworkManager.Instance;
        if (networkManager == null)
        {
            Debug.LogError("NetworkManager instance is null!");
            return;
        }

        cardManager = FindObjectOfType<CardManager>();
        if (cardManager == null)
        {
            Debug.LogError("CardManager not found in the scene!");
            return;
        }

        networkManager.OnConnectionDone += OnMultiplayerConnectionEstablished;
        networkManager.OnRoomJoined += OnRoomJoined;
        networkManager.OnRoomCreated += OnRoomCreated;
        networkManager.OnGameStarted += OnMultiplayerGameStarted;
        networkManager.OnTurnChanged += OnTurnChanged;
        networkManager.OnMoveReceived += OnMoveReceived;
        networkManager.OnGameOver += OnGameOver;

        nextRoundButton.onClick.AddListener(StartNewRound);
        roundEndPanel.SetActive(false);
        finalWinPanel.SetActive(false);

        ChangeState(GameState.MainMenu);
    }

    void Update()
    {
        if (currentState == GameState.MultiPlayer && isPlayerTurn)
        {
            UpdateTurnTimer();
        }
    }

    public void ChangeState(GameState newState)
    {
        currentState = newState;

        switch (currentState)
        {
            case GameState.MainMenu:
                Debug.Log("Entered Main Menu");
                break;
            case GameState.SinglePlayer:
                Debug.Log("Entered Single Player Mode");
                StartSinglePlayerGame();
                break;
            case GameState.MultiPlayer:
                Debug.Log("Entered Multi-Player Mode");
                StartMultiPlayerGame();
                break;
            case GameState.GameOver:
                Debug.Log("Game Over");
                ShowGameOverPanel();
                break;
        }
    }

    void StartSinglePlayerGame()
    {
        Debug.Log("Starting Single Player Game...");
        cardManager.InitializeGame();
        UpdateUI();
    }

    private void StartMultiPlayerGame()
    {
        Debug.Log("Starting Multiplayer Game...");
        networkManager.Connect();
    }

    private void OnMultiplayerConnectionEstablished(bool isSuccess)
    {
        if (isSuccess)
        {
            Debug.Log("Connected to AppWarp successfully!");
            networkManager.GetAvailableRooms();
        }
        else
        {
            Debug.LogError("Failed to connect to AppWarp.");
            statusText.text = "Connection failed. Please try again.";
        }
    }

    private void OnRoomJoined(bool isSuccess, string roomId)
    {
        if (isSuccess)
        {
            Debug.Log("Joined room successfully: " + roomId);
            WarpClient.GetInstance().GetLiveRoomInfo(roomId);
            statusText.text = "Joined room. Waiting for other player...";
        }
        else
        {
            Debug.LogError("Failed to join the room.");
            statusText.text = "Failed to join room. Please try again.";
        }
    }


    private void OnRoomCreated(bool isSuccess, string roomId)
    {
        if (isSuccess)
        {
            Debug.Log("Room created successfully: " + roomId);
        }
        else
        {
            Debug.LogError("Failed to create room.");
            if (statusText != null)
            {
                statusText.text = "Failed to create room. Please try again.";
            }
        }
    }

    private void OnMultiplayerGameStarted(string sender, string roomId, string nextTurn)
    {
        Debug.Log($"Game started by: {sender}. Next turn: {nextTurn}");
        isPlayerTurn = (nextTurn == networkManager.userId);
        ResetTurnTimer();
        UpdateUI();
    }

    public void MakeMove(string move)
    {
        if (currentState == GameState.MultiPlayer && isPlayerTurn)
        {
            networkManager.SendMove(move);
            EndTurn();
        }
    }

    public void EndTurn()
    {
        if (currentState == GameState.MultiPlayer && isPlayerTurn)
        {
            cardManager.EnsurePlayerHasSevenCards();

            if (isPlayerTurn)
            {
                playerTurns++;
            }
            else
            {
                opponentTurns++;
            }

            UpdateUI();

            if (playerTurns >= maxTurns && opponentTurns >= maxTurns)
            {
                EndGame();
                return;
            }

            isPlayerTurn = false;
            networkManager.SendTurnChange(!isPlayerTurn);
            UpdateTurnDisplay();

            if (!isPlayerTurn)
            {
                StartCoroutine(OpponentTurn());
            }
        }
    }

    private void UpdateTurnTimer()
    {
        turnTimer -= Time.deltaTime;
        if (turnTimer <= 0)
        {
            EndTurn();
        }
        UpdateTimerUI();
    }

    private void ResetTurnTimer()
    {
        turnTimer = 60f;
        UpdateTimerUI();
    }

    private void UpdateTimerUI()
    {
        if (timerText != null)
        {
            timerText.text = $"Time left: {Mathf.CeilToInt(turnTimer)}s";
        }
    }

    private void OnTurnChanged(bool isMyTurn)
    {
        isPlayerTurn = isMyTurn;
        ResetTurnTimer();
        UpdateUI();
    }

    private void OnMoveReceived(string move)
    {
        Debug.Log($"Received move: {move}");

        string[] moveParts = move.Split(':');
        if (moveParts.Length < 2)
        {
            Debug.LogError("Invalid move format received");
            return;
        }

        string moveType = moveParts[0];
        string moveData = moveParts[1];

        switch (moveType)
        {
            case "DRAW":
                cardManager.DrawCardForOpponent();
                break;
            case "DISCARD":
                int cardIndex = int.Parse(moveData);
                cardManager.DiscardOpponentCard(cardIndex);
                break;
            case "SPECIAL":
                UseOpponentSpecialCard(moveData);
                break;
            case "TRADE":
                StartCoroutine(HandleOpponentTradeOffer(moveData));
                break;
            default:
                Debug.LogWarning($"Unknown move type: {moveType}");
                break;
        }

        UpdateUI();
    }

    private IEnumerator OpponentTurn()
    {
        yield return new WaitForSeconds(2f);
        statusText.text = "Waiting for opponent's move...";
    }

    private void UseOpponentSpecialCard(string cardType)
    {
        cardManager.UseOpponentSpecialCard(cardType);
    }

    private IEnumerator HandleOpponentTradeOffer(string tradeData)
    {
        statusText.text = "Opponent offered a trade. Do you accept?";
        // הצג UI לאישור או דחיית ההצעה

        bool? tradeAccepted = null;
        while (tradeAccepted == null)
        {
            yield return null;
        }

        if (tradeAccepted == true)
        {
            cardManager.ExecuteTrade(tradeData);
        }

        networkManager.SendMove($"TRADE_RESPONSE:{(tradeAccepted == true ? "ACCEPT" : "DECLINE")}");
    }

    private void EndGame()
    {
        CardSet playerHandStrength = HandEvaluator.EvaluateHand(cardManager.GetPlayerCards());
        CardSet opponentHandStrength = HandEvaluator.EvaluateHand(cardManager.GetOpponentCards());

        string winner;

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

        UpdateWinText();

        if (playerWins >= maxWins || opponentWins >= maxWins)
        {
            string finalWinner = playerWins >= maxWins ? "Player" : "Opponent";
            ShowFinalWinPanel(finalWinner);
        }
        else
        {
            ShowRoundEndPanel(winner);
        }
    }

    private void ShowRoundEndPanel(string winner)
    {
        roundEndText.text = $"{winner} wins this round!";
        roundEndPanel.SetActive(true);
    }

    private void ShowFinalWinPanel(string finalWinner)
    {
        finalWinText.text = $"{finalWinner} wins the game!";
        finalWinPanel.SetActive(true);
    }

    private void StartNewRound()
    {
        roundEndPanel.SetActive(false);
        ResetGame();
    }

    private void ResetGame()
    {
        isPlayerTurn = true;
        turnCount = 0;
        playerTurns = 0;
        opponentTurns = 0;

        cardManager.InitializeGame();
        UpdateUI();

        ChangeState(GameState.MultiPlayer);
    }

    private void OnGameOver(string winner)
    {
        Debug.Log($"Game Over. Winner: {winner}");
        if (winnerText != null)
        {
            winnerText.text = $"Winner: {winner}";
        }
        ShowGameOverPanel();
    }

    private void ShowGameOverPanel()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }

    private void UpdateUI()
    {
        cardManager.UpdateUI();
        UpdateTurnDisplay();
        UpdateTimerUI();
    }

    private void UpdateTurnDisplay()
    {
        if (statusText != null)
        {
            statusText.text = isPlayerTurn ? "Your Turn" : "Opponent's Turn";
        }
    }

    private void UpdateWinText()
    {
        // עדכון טקסט הניצחונות בממשק המשתמש
        if (cardManager != null)
        {
            cardManager.UpdateWinText(playerWins, opponentWins);
        }
    }

    private void OnDestroy()
    {
        if (networkManager != null)
        {
            networkManager.OnConnectionDone -= OnMultiplayerConnectionEstablished;
            networkManager.OnRoomJoined -= OnRoomJoined;
            networkManager.OnRoomCreated -= OnRoomCreated;
            networkManager.OnGameStarted -= OnMultiplayerGameStarted;
            networkManager.OnTurnChanged -= OnTurnChanged;
            networkManager.OnMoveReceived -= OnMoveReceived;
            networkManager.OnGameOver -= OnGameOver;
        }
    }
}
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MultiplayerUIManager : MonoBehaviour
{
    public GameObject mainMenuPanel;
    public GameObject multiplayerPanel;
    public GameObject gamePanel;

    public Button createRoomButton;
    public Button joinRoomButton;
    public TMP_InputField roomIdInput;
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI turnText;
    public TextMeshProUGUI opponentCardCountText;

    private NetworkManager networkManager;
    private CardManager cardManager;
    private GameStateManager gameStateManager;

    private string roomId;
    private string password = "ThePasswordIs123"; // Default password

    void Start()
    {
        networkManager = NetworkManager.Instance;
        gameStateManager = FindObjectOfType<GameStateManager>();
        cardManager = FindObjectOfType<CardManager>();

        createRoomButton.onClick.AddListener(CreateRoom);
        joinRoomButton.onClick.AddListener(JoinRoomButtonClicked);

        networkManager.OnConnectionDone += OnConnectionEstablished;
        networkManager.OnRoomJoined += OnRoomJoined;
        networkManager.OnGameStarted += OnGameStarted;
        networkManager.OnCreateRoomDone += OnCreateRoomDone;

        ShowMainMenu();
    }

    public void CreateRoom()
    {
        if (!networkManager.IsConnected())
        {
            Debug.LogError("Not connected to AppWarp server. Attempting to connect...");
            networkManager.OnConnectionDone += OnConnectedThenCreateRoom;  // Add listener to connection event
            networkManager.Connect();  // Try connecting
            return;
        }

        // Proceed with room creation if connected
        Debug.Log("Attempting to create room...");
        networkManager.CreateRoom();  // NetworkManager handles room creation
    }
    private void OnConnectedThenCreateRoom(bool isConnected)
    {
        if (isConnected)
        {
            Debug.Log("Connected! Now creating room...");
            networkManager.CreateRoom();  // Now it's safe to create a room
        }
        else
        {
            Debug.LogError("Failed to connect to server.");
        }

        // Unsubscribe from the event after using it
        networkManager.OnConnectionDone -= OnConnectedThenCreateRoom;
    }

    void StartSinglePlayerGame()
    {
        Debug.Log("Starting Single Player Game...");
        cardManager.InitializeGame();
        UpdateUI();
    }

    void OnConnectionEstablished(bool isSuccess)
    {
        if (isSuccess)
        {
            statusText.text = "Connected to AppWarp";
            ShowMultiplayerPanel();
        }
        else
        {
            statusText.text = "Failed to connect";
        }
    }

    private void OnCreateRoomDone(bool isSuccess, string roomId)
    {
        if (isSuccess)
        {
            statusText.text = "Room created: " + roomId;
        }
        else
        {
            statusText.text = "Failed to create room";
        }
    }

    void OnRoomJoined(bool isSuccess, string roomId)
    {
        if (isSuccess)
        {
            statusText.text = "Joined room: " + roomId;
        }
        else
        {
            statusText.text = "Failed to join room";
        }
    }

    void OnGameStarted(string sender, string roomId, string nextTurn)
    {
        ShowGamePanel();
        statusText.text = "Game started";
        UpdateTurnDisplay(nextTurn == networkManager.userId);
    }

    public void UpdateTurnDisplay(bool isLocalPlayerTurn)
    {
        turnText.text = isLocalPlayerTurn ? "Your Turn" : "Opponent's Turn";
    }

    public void UpdateOpponentCardCount(int count)
    {
        opponentCardCountText.text = "Opponent Cards: " + count;
    }

    void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        multiplayerPanel.SetActive(false);
        gamePanel.SetActive(false);
    }

    void ShowMultiplayerPanel()
    {
        mainMenuPanel.SetActive(false);
        multiplayerPanel.SetActive(true);
        gamePanel.SetActive(false);
    }

    private void JoinRoomButtonClicked()
    {
        string roomId = roomIdInput.text;
        if (!string.IsNullOrEmpty(roomId))
        {
            networkManager.JoinRoom(roomId, password);  // Use networkManager to join room
            statusText.text = "Joining room...";
        }
        else
        {
            statusText.text = "Please enter a room ID";
        }
    }

    private void UpdateUI()
    {
        // Implement your UI update logic here
    }

    void ShowGamePanel()
    {
        mainMenuPanel.SetActive(false);
        multiplayerPanel.SetActive(false);
        gamePanel.SetActive(true);
    }

    private void OnDestroy()
    {
        if (networkManager != null)
        {
            networkManager.OnConnectionDone -= OnConnectionEstablished;
            networkManager.OnRoomJoined -= OnRoomJoined;
            networkManager.OnGameStarted -= OnGameStarted;
            networkManager.OnCreateRoomDone -= OnCreateRoomDone;
        }
    }
}

using UnityEngine;
using com.shephertz.app42.paas.sdk.csharp;
using com.shephertz.app42.paas.sdk.csharp.app42Event;
using com.shephertz.app42.gaming.multiplayer.client;
using com.shephertz.app42.gaming.multiplayer.client.events;
using com.shephertz.app42.gaming.multiplayer.client.command;
using AssemblyCSharp;
using System;
using System.Collections;
using System.Collections.Generic;

public class NetworkManager : MonoBehaviour
{
    #region Singleton
    private static NetworkManager _instance;
    public static NetworkManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<NetworkManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("NetworkManager");
                    _instance = go.AddComponent<NetworkManager>();
                }
            }
            return _instance;
        }
    }
    #endregion

    #region Private Fields
    private string apiKey = "4c8887238563630dbb036f9351dabbd8e9d70cfa72d2a77f56600ea437908aa9";
    private string secretKey = "f14df2e6c9fa5b88cff7dd4772eb4f045f044ac96af973cef184b06c59daa901";
    private Listener myListener;
    private string roomId = string.Empty;
    private string password = "ThePasswordIs123";  // סיסמת חדר
    private const float ROOM_CREATION_TIMEOUT = 10f;
    private const int MAX_RETRY_ATTEMPTS = 3;
    private const float RETRY_DELAY = 2f;
    private const float CONNECTION_TIMEOUT = 10f;
    #endregion

    #region Public Fields
    public string userId = string.Empty;
    #endregion

    #region Events
    public event Action<bool> OnConnectionDone;
    public event Action<bool, MatchedRoomsEvent> OnRoomsReceived;
    public event Action<bool, string> OnCreateRoomDone;
    public event Action<bool, string> OnRoomJoined;
    public event Action<bool, string> OnRoomCreated;
    public event Action<string, string, string> OnGameStarted;
    public event Action<string, string> OnGameStopped;
    public event Action<string> OnMoveReceived;
    public event Action<bool> OnTurnChanged;
    public event Action<string> OnGameOver;
    #endregion

    #region Unity Lifecycle Methods
    private void Awake()
    {
        InitializeAppWarp();
        ConnectToServer();
    }

    private void Update()
    {
        WarpClient.GetInstance().Update();
    }

    private void OnApplicationQuit()
    {
        LeaveRoom();
        Disconnect();
    }
    #endregion

    #region Initialization Methods
    private void InitializeApp42()
    {
        App42API.Initialize(apiKey, secretKey);
        App42API.EnableEventService(true);
        App42API.SetLoggedInUser(userId);
    }

    private void InitializeAppWarp()
    {
        if (myListener == null)
            myListener = new Listener();

        WarpClient.initialize(apiKey, secretKey);
        AddListeners();
        SetupEventHandlers();
    }

    private void AddListeners()
    {
        WarpClient.GetInstance().AddConnectionRequestListener(myListener);
        WarpClient.GetInstance().AddChatRequestListener(myListener);
        WarpClient.GetInstance().AddUpdateRequestListener(myListener);
        WarpClient.GetInstance().AddLobbyRequestListener(myListener);
        WarpClient.GetInstance().AddNotificationListener(myListener);
        WarpClient.GetInstance().AddRoomRequestListener(myListener);
        WarpClient.GetInstance().AddZoneRequestListener(myListener);
        WarpClient.GetInstance().AddTurnBasedRoomRequestListener(myListener);
    }

    private void SetupEventHandlers()
    {
        Listener.OnConnect += (success) => OnConnectionDone?.Invoke(success);
        Listener.OnRoomsInRange += (success, eventObj) => OnRoomsReceived?.Invoke(success, eventObj);
        Listener.OnJoinRoom += (success, roomId) => OnRoomJoined?.Invoke(success, roomId);
        Listener.OnCreateRoom += (success, roomId) => OnRoomCreated?.Invoke(success, roomId);
        Listener.OnGameStarted += (sender, roomId, nextTurn) => OnGameStarted?.Invoke(sender, roomId, nextTurn);
        Listener.OnGameStopped += (sender, roomId) => OnGameStopped?.Invoke(sender, roomId);
        Listener.OnMoveCompleted += (moveEvent) => OnMoveReceived?.Invoke(moveEvent.getMoveData());
    }
    #endregion

    #region Connection Methods
    public void Connect()
    {
        userId = System.Guid.NewGuid().ToString();
        Debug.Log($"Attempting to connect with User ID: {userId}");
        Debug.Log($"API Key: {apiKey.Substring(0, 5)}... (truncated for security)");
        Debug.Log($"Secret Key: {secretKey.Substring(0, 5)}... (truncated for security)");
        WarpClient.GetInstance().Connect(userId);
        StartCoroutine(ConnectionTimeout());
    }

    private void ConnectToServer()
    {
        userId = System.Guid.NewGuid().ToString();
        WarpClient.GetInstance().Connect(userId);
        Debug.Log("Connecting...");
    }

    private IEnumerator ConnectWithRetry()
    {
        for (int attempt = 0; attempt < MAX_RETRY_ATTEMPTS; attempt++)
        {
            if (attempt > 0)
            {
                Debug.Log($"Retrying connection (Attempt {attempt + 1}/{MAX_RETRY_ATTEMPTS})");
                yield return new WaitForSeconds(RETRY_DELAY);
            }

            userId = System.Guid.NewGuid().ToString();
            Debug.Log($"Attempting to connect with User ID: {userId}");
            Debug.Log($"API Key: {apiKey.Substring(0, 5)}... (truncated for security)");
            Debug.Log($"Secret Key: {secretKey.Substring(0, 5)}... (truncated for security)");

            WarpClient.GetInstance().Connect(userId);

            yield return StartCoroutine(WaitForConnection());
        }

        Debug.LogError($"Failed to connect after {MAX_RETRY_ATTEMPTS} attempts. Please check your network and AppWarp settings.");
        OnConnectionDone?.Invoke(false);
    }

    private IEnumerator WaitForConnection()
    {
        float elapsedTime = 0f;
        while (elapsedTime < CONNECTION_TIMEOUT)
        {
            int connectionState = WarpClient.GetInstance().GetConnectionState();
            Debug.Log($"Current connection state: {connectionState}");

            if (connectionState == WarpConnectionState.CONNECTED)
            {
                Debug.Log("Successfully connected to AppWarp server.");
                OnConnectionDone?.Invoke(true);
                yield break;
            }
            else if (connectionState == WarpConnectionState.DISCONNECTED)
            {
                Debug.LogError("Disconnected from AppWarp server.");
                yield break;
            }

            elapsedTime += Time.deltaTime;
            yield return new WaitForSeconds(0.5f);
        }

        Debug.LogError($"Connection timed out after {CONNECTION_TIMEOUT} seconds.");
    }

    private IEnumerator ConnectionTimeout()
    {
        float elapsedTime = 0f;
        while (elapsedTime < 10f)
        {
            if (IsConnected())
            {
                Debug.Log("Successfully connected to AppWarp server.");
                yield break;
            }
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        Debug.LogError("Connection timed out. Please check your network and AppWarp settings.");
        OnConnectionDone?.Invoke(false);
    }

    public bool IsConnected()
    {
        return WarpClient.GetInstance().GetConnectionState() == 0;
    }

    public void Disconnect()
    {
        WarpClient.GetInstance().Disconnect();
    }
    #endregion

    #region Room Methods
    public void GetAvailableRooms()
    {
        WarpClient.GetInstance().GetRoomsInRange(1, 2);
    }

    public void CreateRoom()
    {
        if (!IsConnected())
        {
            Debug.Log("Not connected to AppWarp server. Attempting to connect...");
            OnConnectionDone += OnConnectedThenCreateRoom;
            Connect();
            return;
        }

        Debug.Log("Attempting to create room...");
        roomId = "Room_" + System.Guid.NewGuid().ToString();
        Debug.Log($"Generated room ID: {roomId}");
        WarpClient.GetInstance().CreateTurnRoom(roomId, password, 2, null, 60);
        StartCoroutine(RoomCreationTimeout());
    }

    private void OnConnectedThenCreateRoom(bool isConnected)
    {
        if (isConnected)
        {
            Debug.Log("Connected! Now creating room...");
            CreateRoom();
        }
        else
        {
            Debug.LogError("Failed to connect to server.");
        }

        OnConnectionDone -= OnConnectedThenCreateRoom;
    }

    public void JoinRoom(string roomIdToJoin, string password)
    {
        WarpClient.GetInstance().JoinRoom(roomIdToJoin);
    }

    public void LeaveRoom()
    {
        if (!string.IsNullOrEmpty(roomId))
        {
            WarpClient.GetInstance().LeaveRoom(roomId);
        }
    }

    public bool IsRoomFull()
    {
        if (!string.IsNullOrEmpty(roomId))
        {
            WarpClient.GetInstance().GetLiveRoomInfo(roomId);
            return Listener.currentPlayers >= 2;
        }
        return false;
    }

    private IEnumerator RoomCreationTimeout()
    {
        yield return new WaitForSeconds(ROOM_CREATION_TIMEOUT);
        if (string.IsNullOrEmpty(roomId))
        {
            Debug.LogError("Room creation timed out");
        }
    }
    #endregion

    #region Game Methods
    public void StartGame()
    {
        WarpClient.GetInstance().startGame();
    }

    public void SendMove(string move)
    {
        WarpClient.GetInstance().sendMove(move);
    }

    public void SendTurnChange(bool isNextPlayerTurn)
    {
        string turnChangeData = isNextPlayerTurn.ToString();
        WarpClient.GetInstance().sendMove(turnChangeData);
        OnTurnChanged?.Invoke(isNextPlayerTurn);
    }

    public void SendGameOver(string winner)
    {
        string gameOverData = $"GAMEOVER:{winner}";
        WarpClient.GetInstance().sendMove(gameOverData);
        OnGameOver?.Invoke(winner);
    }

    public void SendRestartRequest()
    {
        string restartData = "RESTART";
        WarpClient.GetInstance().sendMove(restartData);
    }
    #endregion

    #region Event Tracking Methods
    public void TrackEvent(string eventName, Dictionary<string, object> properties)
    {
        EventService eventService = App42API.BuildEventService();
        eventService.TrackEvent(eventName, properties, new UnityCallBack());
    }

    public void TrackPurchaseEvent(string playerName, int revenue)
    {
        string eventName = "Purchase";
        Dictionary<string, object> properties = new Dictionary<string, object>
        {
            { "Name", playerName },
            { "Revenue", revenue }
        };

        EventService eventService = App42API.BuildEventService();
        eventService.TrackEvent(eventName, properties, new UnityCallBack());
    }
    #endregion

    #region Callback Methods
    public void OnConnectDone(ConnectEvent eventObj)
    {
        if (eventObj.getResult() == WarpResponseResultCode.SUCCESS)
        {
            Debug.Log("Connection success");
        }
        else
        {
            Debug.LogError($"Connection failed. Error code: {eventObj.getResult()}, Description: {eventObj.getResult().ToString()}");
        }
    }

    public void OnDisconnectDone(ConnectEvent eventObj)
    {
        Debug.Log($"Disconnected from server. Reason: {eventObj.getResult()}, Description: {eventObj.getResult().ToString()}");
    }

    public void HandleCreateRoomDone(RoomEvent eventObj)
    {
        bool isSuccess = eventObj.getResult() == 0;
        string createdRoomId = isSuccess ? eventObj.getData().getId() : null;
        Debug.Log($"Room creation result: {(isSuccess ? "Success" : "Failure")}. Room ID: {createdRoomId}");

        if (isSuccess)
        {
            Debug.Log("Room created successfully: " + createdRoomId);
            JoinRoom(createdRoomId, password);

            Dictionary<string, object> properties = new Dictionary<string, object>
            {
                { "RoomID", createdRoomId },
                { "Player", userId }
            };
            TrackEvent("RoomCreated", properties);
        }
        else
        {
            Debug.LogError($"Failed to create room. Error code: {eventObj.getResult()}");
        }
    }

    public void HandleJoinRoomDone(RoomEvent eventObj)
    {
        bool isSuccess = eventObj.getResult() == 0;
        string joinedRoomId = isSuccess ? eventObj.getData().getId() : null;
        OnRoomJoined?.Invoke(isSuccess, joinedRoomId);

        if (isSuccess)
        {
            Debug.Log("Joined room successfully: " + joinedRoomId);
        }
        else
        {
            Debug.LogError("Failed to join room: " + eventObj.getResult());
        }
    }
    #endregion
}

public class UnityCallBack : App42CallBack
{
    public void OnSuccess(object response)
    {
        App42Response app42Response = (App42Response)response;
        Debug.Log("App42Response Is : " + app42Response);
    }

    public void OnException(Exception e)
    {
        Debug.Log("Exception : " + e);
    }
}
//using AssemblyCSharp;
//using com.shephertz.app42.gaming.multiplayer.client;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class MenuLogic : MonoBehaviour
//{
//    private string apiKey = "a0086f7757230c52abfa801286eb9687256b2014c1a338f1e328e2e78c8812fc";
//    private string secretKey = "239d0cee8034640774b5e059925d31c52bf2766fd97211150210c44d0c16218d";

//    private Listener myListener;
//    private string userId = string.Empty;
//    private string roomId = string.Empty;
//    private string password = "your_password";  // צריך להיות זהה בין שני המשתמשים

//    private void OnEnable()
//    {
//        Listener.OnConnect += OnConnect;
//        Listener.OnRoomsInRange += OnRoomsInRange;
//        Listener.OnJoinRoom += OnJoinRoom;
//        Listener.OnCreateRoom += OnCreateRoom;
//        Listener.OnGameStarted += OnGameStarted;
//        Listener.OnGameStopped += OnGameStopped;
//    }

//    private void OnDisable()
//    {
//        Listener.OnConnect -= OnConnect;
//        Listener.OnRoomsInRange -= OnRoomsInRange;
//        Listener.OnJoinRoom -= OnJoinRoom;
//        Listener.OnCreateRoom -= OnCreateRoom;
//        Listener.OnGameStarted -= OnGameStarted;
//        Listener.OnGameStopped -= OnGameStopped;
//    }

//    private void Awake()
//    {
//        if (myListener == null)
//            myListener = new Listener();

//        WarpClient.initialize(apiKey, secretKey);
//        WarpClient.GetInstance().AddConnectionRequestListener(myListener);
//        WarpClient.GetInstance().AddChatRequestListener(myListener);
//        WarpClient.GetInstance().AddUpdateRequestListener(myListener);
//        WarpClient.GetInstance().AddLobbyRequestListener(myListener);
//        WarpClient.GetInstance().AddNotificationListener(myListener);
//        WarpClient.GetInstance().AddRoomRequestListener(myListener);
//        WarpClient.GetInstance().AddZoneRequestListener(myListener);
//        WarpClient.GetInstance().AddTurnBasedRoomRequestListener(myListener);

//        userId = System.Guid.NewGuid().ToString();
//        WarpClient.GetInstance().Connect(userId);
//        Debug.Log("Connecting...");
//    }

//    void Start()
//    {
//        // ברגע שהחיבור יצליח נתחיל את תהליך קבלת החדרים.
//    }

//    // יטפל בהצלחה של החיבור
//    private void OnConnect(bool isSuccess)
//    {
//        if (isSuccess)
//        {
//            Debug.Log("Connected successfully!");
//            // בקש חדרים עם מינימום 1 שחקן ומקסימום 2 שחקנים
//            WarpClient.GetInstance().GetRoomsInRange(1, 2);
//        }
//        else
//        {
//            Debug.LogError("Failed to connect.");
//        }
//    }

//    // יטפל בקבלת רשימת החדרים
//    private void OnRoomsInRange(bool isSuccess, MatchedRoomsEvent eventObj)
//    {
//        if (isSuccess && eventObj.getRoomsData().Length > 0)
//        {
//            Debug.Log("Rooms found: " + eventObj.getRoomsData().Length);
//            bool foundRoom = false;

//            foreach (var room in eventObj.getRoomsData())
//            {
//                // בדיקת סיסמא
//                if (room.getRoomOwner() == password)
//                {
//                    Debug.Log("Found room with matching password, joining...");
//                    roomId = room.getId();
//                    WarpClient.GetInstance().JoinRoom(roomId);
//                    foundRoom = true;
//                    break;
//                }
//            }

//            if (!foundRoom)
//            {
//                CreateNewRoom();
//            }
//        }
//        else
//        {
//            Debug.Log("No rooms found, creating a new room...");
//            CreateNewRoom();
//        }
//    }

//    // יצירת חדר חדש
//    private void CreateNewRoom()
//    {
//        roomId = "Room_" + System.Guid.NewGuid().ToString(); // יצירת ID ייחודי לחדר
//        WarpClient.GetInstance().CreateTurnRoom(roomId, password, 2, null, 60);
//        Debug.Log("Creating a new room...");
//    }

//    // יטפל בהצטרפות לחדר
//    private void OnJoinRoom(bool isSuccess, string _RoomId)
//    {
//        if (isSuccess)
//        {
//            Debug.Log("Joined room successfully: " + _RoomId);
//            WarpClient.GetInstance().SubscribeRoom(_RoomId);

//            // ברגע שהשחקנים בחדר, המשחק יתחיל
//            if (WarpClient.GetInstance().GetLiveRoomInfo(_RoomId).getJoinedUsers().Length == 2)
//            {
//                StartGame();
//            }
//        }
//        else
//        {
//            Debug.LogError("Failed to join the room.");
//        }
//    }

//    // יצירת חדר בהצלחה
//    private void OnCreateRoom(bool isSuccess, string _RoomId)
//    {
//        if (isSuccess)
//        {
//            Debug.Log("Room created successfully: " + _RoomId);
//            WarpClient.GetInstance().JoinRoom(_RoomId);
//        }
//        else
//        {
//            Debug.LogError("Failed to create room.");
//        }
//    }

//    // התחלת משחק
//    private void StartGame()
//    {
//        WarpClient.GetInstance().StartGame();
//        Debug.Log("Game has started!");
//    }

//    // סיום משחק
//    private void OnGameStarted(string sender, string _RoomId, string nextTurn)
//    {
//        Debug.Log("Game started by: " + sender);
//        Debug.Log("Next turn is: " + nextTurn);
//    }

//    // סיום המשחק
//    private void OnGameStopped(string sender, string _RoomId)
//    {
//        Debug.Log("Game stopped by: " + sender);
//    }

//    void Update()
//    {
//        // שמירה על חיבור עדכני
//        WarpClient.GetInstance().Update();
//    }
//}

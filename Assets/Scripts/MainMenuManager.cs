using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    public static string sceneToLoad;

    public GameObject gameTypePanel;
    public GameObject multiplayerOptionsPanel;
    public Button createRoomButton;
    public Button joinRoomButton;
    public TMP_InputField roomCodeInput;

    private NetworkManager networkManager;


    private void Awake()
    {
        networkManager = NetworkManager.Instance;
        if (networkManager == null)
        {
            Debug.LogError("NetworkManager instance is null. Ensure it's properly initialized.");
            // Consider creating a NetworkManager instance here if it doesn't exist
        }
    }


    void Start()
    {
        networkManager = NetworkManager.Instance;
        createRoomButton.onClick.AddListener(CreateRoom);
        joinRoomButton.onClick.AddListener(JoinRoom);
    }

    public void OnSinglePlayerClicked()
    {
        LoadSinglePlayerScene();
    }

    public void OnMultiPlayerClicked()
    {
        gameTypePanel.SetActive(false);
        multiplayerOptionsPanel.SetActive(true);
    }

    public void closeMultyPlayerPanel()
    {
        gameTypePanel.SetActive(true);
        multiplayerOptionsPanel.SetActive(false);
    }
    private void CreateRoom()
    {
        networkManager.CreateRoom();
    }

    private void JoinRoom()
    {
        string roomCode = roomCodeInput.text;
        if (!string.IsNullOrEmpty(roomCode))
        {
            // כאן גם צריך להוסיף את הסיסמה
            string password = "ThePasswordIs123"; // סיסמה קבועה לדוגמה
            networkManager.JoinRoom(roomCode, password);
            LoadMultiPlayerScene();
        }
        else
        {
            Debug.LogWarning("Please enter a room code");
            // כאן אפשר להוסיף הודעת שגיאה למשתמש
        }
    }

    private void OnEnable()
    {
        if (networkManager != null)
        {
            networkManager.OnCreateRoomDone += OnRoomCreated;
        }
        else
        {
            Debug.LogError("NetworkManager is still null in MainMenuManager.OnEnable()");
        }
    }

    private void OnDisable()
    {
        networkManager.OnCreateRoomDone -= OnRoomCreated;
    }

    private void OnRoomCreated(bool isSuccess, string roomId)
    {
        if (isSuccess)
        {
            LoadMultiPlayerScene();
        }
        else
        {
            Debug.LogError("Failed to create room");
            // Show error message to the user
        }
    }

    public void LoadSinglePlayerScene()
    {
        sceneToLoad = "SinglePlayerScene";
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            SceneManager.LoadScene("LoadingScene");
        }
        else
        {
            Debug.LogError("Scene to load is not set!");
        }
    }

    public void LoadMultiPlayerScene()
    {
        sceneToLoad = "MultiPlayerScene";
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            SceneManager.LoadScene("LoadingScene");
        }
        else
        {
            Debug.LogError("Scene to load is not set!");
        }
    }
}
using UnityEngine;

public class MuteButton : MonoBehaviour
{
    private MusicManager musicManager;

    void Start()
    {
        // מחפש את אובייקט MusicManager שנמצא בסצנה (שלא נהרס בין סצנות)
        musicManager = FindObjectOfType<MusicManager>();
    }

    public void ToggleMute()
    {
        if (musicManager != null)
        {
            musicManager.ToggleMute(); // מפעיל או משתיק את המוזיקה
        }
        else
        {
            Debug.LogError("MusicManager not found!");
        }
    }
}

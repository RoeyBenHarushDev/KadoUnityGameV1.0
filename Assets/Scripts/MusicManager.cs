using UnityEngine;

public class MusicManager : MonoBehaviour
{
    private static MusicManager instance = null; // שמירת המופע (Instance) של אובייקט המוזיקה
    public AudioSource BackgroundMusic; // מקור המוזיקה
    private bool isMute = false; // משתנה לניהול מצב השתקה

    void Awake()
    {
        // בדיקה אם יש כבר מופע של MusicManager
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // לא להרוס את אובייקט המוזיקה כשעוברים בין סצנות
        }
        else
        {
            Destroy(gameObject); // אם כבר יש מופע קיים, לא ליצור חדש
        }
    }

    // פונקציה להחלפת מצב המוזיקה בין השתקה וניגון
    public void ToggleMute()
    {
        if (isMute)
        {
            BackgroundMusic.Play(); // הפעלת המוזיקה אם היא מושתקת
            isMute = false;
        }
        else
        {
            BackgroundMusic.Pause(); // השתקת המוזיקה אם היא מתנגנת
            isMute = true;
        }
    }
}

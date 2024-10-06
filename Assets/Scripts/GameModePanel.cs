using UnityEngine;

public class PanelManager : MonoBehaviour
{
    public GameObject panel; // הפאנל שאנחנו רוצים להציג או להסתיר

    // פונקציה להצגת הפאנל
    public void ShowPanel()
    {
        panel.SetActive(true); // מציג את הפאנל
    }

    // פונקציה להסתרת הפאנל
    public void HidePanel()
    {
        panel.SetActive(false); // מסתיר את הפאנל
    }
}

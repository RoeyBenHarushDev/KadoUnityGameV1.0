using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static string sceneToLoad;

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
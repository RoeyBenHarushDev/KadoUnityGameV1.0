using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class LoadingManager : MonoBehaviour
{
    public TextMeshProUGUI loadingText;
    public float colorChangeInterval = 0.1f;
    public float minimumLoadingTime = 1f; // מינימום זמן טעינה בשניות
    private Color32[] colors = { new Color32(255, 0, 0, 255), new Color32(0, 255, 0, 255),
                                 new Color32(0, 0, 255, 255), new Color32(255, 255, 0, 255),
                                 new Color32(0, 255, 255, 255), new Color32(255, 0, 255, 255) };
    private int currentColorIndex = 0;

    void Start()
    {
        StartCoroutine(LoadSceneAsync());
        StartCoroutine(ChangeTextColor());
    }

    IEnumerator LoadSceneAsync()
    {
        if (string.IsNullOrEmpty(MainMenuManager.sceneToLoad))
        {
            Debug.LogError("Scene to load is not set!");
            yield break;
        }

        AsyncOperation operation = SceneManager.LoadSceneAsync(MainMenuManager.sceneToLoad);
        if (operation == null)
        {
            Debug.LogError("Failed to start scene loading operation!");
            yield break;
        }

        operation.allowSceneActivation = false;
        float startTime = Time.time;

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            loadingText.text = $"Loading... {progress * 100:F0}%";

            if (operation.progress >= 0.9f && Time.time - startTime >= minimumLoadingTime)
            {
                loadingText.text = "Loading complete!";
                operation.allowSceneActivation = true;
            }

            yield return null;
        }
    }

    IEnumerator ChangeTextColor()
    {
        while (true)
        {
            loadingText.color = colors[currentColorIndex];
            currentColorIndex = (currentColorIndex + 1) % colors.Length;
            yield return new WaitForSeconds(colorChangeInterval);
        }
    }
}
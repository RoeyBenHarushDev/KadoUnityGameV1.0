using UnityEngine;
using UnityEngine.SceneManagement;

public class RestartManager : MonoBehaviour
{
    public void OnRestartButtonClicked()
    {
        CardManager.Instance.ShowRestartConfirmation();
    }
}
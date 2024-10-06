using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitGame : MonoBehaviour
{
    public void Exit()
    {
        Application.Quit();

        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #endif  
    }
}

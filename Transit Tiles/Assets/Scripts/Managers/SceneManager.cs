using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneManager : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void LoadMenuScene()
    {

    }

    public void LoadGameScene()
    {
    
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}

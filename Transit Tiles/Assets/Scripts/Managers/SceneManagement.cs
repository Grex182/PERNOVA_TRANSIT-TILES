using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagement : Singleton<SceneManagement>
{
    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    public void LoadMenuScene()
    {
        GameManager.Instance.gameState = GameState.GameMenu;
        SceneManager.LoadSceneAsync(0);
    }

    public void LoadTutorialScene()
    {
        // LoadingScreen.Instance.SwitchToScene(sceneBuildIndices[1]);
    }

    public IEnumerator LoadGameScene()
    {
        GameManager.Instance.gameState = GameState.GameInit;

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("MainGame");
        asyncLoad.allowSceneActivation = false;

        // Wait until load reaches 90%
        while (asyncLoad.progress < 0.9f)
        {
            Debug.Log("Loading progress: " + asyncLoad.progress);
            yield return null;
        }

        Debug.Log("Scene is ready to activate");

        yield return new WaitForSeconds(0.5f);

        bool sceneActivated = false;

        asyncLoad.completed += (op) =>
        {
            Debug.Log("Scene activated. Starting game.");
            GameManager.Instance.gameState = GameState.GameStart;
            GameManager.Instance.StartGame();
            sceneActivated = true;
        };

        asyncLoad.allowSceneActivation = true;

        // Wait until callback confirms scene activation
        while (!sceneActivated)
        {
            yield return null;
        }
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}

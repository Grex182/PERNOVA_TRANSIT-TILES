using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagement : Singleton<SceneManagement>
{
    private void Awake()
    {
        // Ensure only one instance exists
        if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
    }

    public void LoadMenuScene()
    {
        ResetGameState();
        StartCoroutine(LoadSceneCoroutine(0, GameState.GameMenu));
    }

    public void LoadTutorialScene()
    {
        // LoadingScreen.Instance.SwitchToScene(sceneBuildIndices[1]);
    }

    public void LoadGameScene()
    {
        StartCoroutine(LoadGameSceneCoroutine());
    }

    private IEnumerator LoadGameSceneCoroutine()
    {
        GameManager.Instance.gameState = GameState.GameInit;

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("GameScene");
        asyncLoad.allowSceneActivation = false;

        while (asyncLoad.progress < 0.9f)
        {
            Debug.Log($"Loading progress: {asyncLoad.progress * 100}%");
            yield return null;
        }

        Debug.Log("Scene ready for activation");
        yield return new WaitForSeconds(0.5f); // Optional delay

        // Activate scene
        asyncLoad.allowSceneActivation = true;

        // Wait until scene is fully loaded
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // Scene is now fully loaded
        Debug.Log("Scene activation complete");
        GameManager.Instance.gameState = GameState.GameStart;
        GameManager.Instance.StartGame();
    }

    private IEnumerator LoadSceneCoroutine(int sceneIndex, GameState targetState)
    {
        // Reset current state
        ResetGameState();

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneIndex);

        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        GameManager.Instance.gameState = targetState;
    }

    private void ResetGameState()
    {
        // Clean up any persistent objects if needed
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetGame();
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

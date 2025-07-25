using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagement : MonoBehaviour
{
    public static SceneManagement Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);  // Destroy any duplicates
        }
    }

    public void LoadMenuScene()
    {
        ResetGameState();
        StartCoroutine(LoadMenuSceneCoroutine());
    }

    public void LoadTutorialScene()
    {
        StartCoroutine(LoadTutorialSceneCoroutine());
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
        AudioManager.Instance.PlayBGM(AudioManager.Instance.musicClips[0]);
        GameManager.Instance.StartGame();
    }

    private IEnumerator LoadTutorialSceneCoroutine()
    {
        GameManager.Instance.gameState = GameState.GameTutorial;

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("TutorialScene");
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
        AudioManager.Instance.PlayBGM(AudioManager.Instance.musicClips[0]);
    }

    private IEnumerator LoadMenuSceneCoroutine()
    {
        GameManager.Instance.gameState = GameState.GameEnded; // Optional: Set loading state

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(0);
        asyncLoad.allowSceneActivation = false;

        // Wait until the scene is almost loaded (progress reaches 0.9)
        while (asyncLoad.progress < 0.9f)
        {
            Debug.Log($"Loading menu progress: {asyncLoad.progress * 100}%");
            yield return null;
        }

        Debug.Log("Menu scene ready for activation");
        yield return new WaitForSeconds(0.5f); // Optional delay (can be removed)

        // Activate the scene
        asyncLoad.allowSceneActivation = true;

        // Wait until fully loaded
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        Debug.Log("Menu scene activation complete");

        // Play menu music if needed
        //AudioManager.Instance.PlayBGM(AudioManager.Instance.menuMusicClip);

        GameManager.Instance.gameState = GameState.GameMenu;
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

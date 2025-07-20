using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : Singleton<GameOverManager>
{
    [SerializeField] private GameObject gameOverCanvas;

    private Animator anim;

    private void Awake()
    {
        anim = gameOverCanvas.GetComponentInChildren<Animator>();

        gameOverCanvas.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            GameOver();
        }
    }

/*    public void Retry()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }*/

    public void BackToMainMenu()
    {
        Debug.Log("Went back to main menu hehe.");
    }

    public void GameOver()
    {
        gameOverCanvas.SetActive(true);
        UiManager.Instance.SetGameOverScoreText(LevelManager.Instance.currentScore);
        //Add pausing thing here or smtn
    }
}

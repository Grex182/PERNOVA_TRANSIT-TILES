using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;

public enum GameState
{
    GameMenu,
    GameTutorial,
    GameInit,
    GameStart,
    GameReset,
    GameEnded
}

public class GameManager : Singleton<GameManager>
{
    public GameState gameState;

    private void Awake()
    {
        // Ensure only one instance exists
        if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);

        gameState = GameState.GameMenu;
    }

    private void Update()
    {
        switch (gameState)
        {
            case GameState.GameMenu:
                //if (AudioManager.Instance != null)
                //{
                //    AudioManager.Instance.PlayBGM(AudioManager.Instance.musicClips[0]);
                //}
                break;
            case GameState.GameTutorial:
                // Handle tutorial logic
                break;
            case GameState.GameInit:
                
                break;
            case GameState.GameStart:
                break;
            case GameState.GameReset:
                // Handle game reset logic
                break;
            case GameState.GameEnded:
                // Handle game end logic
                break;
        }
    }

    public void StartGame()
    {
        LevelManager.Instance.StartGameFlow();
        Debug.Log("Start Game called");
        // Call all spawners
    }

    public void EndGame()
    {
        // Clean Destroys, arrays, lists
        // Pause game
    }

    public void ResetGame()
    {
        // Call InitializeGame
    }
}

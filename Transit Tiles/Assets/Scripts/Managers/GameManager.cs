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
        DontDestroyOnLoad(this.gameObject);

        gameState = GameState.GameStart;
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

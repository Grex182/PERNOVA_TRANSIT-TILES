using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;

public enum GameState
{
    GameTutorial,
    GameInit,
    GameStart,
    GameReset,
    GameEnded
}

public class GameManager : Singleton<GameManager>
{
    public GameState gameState;

    [Header("Board References")]
    [SerializeField] Board _board;
    public Board Board { get { return _board; } set { _board = value; } }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        InitializeGame();
    }

    public void InitializeGame()
    {
        gameState = GameState.GameInit;

        LevelManager.Instance.InitializeLevel();
        WorldGenerator.Instance.InitializeWorld();
    }

    public void StartGame()
    {
        gameState = GameState.GameStart;
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

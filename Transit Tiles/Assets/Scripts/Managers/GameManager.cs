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
    public GameState gameState; // REMOVE

    [Header("Board References")]
    [SerializeField] Board _board;
    public Board Board { get { return _board; } set { _board = value; } }

    #region MANAGER SCRIPT REFERENCES
    [SerializeField] StationManager _stationManager;
    public StationManager StationManager { get { return _stationManager; } set { _stationManager = value; } }

    [SerializeField] PublicRatingManager _publicRatingManager;
    public PublicRatingManager PublicRatingManager { get { return _publicRatingManager; } set { _publicRatingManager = value; } }

    [SerializeField] ScoreManager _scoreManager;
    public ScoreManager ScoreManager { get { return _scoreManager; } set { _scoreManager = value; } }

    [SerializeField] StageSpawner _stageSpawner;
    public StageSpawner StageSpawner { get { return _stageSpawner; } set { _stageSpawner = value; } }
    #endregion

    private void Awake()
    {
        //instance = this;
        DontDestroyOnLoad(gameObject);
    }

    

    private void Start()
    {
        //instance = this;
        gameState = GameState.GameInit; // Initialize game
    }

    public void InitializeGame()
    {
        // Populate with variables that set to initial value
        // Call all Initialize(); functions under every script
    }

    public void StartGame()
    {
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

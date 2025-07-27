using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;
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

public enum SelectionMode
{
    Hold,
    Toggle
}

public enum ColorblindMode
{
    Enabled,
    Disabled
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public GameState gameState;

    public SelectionMode selectionMode;
    public ColorblindMode colorblindMode;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            gameState = GameState.GameMenu;
        }
        else
        {
            Destroy(gameObject);  // Destroy any duplicates
        }

        gameState = GameState.GameMenu;

        // Default settings
        selectionMode = SelectionMode.Toggle;
        colorblindMode = ColorblindMode.Disabled;
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

    public void SetSelectionMode(float value)
    {
        selectionMode = (value == 0) ? SelectionMode.Toggle : SelectionMode.Hold;
    }

    public void SetColorblindMode(float value)
    {
        colorblindMode = (value == 0) ? ColorblindMode.Disabled : ColorblindMode.Enabled;

        if (PassengerSpawner.Instance == null) { return; }

        PassengerSpawner spawner = PassengerSpawner.Instance;
        SetPassengerUI(spawner.trainParent.transform);
        SetPassengerUI(spawner.stationPassengersParent.transform);
    }

    private void SetPassengerUI(Transform parent)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            child.GetComponent<PassengerUI>().SetColorblindCanvasState();
        }
    }
}

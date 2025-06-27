using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Rendering.InspectorCurveEditor;
using static UnityEngine.CullingGroup;

public enum MovementState
{
    Accelerate,
    Decelerate,
    Station,
    Card,
    Travel
}

public class LevelManager : Singleton<LevelManager> // Handle passenger spawning, Game flow, Board
{
    [Header("Game Flow")]
    private Coroutine gameflowCoroutine;
    private Coroutine timerCoroutine;
    public MovementState currState = MovementState.Station;
    private readonly float _phaseTimer = 10.0f; // 20
    private readonly float _speedTimer = 3.0f;
    public float currTimer { get; private set; }

    [Header("Public Rating")]
    [SerializeField] private float maxPublicRating = 5.0f;
    [SerializeField] private float basePublicRating = 2.5f;
    [SerializeField] private float currPublicRating;

    [Header("Player Score")]
    [SerializeField] public int currentScore = 0;
    [SerializeField] public int baseScoreValue = 100;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currPublicRating -= 2.5f;
            ReducePublicRating();
        }
    }

    public void InitializeLevel()
    {
        // PUBLIC RATING
        currPublicRating = basePublicRating;

        // Score
        currentScore = 0;

        SpawnPassengers.Instance.ResetData();
    }

    public void StartGameFlow()
    {
        gameflowCoroutine = StartCoroutine(DoGameFlow());
    }

    #region GAME FLOW
    private IEnumerator DoGameFlow()
    {
        Debug.Log("Game flow started");

        while (GameManager.Instance.gameState == GameState.GameStart) // NOTE: Replace with GameManager.Instance.gameState != GameManager.Instance.GameState.GameEnded
        {
            #region STATION PHASE
            Debug.Log("Station Phase");

            currTimer = _phaseTimer;
            SetPhase(MovementState.Station, currTimer);

            GameManager.Instance.Board.GetComponent<SpawnTiles>().EnablePlatformTiles();
            //StationManager.Instance.UpdateStationColor();

            yield return new WaitForSeconds(currTimer);
            #endregion

            #region CARD PHASE
            Debug.Log("Card Phase");

            currTimer = _phaseTimer;
            SetPhase(MovementState.Card, currTimer);

            GameManager.Instance.Board.GetComponent<SpawnTiles>().DisablePlatformTiles();

            yield return new WaitForSeconds(currTimer);
            #endregion

            #region LEAVING STATION
            Debug.Log("Accelerating");

            currTimer = _speedTimer;
            SetPhase(MovementState.Accelerate, currTimer);

            yield return new WaitForSeconds(currTimer);
            #endregion

            #region TRAVEL PHASE
            Debug.Log("Travel Phase");

            currTimer = _phaseTimer;
            SetPhase(MovementState.Travel, currTimer);

            yield return new WaitForSeconds(currTimer);
            #endregion

            #region ARRIVING STATION
            Debug.Log("Decelerating");

            currTimer = _speedTimer;
            SetPhase(MovementState.Decelerate, currTimer);

            yield return new WaitForSeconds(currTimer);
            #endregion
        }
    }

    private void SetPhase(MovementState state, float time)
    {
        currState = state;

        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
        }

        timerCoroutine = StartCoroutine(UiManager.Instance.StartPhaseTimer(time));
        UiManager.Instance.SetPhaseText(state);
    }
    #endregion

    #region PUBLIC RATING
    public void AddPublicRating(float value) // Standard = 0.5f, Special = 1.0f
    {
        currPublicRating = Mathf.Clamp(value, 0f, maxPublicRating);
        //Debug.Log("Public Rating Increased! CurrentPublicRating: " + currPublicRating);
    }

    public void ReducePublicRating()
    {
        currPublicRating = Mathf.Clamp(currPublicRating - 0.5f, 0f, maxPublicRating);
        //Debug.Log("Public Rating Decreased. CurrentPublicRating: " + currPublicRating);

        //Angry Standard: -0.5 PR | Angry Priority: -1 PR
        if (currPublicRating <= 0)
        {
            //currPublicRating = 0;
            //UiManager.Instance.ActivateGameoverPanel();
            //Time.timeScale = 0f;
        }
    }
    #endregion

    #region SCORE
    public void AddScore()
    {
        //Happy Standard: 10 points | Happy Priority: 50 points | Landing the station: 100 points (When player lands on a station, they get 100 points, the passengers are just plus points)
        currentScore += 100;

        UiManager.Instance.SetScoreText(currentScore);
        Debug.Log("Current Score: " + currentScore);
        //Add the update text line under the score line
    }
    #endregion
}

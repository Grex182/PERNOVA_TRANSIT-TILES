using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements.Experimental;
using static System.Collections.Specialized.BitVector32;
using static UnityEditor.Rendering.InspectorCurveEditor;
using static UnityEngine.CullingGroup;

#region ENUMS
public enum MovementState
{
    Accelerate,
    Decelerate,
    Station,
    Card,
    Travel
}

public enum CurrentStation
{
    Heart,
    Flower,
    Orange,
    Star,
    Square,
    Diamond,
    Triangle
}

public enum CurrentColor
{
    Red,
    Pink,
    Orange,
    Yellow,
    Green,
    Blue,
    Violet
}

public enum TrainDirection { Right, Left }
#endregion

public class LevelManager : Singleton<LevelManager> // Handle passenger spawning, Game flow, Board
{
    [Header("Game Flow")]
    [SerializeField] public MovementState currState = MovementState.Station;
    [SerializeField] public TrainDirection currDirection = TrainDirection.Right;
    private Coroutine gameflowCoroutine;
    private Coroutine timerCoroutine;

    private readonly float _stationPhaseTimer = 5.0f; // 20
    private readonly float _cardPhaseTimer = 5.0f; // 20
    public readonly float _travelPhaseTimer = 5.0f; // 20
    private readonly float _speedTimer = 5.0f;
    public float currTimer { get; private set; }

    public bool hasAccelerated = false;
    public bool hasDecelerated = false;

    [Header("Station Information")]
    [SerializeField] public CurrentStation currStation = CurrentStation.Heart;
    [SerializeField] public CurrentColor currColor = CurrentColor.Red;
    [SerializeField] public Color currStationColor;
    [SerializeField] public Color targetStationColor;
    [SerializeField] private Color[] stationColors = new Color[]
    {
        new Color(1f, 0f, 0f), // Red
        new Color(1f, 0.41f, 0.71f), // Pink
        new Color(1f, 0.65f, 0f), // Orange
        new Color(1f, 1f, 0f), // Yellow
        new Color(0f, 1f, 0f), // Green
        new Color(0f, 0f, 1f), // Blue
        new Color(0.93f, 0.51f, 0.93f) // Violet
    };

    [Header("Public Rating")]
    [SerializeField] private readonly int maxPublicRating = 10;
    [SerializeField] private readonly int basePublicRating = 5;
    [SerializeField] public int currPublicRating;

    [Header("Player Score")]
    [SerializeField] public int currentScore = 0;
    [SerializeField] public int baseScoreValue = 100;
    private int _happyPassengerCount = 0;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            ReducePublicRating(true); // Angry Standard
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            AddPublicRating(true); // Happy Standard
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ReducePublicRating(false); // Angry Priority
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            AddPublicRating(false); // Happy Priority
        }
    }

    public void InitializeLevel()
    {
        // FLOW
        currState = MovementState.Station;
        currDirection = TrainDirection.Right;

        // STATION 
        currStation = CurrentStation.Heart;
        currColor = CurrentColor.Red;
        UpdateStationColor();

        // PUBLIC RATING
        currPublicRating = basePublicRating;
        UiManager.Instance.SetRating(currPublicRating);

        // SCORE
        currentScore = 0;

        SpawnPassengers.Instance.ResetData();
    }

    #region GAME FLOW
    public void StartGameFlow()
    {
        gameflowCoroutine = StartCoroutine(DoGameFlow());
    }

    private IEnumerator DoGameFlow()
    {
        Debug.Log("Game flow started");

        while (GameManager.Instance.gameState == GameState.GameStart)
        {
            OnStationPhase();
            yield return new WaitForSeconds(currTimer);

            OnCardPhase();
            yield return new WaitForSeconds(currTimer);

            OnAcceleratePhase();
            yield return new WaitUntil(() => hasAccelerated);

            OnTravelPhase();
            yield return new WaitForSeconds(currTimer);

            OnDeceleratePhase();
            yield return new WaitUntil(() => hasDecelerated);
        }
    }

    private void OnStationPhase()
    {
        Debug.Log("Station Phase");

        hasDecelerated = false;
        currTimer = _stationPhaseTimer;
        SetPhase(MovementState.Station, currTimer);
        LevelManager.Instance.AddScore(1);

        GameManager.Instance.Board.GetComponent<SpawnTiles>().EnablePlatformTiles();
        //StationManager.Instance.UpdateStationColor();
    }

    private void OnCardPhase()
    {
        Debug.Log("Card Phase");

        currTimer = _cardPhaseTimer;
        SetPhase(MovementState.Card, currTimer);

        GameManager.Instance.Board.GetComponent<SpawnTiles>().DisablePlatformTiles();
    }

    private void OnAcceleratePhase()
    {
        Debug.Log("Accelerating");

        currTimer = _speedTimer;
        SetPhase(MovementState.Accelerate, currTimer);
    }

    private void OnTravelPhase()
    {
        Debug.Log("Travel Phase");

        hasAccelerated = false;
        currTimer = _travelPhaseTimer;
        SetPhase(MovementState.Travel, currTimer);

        WorldGenerator.Instance.hasStation = false;
        currStation = StationCycler.GetNextStation(currStation, currDirection);
        UpdateStationColor();
        UiManager.Instance.SetTrackerSlider();
    }

    private void OnDeceleratePhase()
    {
        Debug.Log("Decelerating");

        currTimer = _speedTimer;
        SetPhase(MovementState.Decelerate, currTimer);
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

    #region STATION TRACKER
    public static class StationCycler
    {
        private static readonly CurrentStation[] rightOrder = new[]
        {
            CurrentStation.Heart,
            CurrentStation.Flower,
            CurrentStation.Orange,
            CurrentStation.Star,
            CurrentStation.Square,
            CurrentStation.Diamond,
            CurrentStation.Triangle
        };

        private static readonly CurrentStation[] leftOrder = new[]
        {
            CurrentStation.Triangle,
            CurrentStation.Diamond,
            CurrentStation.Square,
            CurrentStation.Star,
            CurrentStation.Orange,
            CurrentStation.Flower,
            CurrentStation.Heart
        };

        public static CurrentStation GetNextStation(CurrentStation current, TrainDirection currDirection)
        {
            var order = currDirection == TrainDirection.Right ? rightOrder : leftOrder;

            int index = System.Array.IndexOf(order, current);
            int nextIndex = (index + 1) % order.Length;

            bool reachEnd = nextIndex == 0;

            if (reachEnd)
            {
                // If we reach the end of the stations, switch direction
                TrainDirection newDirection = currDirection == TrainDirection.Right ? TrainDirection.Left : TrainDirection.Right;
                currDirection = newDirection;
            }

            return order[nextIndex];
        }
    }

    public void UpdateStationColor()
    {
        switch (currStation)
        {
            case CurrentStation.Heart:
                currColor = CurrentColor.Red;
                targetStationColor = stationColors[0]; // Red
                break;

            case CurrentStation.Flower:
                currColor = CurrentColor.Pink;
                targetStationColor = stationColors[1]; // Pink
                break;

            case CurrentStation.Orange:
                currColor = CurrentColor.Orange;
                targetStationColor = stationColors[2]; // Orange
                break;

            case CurrentStation.Star:
                currColor = CurrentColor.Yellow;
                targetStationColor = stationColors[3]; // Yellow
                break;

            case CurrentStation.Square:
                currColor = CurrentColor.Green;
                targetStationColor = stationColors[4]; // Green
                break;

            case CurrentStation.Diamond:
                currColor = CurrentColor.Blue;
                targetStationColor = stationColors[5]; // Blue
                break;

            case CurrentStation.Triangle:
                currColor = CurrentColor.Violet;
                targetStationColor = stationColors[6]; // Violet
                break;

            default:
                currColor = CurrentColor.Red;
                this.targetStationColor = stationColors[0]; // Default to Red
                Debug.LogWarning($"Unknown station: {currStation}");
                break;
        }

        if (UiManager.Instance.colorTransitionCoroutine != null)
        {
            StopCoroutine(UiManager.Instance.colorTransitionCoroutine);
        }

        UiManager.Instance.colorTransitionCoroutine = StartCoroutine(UiManager.Instance.TransitionColor(currStationColor, targetStationColor));

    }
    #endregion

    #region PUBLIC RATING
    public void AddPublicRating(bool isStandard) // Standard = 0.5f, Special = 1.0f
    {
        int value = isStandard ? 1 : 2; // Set value based on rating type

        currPublicRating = Mathf.Clamp(currPublicRating + value, 0, maxPublicRating);
        UiManager.Instance.SetRating(currPublicRating);

        int scoreType = isStandard ? 2 : 3; // 2 for Happy Standard, 3 for Happy Priority
        AddScore(scoreType);
    }

    public void ReducePublicRating(bool isStandard) // Angry Standard: -0.5 PR | Angry Priority: -1 PR
    {
        int value = isStandard ? 1 : 2; // Set value based on rating type

        currPublicRating = Mathf.Clamp(currPublicRating - value, 0, maxPublicRating);
        if (currPublicRating <= 0)
        {
            currPublicRating = 0;
            //UiManager.Instance.ActivateGameoverPanel();
            //Time.timeScale = 0f;
        }
        
        UiManager.Instance.SetRating(currPublicRating);
    }
    #endregion

    #region SCORE
    public void AddScore(int scoreType) // 1: Station Arrival, 2: Happy Standard, 3: Happy Priority
    {
        switch (scoreType)
        {
            case 1: // Station Arrival
                currentScore += 100;
                break;

            case 2: // Happy Standard
                currentScore += 10;
                _happyPassengerCount++;
                break;

            case 3: // Happy Priority
                currentScore += 50;
                _happyPassengerCount++;
                break;

            default:
                Debug.LogWarning("Invalid score type provided.");
                break;
        }

        UiManager.Instance.SetScoreText(currentScore);
    }
    #endregion
}

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
    Station,
    Card,
    Travel,
    Stop
}

public enum CurrentStation
{
    Heart,
    Flower,
    Circle,
    Star,
    Square,
    Diamond,
    Triangle
}

public enum StationColor
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
    [Header("Script References")]
    [SerializeField] private PassengerSpawner passengerSpawner;
    [SerializeField] private BoardManager boardManager;

    [Header("Game Flow")]
    [SerializeField] public MovementState currState = MovementState.Station;
    [SerializeField] public TrainDirection currDirection = TrainDirection.Right;
    private Coroutine gameflowCoroutine;
    private Coroutine timerCoroutine;

    private readonly float _stationPhaseTimer = 10.0f;
    private readonly float _cardPhaseTimer = 5.0f;
    private readonly float _stopPhaseTimer = 1.0f;
    public float _travelPhaseTimer;
    public float currTimer { get; private set; }

    public bool hasTraveled = false;
    public bool isTraveling = false;

    public float decelerationTimer;

    [Header("Station Information")]
    [SerializeField] public CurrentStation currStation = CurrentStation.Heart;
    [SerializeField] public CurrentStation nextStation = CurrentStation.Flower;


    [SerializeField] public StationColor currColor = StationColor.Red;
    [SerializeField] public Color currStationColor;
    [SerializeField] public Color targetStationColor;
    [SerializeField] public Color[] stationColors = new Color[]
    {
        new Color(1f, 0f, 0f), // Red
        new Color(1f, 0.41f, 0.71f), // Pink
        new Color(1f, 0.65f, 0f), // Orange
        new Color(1f, 1f, 0f), // Yellow
        new Color(0f, 1f, 0f), // Green
        new Color(0f, 0f, 1f), // Blue
        new Color(0.93f, 0.51f, 0.93f) // Violet
    };

    [SerializeField] public Material stationMaterial;
    [SerializeField] public Material roofMaterial;

    [Header("Public Rating")]
    [SerializeField] private readonly int maxPublicRating = 10;
    [SerializeField] private readonly int basePublicRating = 5;
    [SerializeField] public int currPublicRating;

    [Header("Player Score")]
    [SerializeField] public int currentScore = 0;
    [SerializeField] public int baseScoreValue = 100;
    private int _happyPassengerCount = 0;

    private void Start()
    {
        InitializeLevel();
    }

    public void InitializeLevel()
    {
        // FLOW
        currState = MovementState.Station;
        currDirection = TrainDirection.Right;
        _travelPhaseTimer = 10.0f;

        // STATION 
        currStation = CurrentStation.Heart;
        currColor = StationColor.Red;
        
        UpdateStationColor();

        // PUBLIC RATING
        currPublicRating = basePublicRating;
        UiManager.Instance.SetRating(currPublicRating);

        // SCORE
        currentScore = 0;

        //SpawnPassengers.Instance.ResetData();

        StartGameFlow();
    }

    #region GAME FLOW
    public void StartGameFlow()
    {
        gameflowCoroutine = StartCoroutine(DoGameFlow());
        passengerSpawner.SpawnPassengerGroup();
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

            OnTravelPhase();
            yield return new WaitUntil(() => hasTraveled);

            OnStopPhase();
            yield return new WaitForSeconds(currTimer);
        }
    }

    private void OnStationPhase()
    {
        Debug.Log("Station Phase");

        currTimer = _stationPhaseTimer;

        SetPhase(MovementState.Station, currTimer);
        AddScore(1);

        
        //Board.Instance.GetComponent<SpawnTiles>().EnablePlatformTiles();
        //StationManager.Instance.UpdateStationColor();
    }

    private void OnCardPhase()
    {
        Debug.Log("Card Phase");
        Debug.Log("Decel Time = " + decelerationTimer);
        currTimer = _cardPhaseTimer;
        SetPhase(MovementState.Card, currTimer);

        //Board.Instance.GetComponent<SpawnTiles>().DisablePlatformTiles();
    }

    private void OnTravelPhase()
    {
        Debug.Log("Travel Phase");

        isTraveling = true;
        
        currTimer = _travelPhaseTimer + (decelerationTimer * 2f);

        Debug.Log("Travel Time = " + currTimer);
        SetPhase(MovementState.Travel, currTimer);

        boardManager.DisableStationsTiles();
        currStation = StationCycler.GetNextStation(currStation, currDirection);
        UpdateStationColor();
        UiManager.Instance.SetTrackerSlider();
    }

    private void OnStopPhase()
    {
        Debug.Log("Stop Phase");
        currTimer = _stopPhaseTimer;
        hasTraveled = false;
        isTraveling = false;
        WorldGenerator.Instance.ActivateStations();
        passengerSpawner.SpawnPassengerGroup();
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
            CurrentStation.Circle,
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
            CurrentStation.Circle,
            CurrentStation.Flower,
            CurrentStation.Heart
        };

        public static CurrentStation GetNextStation(CurrentStation current, TrainDirection currDirection)
        {
            var order = currDirection == TrainDirection.Right ? rightOrder : leftOrder;

            //
            int index = System.Array.IndexOf(order, current);
            int nextIndex = (index + 1) % order.Length;
            //

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
                currColor = StationColor.Red;
                targetStationColor = stationColors[0]; // Red
                break;

            case CurrentStation.Flower:
                currColor = StationColor.Pink;
                targetStationColor = stationColors[1]; // Pink
                break;

            case CurrentStation.Circle:
                currColor = StationColor.Orange;
                targetStationColor = stationColors[2]; // Orange
                break;

            case CurrentStation.Star:
                currColor = StationColor.Yellow;
                targetStationColor = stationColors[3]; // Yellow
                break;

            case CurrentStation.Square:
                currColor = StationColor.Green;
                targetStationColor = stationColors[4]; // Green
                break;

            case CurrentStation.Diamond:
                currColor = StationColor.Blue;
                targetStationColor = stationColors[5]; // Blue
                break;

            case CurrentStation.Triangle:
                currColor = StationColor.Violet;
                targetStationColor = stationColors[6]; // Violet
                break;

            default:
                currColor = StationColor.Red;
                this.targetStationColor = stationColors[0]; // Default to Red
                Debug.LogWarning($"Unknown station: {currStation}");
                break;
        }

        roofMaterial.color = targetStationColor;
        stationMaterial.color = targetStationColor;

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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements.Experimental;
using static System.Collections.Specialized.BitVector32;
using static UnityEngine.CullingGroup;

#region ENUMS
public enum MovementState
{
    Station,
    Card,
    Travel,
    Stop
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

public class LevelManager : MonoBehaviour // Handle passenger spawning, Game flow, Board
{
    public static LevelManager Instance;

    [Header("Script References")]
    [SerializeField] private BoardManager boardManager;
    [SerializeField] private PassengerSpawner passengerSpawner;
    [SerializeField] private StationTiles stationTiles; // Handles station tiles and people

    [Header("Game Flow")]
    public MovementState currState = MovementState.Station;
    public TrainDirection currDirection = TrainDirection.Right;
    private Coroutine gameflowCoroutine;
    private Coroutine timerCoroutine;

    [Header("Timers")]
    private readonly float _stationPhaseTimer = 10.0f;
    private readonly float _cardPhaseTimer = 5.0f;
    private readonly float _stopPhaseTimer = 1.0f;
    public float _travelPhaseTimer = 12.0f;
    public float decelerationTimer;
    public float currTimer { get; private set; }

    [Header("Movement Flags")]
    public bool hasTraveled = false;
    public bool isTraveling = false;

    [Header("Station Information")]
    public StationColor currStation = StationColor.Red;
    public StationColor nextStation = StationColor.Pink;
    public Color currStationColor;
    public Color nextStationColor;

    [Header("Station Colors")]
    public Color[] stationColors = new Color[]
    {
        new Color(1f, 0f, 0f), // Red
        new Color(1f, 0.41f, 0.71f), // Pink
        new Color(1f, 0.65f, 0f), // Orange
        new Color(1f, 1f, 0f), // Yellow
        new Color(0f, 1f, 0f), // Green
        new Color(0f, 0f, 1f), // Blue
        new Color(0.93f, 0.51f, 0.93f) // Violet
    };

    [Header("Station Materials")]
    public Material stationMaterial;
    public Material roofMaterial;

    [Header("Public Rating")]
    private readonly int maxPublicRating = 10;
    private readonly int basePublicRating = 5;
    public int currPublicRating;

    [Header("Player Score")]
    public int currentScore = 0;
    private readonly int baseScoreValue = 1000;
    private int _happyPassengerCount = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
    }

    public void Start()
    {
        // Initialize the level
        InitializeLevel();
    }

    public void InitializeLevel()
    {
        //WORLDGEN
        WorldGenerator.Instance.InitializeWorld();

        // FLOW
        currState = MovementState.Station;
        currDirection = TrainDirection.Right;


        // STATION 
        currStation = StationColor.Red;
        currStationColor = GetColorFromEnum(currStation);
        nextStation = StationColor.Pink;
        stationTiles.Initialize();
        UpdateStationColor();

        //BOARD
        boardManager.Initialize();
        passengerSpawner.SpawnPassengers();

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
        boardManager.BlockStationTiles(false);
        Debug.Log("Station Phase");

        currTimer = _stationPhaseTimer;

        SetPhase(MovementState.Station, currTimer);
        AddScore(baseScoreValue);
    }

    private void OnCardPhase()
    {
        boardManager.BlockStationTiles(true);
        Debug.Log("Card Phase");
        Debug.Log("Decel Time = " + decelerationTimer);
        currTimer = _cardPhaseTimer;
        SetPhase(MovementState.Card, currTimer);
    }

    private void OnTravelPhase()
    {
        Debug.Log("Travel Phase");

        isTraveling = true;
        currTimer = _travelPhaseTimer + (decelerationTimer * 2f);

        Debug.Log("Travel Time = " + currTimer);
        SetPhase(MovementState.Travel, currTimer);

        currStation = StationCycler.GetNextStation(currStation, ref currDirection);
        nextStation = currStation;

        UpdateStationColor();
        UiManager.Instance.SetTrackerSlider();

        AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxClips[0], true);
    }

    private void OnStopPhase()
    {
        Debug.Log("Stop Phase");
        AudioManager.Instance.StopSFX();
        currTimer = _stopPhaseTimer;
        hasTraveled = false;
        isTraveling = false;
        WorldGenerator.Instance.ActivateStations();
        Debug.Log($"Game State: {GameManager.Instance.gameState}");
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
        private static readonly StationColor[] rightOrder = new[]
        {
            StationColor.Red,
            StationColor.Pink,
            StationColor.Orange,
            StationColor.Yellow,
            StationColor.Green,
            StationColor.Blue,
            StationColor.Violet
        };

        private static readonly StationColor[] leftOrder = new[]
        {
            StationColor.Violet,
            StationColor.Blue,
            StationColor.Green,
            StationColor.Yellow,
            StationColor.Orange,
            StationColor.Pink,
            StationColor.Red
        };

        public static StationColor GetNextStation(StationColor current, ref TrainDirection currDirection)
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

                order = currDirection == TrainDirection.Right ? rightOrder : leftOrder;

                //
                index = System.Array.IndexOf(order, current);
                nextIndex = (index + 1) % order.Length;
            }

            Debug.Log($"Direction = {currDirection} Next Station: {order[nextIndex]}");

            return order[nextIndex];
        }
    }

    public void UpdateStationColor()
    {
        switch (currStation)
        {
            case StationColor.Red:
                nextStationColor = stationColors[0]; // Red
                break;

            case StationColor.Pink:
                nextStationColor = stationColors[1]; // Pink
                break;

            case StationColor.Orange:
                nextStationColor = stationColors[2]; // Orange
                break;

            case StationColor.Yellow:
                nextStationColor = stationColors[3]; // Yellow
                break;

            case StationColor.Green:
                nextStationColor = stationColors[4]; // Green
                break;

            case StationColor.Blue:
                nextStationColor = stationColors[5]; // Blue
                break;

            case StationColor.Violet:
                nextStationColor = stationColors[6]; // Violet
                break;

            default:
                this.nextStationColor = stationColors[0]; // Default to Red
                Debug.LogWarning($"Unknown station: {currStation}");
                break;
        }

        roofMaterial.color = nextStationColor;
        stationMaterial.color = nextStationColor;

        if (UiManager.Instance.colorTransitionCoroutine != null)
        {
            StopCoroutine(UiManager.Instance.colorTransitionCoroutine);
        }

        UiManager.Instance.colorTransitionCoroutine = StartCoroutine(UiManager.Instance.TransitionColor(currStationColor, nextStationColor));
    }

    public Color GetColorFromEnum(StationColor _enumColor)
    {
        Color _color = Color.white;
        switch (_enumColor)
        {
            case StationColor.Red:
                _color = stationColors[0]; // Red
                break;

            case StationColor.Pink:
                _color = stationColors[1]; // Pink
                break;

            case StationColor.Orange:
                _color = stationColors[2]; // Orange
                break;

            case StationColor.Yellow:
                _color = stationColors[3]; // Yellow
                break;

            case StationColor.Green:
                _color = stationColors[4]; // Green
                break;

            case StationColor.Blue:
                _color = stationColors[5]; // Blue
                break;

            case StationColor.Violet:
                _color = stationColors[6]; // Violet
                break;

            default:
                _color = stationColors[0]; // Default to Red
                Debug.LogWarning($"Unknown station: {_enumColor}");
                break;
        }
        return _color;
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
    public void AddScore(int scoreType)
    {
        currentScore += scoreType;
        UiManager.Instance.SetScoreText(currentScore);
    }
    #endregion
}

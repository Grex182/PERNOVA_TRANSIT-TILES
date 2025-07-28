using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements.Experimental;
using static System.Collections.Specialized.BitVector32;
using static UnityEngine.CullingGroup;
using static UnityEngine.Rendering.DebugUI;

#region ENUMS
public enum MovementState
{
    Shop,
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
    [SerializeField] private LightingManager lightingManager;
    [SerializeField] private DifficultyManager difficultyManager;

    [Header("Game Flow")]
    public MovementState currState = MovementState.Station;
    public TrainDirection currDirection = TrainDirection.Right;
    private Coroutine gameflowCoroutine;
    private Coroutine timerCoroutine;
    public bool isEndStation = false;

    [Header("Timers")]
    public float _stationPhaseTimer = 15.0f;
    private readonly float _cardPhaseTimer = 5.0f;
    private readonly float _stopPhaseTimer = 1.0f;
    public float travelPhaseTimer = 12.0f;
    public float decelerationTimer;
    [SerializeField] private float currTimer;

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
    public int earnedStars = 0;

    public float passengerSpawnedCount = 0; // Total passengers spawned in the station x
    public float passengerToDisembarkCount = 0; // Total passengers that disembarked in the station

    public float correctDisembarkCount = 0; // Total passengers that disembarked in the right station
    public float passengersLeftInStation = 0; // Total passengers left in the station (after disembarking)
    public bool hasDisembarkedWrong = false; // If any passenger disembarked in the wrong station

    [Header("Player Score")]
    public int currentScore = 0;
    private readonly int baseScoreValue = 1000;

    [Header("Card Effects")]
    // Filipino Time
    public bool hasFilipinoTimeEffect = false;
    private readonly float stationDelayTime = 5.0f;
    // Excuse Me Po
    public bool hasExcuseMePo = false;

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
        isEndStation = false;

        // STATION 
        currStation = StationColor.Red;
        currStationColor = GetColorFromEnum(currStation);
        stationTiles.Initialize();
        UpdateStationColor();

        //BOARD
        boardManager.Initialize();
        passengerSpawner.SpawnPassengers();

        // PUBLIC RATING
        currPublicRating = basePublicRating;
        UiManager.Instance.SetRating(currPublicRating);
        earnedStars = 0;

        // SCORE
        currentScore = 0;

        //SpawnPassengers.Instance.ResetData();

        GetPublicRatingValues();
        HandManager.Instance.DrawStartingHand();
        StartGameFlow();
    }

    #region GAME FLOW
    public void StartGameFlow()
    {
        gameflowCoroutine = StartCoroutine(DoGameFlow());
    }

    public void StopGameFlow()
    {
        if (gameflowCoroutine != null)
        {
            StopCoroutine(gameflowCoroutine);
        }
        gameflowCoroutine = null;
    }

    private IEnumerator DoGameFlow()
    {
        Debug.Log("Game flow started");

        while (GameManager.Instance.gameState == GameState.GameStart)
        {
            /* ------ STATION PHASE ------ */
            AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxClips[6], false); // Doors opening alarm
            AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxClips[7], false); // Doors opening
            AudioManager.Instance.DoAnnouncementCoroutine(MovementState.Station, currStation);

            OnStationPhase();
            yield return new WaitForSeconds(currTimer);

            /* ------- SHOP PHASE ------- */
            if (isEndStation)
            {
                OnShopPhase();
                yield return new WaitUntil(() => !isEndStation);
            }

            /* ------- CARD PHASE ------- */
            AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxClips[6], false); // Doors closing alarm
            AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxClips[8], false); // Doors closing

            OnCardPhase();
            yield return new WaitForSeconds(currTimer);

            /* ----- TRAVEL PHASE ----- */
            OnTravelPhase();
            yield return new WaitUntil(() => hasTraveled);
             
            /* ------ STOP PHASE ------ */
            OnStopPhase();
            yield return new WaitForSeconds(currTimer);
        }
    }

    private void OnShopPhase()
    {
        earnedStars += Mathf.FloorToInt(currPublicRating / 2);
        UiManager.Instance.SetCardShopState(true);
        ShopManager.Instance.TogglePanel();
    }

    private void OnStationPhase()
    {
        if (currStation == StationColor.Red && currDirection == TrainDirection.Left)
        {
            isEndStation = true;
        }

        boardManager.BlockStationTiles(false);
        Debug.Log("Station Phase");

        currTimer = hasFilipinoTimeEffect ? _stationPhaseTimer + stationDelayTime : _stationPhaseTimer;
        Debug.Log("Filipino Time Effect: " + hasFilipinoTimeEffect + "|| Station Phase Time = " + currTimer);

        SetPhase(MovementState.Station, currTimer);
        AddScore(baseScoreValue);
    }

    private void OnCardPhase()
    {
        if (hasExcuseMePo)
        {
            hasExcuseMePo = false;
        }
        
        passengerSpawner.ClearTrainDoors();
        boardManager.BlockStationTiles(true);
        boardManager.SpawnTrash();
        Debug.Log("Card Phase");
        Debug.Log("Decel Time = " + decelerationTimer);
        currTimer = _cardPhaseTimer;
        hasFilipinoTimeEffect = false;
        SetPhase(MovementState.Card, currTimer);
        SetPublicRating();
    }

    private void OnTravelPhase()
    {
        Debug.Log("Travel Phase");

        difficultyManager.UpdateDifficulty();

        isTraveling = true;
        currTimer = travelPhaseTimer + (decelerationTimer * 2f);

        Debug.Log("Travel Time = " + currTimer);
        SetPhase(MovementState.Travel, currTimer);

        currStation = StationCycler.GetNextStation(currStation, ref currDirection);

        UpdateStationColor();
        UiManager.Instance.SetTrackerSlider();
        UiManager.Instance.SetStationLED(currStation, true);

        AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxClips[0], true);
        AudioManager.Instance.DoAnnouncementCoroutine(MovementState.Travel, currStation);
    }

    private void OnStopPhase()
    {
        Debug.Log("Stop Phase");
        AudioManager.Instance.StopSFX();
        UiManager.Instance.SetStationLED(currStation, false);
        passengerSpawner.ResetPassengerMood();
        currTimer = _stopPhaseTimer;
        hasTraveled = false;
        isTraveling = false;
        WorldGenerator.Instance.ActivateStations();
        Debug.Log($"Game State: {GameManager.Instance.gameState}");

        GetPublicRatingValues();
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

                LevelManager.Instance.isEndStation = true;
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
    public void DoSukiStar(int value)
    {
        currPublicRating = Mathf.Clamp(currPublicRating + value, 0, maxPublicRating);
        UiManager.Instance.SetRating(currPublicRating);
    }

    private void GetPublicRatingValues()
    {
        passengerSpawnedCount = passengerSpawner.stationPassengersParent.transform.childCount;
        if (passengerSpawnedCount <= 0) 
        { 
            passengerSpawnedCount = 1; 
        }

        passengerSpawner.GetTotalDisembarkCount();
        if (passengerToDisembarkCount <= 0)
        {
            passengerToDisembarkCount = 1;
            correctDisembarkCount = 1;
        }
    }

    private void SetPublicRating()
    {
        foreach (Transform child in passengerSpawner.stationPassengersParent.transform)
        {
            passengersLeftInStation++;
        }

        float stationRatio = 1 - (passengersLeftInStation / passengerSpawnedCount);
        float disembarkRatio = correctDisembarkCount / passengerToDisembarkCount;

        float avg = (stationRatio + disembarkRatio) / 2;


        if (avg > 0.85)
        {
            currPublicRating = Mathf.Clamp(currPublicRating + 2, 0, maxPublicRating);
        }
        else if (avg > 0.65)
        {
            currPublicRating = Mathf.Clamp(currPublicRating + 1, 0, maxPublicRating);
        }
        else if (avg > 0.3)
        {
            currPublicRating = Mathf.Clamp(currPublicRating - 2, 0, maxPublicRating);
        }
        else
        {
            currPublicRating = Mathf.Clamp(currPublicRating - 3, 0, maxPublicRating);
        }

        if (hasDisembarkedWrong)
        {
            currPublicRating = Mathf.Clamp(currPublicRating - 1, 0, maxPublicRating);
            hasDisembarkedWrong = false;
        }


        passengerSpawnedCount = 0;
        passengerToDisembarkCount = 0;

        correctDisembarkCount = 0;
        passengersLeftInStation = 0;

        if (currPublicRating <= 0)
        {
            currPublicRating = 0;
            UiManager.Instance.ActivateGameoverPanel();
        }
        boardManager.SetSpawnableTiles(currPublicRating);
        UiManager.Instance.SetRating(currPublicRating);
    }
    #endregion

    #region SCORE
    public void AddScore(int scoreType)
    {
        currentScore += scoreType;
        UiManager.Instance.CreateScoreFloatie(scoreType);
        UiManager.Instance.SetScoreText(currentScore);
    }
    #endregion

    public void ClearTrash()
    {
        boardManager.ClearTrash();
    }
}

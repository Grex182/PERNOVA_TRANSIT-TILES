using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

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
    // Excuse Me Po
    public bool hasExcuseMePo = false;

    [Header("Tutorial")]
    [SerializeField] private GameObject _standardPassengerPrefab;
    [SerializeField] private GameObject _tutorialPanel;
    [SerializeField] private TextMeshProUGUI _tutorialText;
    public bool isPressed = false;

    [SerializeField] private string[] _tutorialTexts;
    private int _currentTutorialIndex = 0;
    public bool canAnimateTrain = false;

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

        // PUBLIC RATING
        currPublicRating = basePublicRating;
        UiManager.Instance.SetRating(currPublicRating);
        earnedStars = 0;

        // SCORE
        currentScore = 0;

        GetPublicRatingValues();

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
        passengerSpawner.resetPassengerMood();
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
        passengerSpawnedCount = passengerSpawner.stationParent.transform.childCount;
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
        foreach (Transform child in passengerSpawner.stationParent.transform)
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
    public void OnNextTutorialClicked()
    {
        _currentTutorialIndex++;

        if (_currentTutorialIndex >= _tutorialTexts.Length)
        {
            EndTutorial();
            return;
        }

        // Update the tutorial text
        _tutorialText.text = _tutorialTexts[_currentTutorialIndex];

        // You can add specific actions for certain tutorial steps here
        HandleTutorialStepActions(_currentTutorialIndex);
    }

    private void HandleTutorialStepActions(int stepIndex)
    {
        // Add any specific actions that should happen at certain tutorial steps
        switch (stepIndex)
        {
            case 2: // Passenger Movement
                GameObject spawnTile = boardManager.grid[10, 3];
                passengerSpawner.SpawnSinglePassenger(spawnTile, _standardPassengerPrefab);
                break;
        }
    }

    private void EndTutorial()
    {
        _tutorialPanel.SetActive(false);
        //_nextButton.SetActive(false);
        // Any other cleanup or game start logic
    }

    private void SetTutorialTexts()
    {
        _tutorialTexts = new string[]
        {
            // Introduction
            "Hey there rookie! Welcome to your first day of managing Metro Linear Transit, or MLT for short.",
            "Your job is to ensure every passenger reaches their destination happily by keeping them comfortable throughout the journey.",
            
            // Passenger Movement
            "Look! A passenger has arrived at the station. To select them, use your left mouse button. Then, drag your mouse to guide them to the train.",
            "Once they're inside: \n " +
            "- Toggle Selection Mode: Left-click again to drop them off. \n " +
            "- Hold Selection Mode: Simply release the left mouse button.",
            
            // Phase Timer and Station Phase
            "This is the phase timer, this shows you how much time you have left on each phase. Right now we are at the “Arrived at Station” Phase, this is where we board and disembark passengers.",
            "Board all the passengers in the train!",

            // Card Phase
            "Great! Now we’re entering the ‘Doors Are Closing’ phase. During this time, you can’t move passengers—but you can play cards! ",
            "Hover your mouse over the card to inspect it. Each card has unique effects and rarities. You’ll learn more about them soon, but for now lets activate this card by dragging it into the “Play Card Zone”",
            "Remember, you can check your cards anytime, but you can only play them during this phase!\r\n",
            
            // Travel Phase
            "Next is the ‘Approaching Next Station’ phase. The train starts moving again, and you can now rearrange passengers while we’re traveling.",

            // Station Tracker
            "This is the station tracker. This shows the train's location! We are leaving Red Heart Station, and our next stop is Pink Flower Station.",
            "See this passenger wearing pink? That means they need to get off at the next station! Let’s make sure they’re ready.",
            "We arrived at the pink flower station! Drag the pink passenger outside to let them disembark.",

            // Passenger Types
            "Oh my… These passengers on the station look quite unique.",
            "Some passengers carry bulky items with them, make sure you make enough space for them!\r\nYou can click ‘R’ or right-click to rotate passengers.\r\n",
            "Some passengers negatively affect their surroundings. Be careful who you place around them!",
            "Some passengers take priority to sit down. Make space when you can to ensure they stay happy.",
            "Some passengers didn’t get enough sleep last night. They doze off when left alone for a while.\r\nClick on them to wake them up, They take some time to wake up before you can move them. When they’re awake click on them again to move them.",
            
            // Display Board and Rush Hour
            "These passengers get extra tricky during rush hour! This Display Board flashes announcements—like when rush hour starts! It also shows the current time and day of the week.",
            
            // LED Board
            "Passengers will rate their experience in the Metro Linear Transit, you can track your current overall performance in this LED Board. Be careful! If your ratings reach 0, it's Game over. So let’s keep our passengers happy.",
            
            // End Station
            "I got something to show you on violet triangle station. So let’s go!",
            "When you reach either of the end stations, you’ll earn 1 Star Point for every Public Star Rating you’ve maintained. Now, let’s put those points to good use!",
            "Welcome to Claire’s Commuter Hacks Shop, Your go-to spot for powerful commuter hack cards! I have a branch on both Violet Triangle and Red Heart Station.",
            
            // End
            "That was a lot of information… Now let’s see how well you listened, rookie! Goodluck!"
        };

    }
}

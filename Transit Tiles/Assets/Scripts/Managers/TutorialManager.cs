using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    [Header("Script References")]
    [SerializeField] private BoardManager boardManager;
    [SerializeField] private PassengerSpawner passengerSpawner;
    [SerializeField] private StationTiles stationTiles; // Handles station tiles and people
    [SerializeField] private LightingManager lightingManager;

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
    [SerializeField] private GameObject _tutorialObject;
    [SerializeField] private RectTransform _tutorialPanel;
    [SerializeField] private TextMeshProUGUI _tutorialText;
    public bool isPressed = false;

    [SerializeField] private string[] _tutorialTexts;
    public int _currentTutorialIndex = -1;
    public bool canAnimateTrain = false;
    [SerializeField] private GameObject _nextButton;
    [SerializeField] private GameObject _highlightBox;
    private float _manualTimer = 12f;
    [SerializeField] private GameObject billboard;

    private GameObject spawnedPassenger;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        // Initialize the level
        InitializeLevel();
    }

    private void Update()
    {
        if (spawnedPassenger != null)
        {
            
            if (spawnedPassenger.transform.position.y > 0.5f &&
                    _currentTutorialIndex == 2)
            {
                OnNextTutorialClicked();
            }

            if (spawnedPassenger.GetComponent<PassengerData>().currTile != TileTypes.Station &&
                spawnedPassenger.transform.position.y < 0.5f &&
                _currentTutorialIndex == 3)
            {
                OnNextTutorialClicked();
                _nextButton.SetActive(true);
                SetPhase(MovementState.Station, currTimer);

            }

        }
        else if (spawnedPassenger == null
            && _currentTutorialIndex == 12)
        {
            OnNextTutorialClicked();
        }

        if (_currentTutorialIndex == 5)
        {
            _manualTimer -= Time.deltaTime;
            if (_manualTimer <= 0)
            {
                OnNextTutorialClicked();
            }
        }

        if (_currentTutorialIndex == 11)
        {
            if (currState == MovementState.Stop)
            {
                _nextButton.SetActive(true);
            }
        }

        if (_currentTutorialIndex == 19)
        {
            if (billboard.transform.localPosition.y < -200f)
            {
                _nextButton.SetActive(true);
                _highlightBox.SetActive(false);
            }
        }
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
        stationTiles.gameObject.transform.SetParent(WorldGenerator.Instance.stationsParent.transform.GetChild(1),false);
        UpdateStationColor();

        //BOARD
        boardManager.Initialize();

        // PUBLIC RATING
        currPublicRating = basePublicRating;
        TutorialUiManager.Instance.SetRating(currPublicRating);
        earnedStars = 0;

        // SCORE
        currentScore = 0;

        // TUTORIAL
        _currentTutorialIndex = -1;
        SetTutorialTexts();
        OnNextTutorialClicked();

        GetPublicRatingValues();
        _highlightBox.SetActive(false);
    }

    #region GAME FLOW
    public void StartTravelPhase()
    {
        gameflowCoroutine = StartCoroutine(DoTravelPhase());
    }

    public void StopGameFlow()
    {
        if (gameflowCoroutine != null)
        {
            StopCoroutine(gameflowCoroutine);
        }
        gameflowCoroutine = null;
    }

    private IEnumerator DoTravelPhase()
    {
        /* ----- TRAVEL PHASE ----- */
        Debug.Log("Travel Phase");

        isTraveling = true;
        currTimer = travelPhaseTimer + (decelerationTimer * 2f);

        Debug.Log("Travel Time = " + currTimer);
        SetPhase(MovementState.Travel, currTimer);

        currStation = StationCycler.GetNextStation(currStation, ref currDirection);

        UpdateStationColor();
        TutorialUiManager.Instance.SetTrackerSlider();
        TutorialUiManager.Instance.SetStationLED(currStation, true);

        AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxClips[0], true);
        AudioManager.Instance.DoAnnouncementCoroutine(MovementState.Travel, currStation);

        yield return new WaitForSeconds(currTimer);

        /* ------ STOP PHASE ------ */
        Debug.Log("Stop Phase");
        AudioManager.Instance.StopSFX();
        TutorialUiManager.Instance.SetStationLED(currStation, false);
        passengerSpawner.resetPassengerMood();
        currTimer = _stopPhaseTimer;
        hasTraveled = false;
        isTraveling = false;
        if (_currentTutorialIndex != 21)
        {
            WorldGenerator.Instance.ActivateStations();
        }
        Debug.Log($"Game State: {GameManager.Instance.gameState}");
        SetPhase(MovementState.Stop, currTimer);
        GetPublicRatingValues();

    }

    private void OnShopPhase()
    {
        earnedStars += Mathf.FloorToInt(currPublicRating / 2);
        TutorialUiManager.Instance.SetCardShopState(true);
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
        Debug.Log("Card Phase");
        Debug.Log("Decel Time = " + decelerationTimer);
        currTimer = _cardPhaseTimer;
        SetPhase(MovementState.Card, currTimer);
        SetPublicRating();
    }

    private void SetPhase(MovementState state, float time)
    {
        currState = state;

        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
        }

        timerCoroutine = StartCoroutine(TutorialUiManager.Instance.StartPhaseTimer(time));
        TutorialUiManager.Instance.SetPhaseText(state);
    }
    public void SetTravel(bool hasTravelled)
    {
        hasTraveled = hasTravelled;
    }

    public void SetDeceleration(float deceleration)
    {
        decelerationTimer = deceleration;
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

                TutorialManager.Instance.isEndStation = true;
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

        if (TutorialUiManager.Instance.colorTransitionCoroutine != null)
        {
            StopCoroutine(TutorialUiManager.Instance.colorTransitionCoroutine);
        }

        TutorialUiManager.Instance.colorTransitionCoroutine = StartCoroutine(TutorialUiManager.Instance.TransitionColor(currStationColor, nextStationColor));
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

        boardManager.SetSpawnableTiles(currPublicRating);
        TutorialUiManager.Instance.SetRating(currPublicRating);
    }
    #endregion

    #region SCORE
    public void AddScore(int scoreType)
    {
        currentScore += scoreType;
        TutorialUiManager.Instance.CreateScoreFloatie(scoreType);
        TutorialUiManager.Instance.SetScoreText(currentScore);
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
            case 0:
                _nextButton.SetActive(true);
                SetPhase(MovementState.Station, _stationPhaseTimer);
                break;

            case 2: // Passenger Movement
                _nextButton.SetActive(false);
                GameObject spawnTile = boardManager.grid[10, 3];

                PassengerData data = _standardPassengerPrefab.GetComponent<PassengerData>();
                data.targetStation = StationColor.Pink;
                data.currTile = TileTypes.Station;

                spawnedPassenger = Instantiate(_standardPassengerPrefab, spawnTile.transform.position,
                                                   Quaternion.identity, passengerSpawner.stationPassengersParent.transform);
                spawnedPassenger.transform.localScale = Vector3.one * 0.01f;
                break;

            case 4:
                _highlightBox.SetActive(true);
                RectTransform highlightRect = _highlightBox.GetComponent<RectTransform>();
                StartCoroutine(AnimateHighlightBox(highlightRect, new Vector2(0, 440), new Vector2(525, 115)));
                //Set HighlightBox Size and Position
                break;
            case 5:
                _highlightBox.SetActive(false);
                _nextButton.SetActive(false);
                passengerSpawner.SpawnPassengersStandard(true);
                SetPhase(MovementState.Station, _stationPhaseTimer);
                _manualTimer = _stationPhaseTimer;

                
                break;
            case 6:
                OnCardPhase();
                _nextButton.SetActive(true);
                HandManager.Instance.DrawStartingHand();
                _tutorialObject.transform.localPosition += Vector3.up * 100f;
                break;

            case 7:
                TutorialUiManager.Instance._dropZoneObj.SetActive(true);
                _nextButton.SetActive(false);
                
                
                break;
            case 8:
                _nextButton.SetActive(true);
                _tutorialObject.transform.localPosition -= Vector3.up * 100f;
                break;

            case 9:
                TutorialUiManager.Instance.StartCoroutine(TutorialUiManager.Instance.DeactivateDropZone());
                StartTravelPhase();
                break;
            case 10:
                _highlightBox.SetActive(true);
                RectTransform highlightRect2 = _highlightBox.GetComponent<RectTransform>();
                highlightRect2.anchoredPosition = new Vector2(0, 513f);
                highlightRect2.sizeDelta = new Vector2(2000, 1000);
                StartCoroutine(AnimateHighlightBox(highlightRect2, new Vector2(0, 513), new Vector2(684, 55)));
                break;
            case 11:
                _highlightBox.SetActive(false);
                _nextButton.SetActive(false);
                break;
            case 12:
                _nextButton.SetActive(false);
                OnStationPhase();
                break;
            case 13:
                SetPhase(MovementState.Card, 0f);
                passengerSpawner.DeletePassengers();
                boardManager.VacateStationTiles(true);
                passengerSpawner.SpawnPassengersSpecials();
                _nextButton.SetActive(true);
                break;
            case 14:
                //Bulky
                _highlightBox.SetActive(true);
                RectTransform highlightRect3 = _highlightBox.GetComponent<RectTransform>();
                highlightRect3.anchoredPosition = new Vector2(-70, 160);
                highlightRect3.sizeDelta = new Vector2(1800, 2000);
                StartCoroutine(AnimateHighlightBox(highlightRect3, new Vector2(-70, 160), new Vector2(180, 200)));
                break;
            case 15:
                //Negative
                RectTransform highlightRect4 = _highlightBox.GetComponent<RectTransform>();
                highlightRect4.anchoredPosition = new Vector2(70, 160);
                highlightRect4.sizeDelta = new Vector2(1800, 2000);
                StartCoroutine(AnimateHighlightBox(highlightRect4, new Vector2(70, 160), new Vector2(180, 200)));
                break;
            case 16:
                //Priority
                RectTransform highlightRect5 = _highlightBox.GetComponent<RectTransform>();
                highlightRect5.anchoredPosition = new Vector2(-35, 220);
                highlightRect5.sizeDelta = new Vector2(1000, 3000);
                StartCoroutine(AnimateHighlightBox(highlightRect5, new Vector2(-35, 220), new Vector2(100, 300)));
                break;
            case 17:
                //Sleepy
                RectTransform highlightRect6 = _highlightBox.GetComponent<RectTransform>();
                highlightRect6.anchoredPosition = new Vector2(35, 310);
                highlightRect6.sizeDelta = new Vector2(1000, 1800);
                StartCoroutine(AnimateHighlightBox(highlightRect6, new Vector2(35, 310), new Vector2(100, 180)));
                break;
            case 19:
                _nextButton.SetActive(false);
                RectTransform highlightRect7 = _highlightBox.GetComponent<RectTransform>();
                highlightRect7.anchoredPosition = new Vector2(-630, 450);
                highlightRect7.sizeDelta = new Vector2(6500, 1700);
                StartCoroutine(AnimateHighlightBox(highlightRect7, new Vector2(-630, 450), new Vector2(650, 170)));
                break;
            case 20:
                _tutorialPanel.sizeDelta = new Vector2(729, 255);
                _highlightBox.SetActive(true);
                RectTransform highlightRect8 = _highlightBox.GetComponent<RectTransform>();
                highlightRect8.anchoredPosition = new Vector2(-700, -440);
                highlightRect8.sizeDelta = new Vector2(5000, 1900);
                StartCoroutine(AnimateHighlightBox(highlightRect8, new Vector2(-700, -440), new Vector2(500, 190)));
                break;
            case 21:
                currStation = StationColor.Blue;
                nextStation = StationColor.Violet;
                StartTravelPhase();
                break;
        }
    }

    private IEnumerator AnimateHighlightBox(RectTransform rect, Vector2 pos, Vector2 targetSize)
    {
        Vector2 initialSize = new Vector2(targetSize.x + 80f, targetSize.y + 80f);
        rect.sizeDelta = initialSize;

        Color currColor = _highlightBox.GetComponent<Image>().color;
        _highlightBox.GetComponent<Image>().color = new Color(currColor.r, currColor.g, currColor.b, 0f);
        
        rect.anchoredPosition = pos;

        float elapsedTime = 0f;
        float duration = 0.5f;
        while (elapsedTime < duration)
        {
            float progress = elapsedTime / duration;
            // Interpolate size and position
            rect.sizeDelta = Vector2.Lerp(
                initialSize,
                targetSize,
                progress
            );

            Color currentColor = _highlightBox.GetComponent<Image>().color;
            currentColor.a = Mathf.Lerp(0f, 1f, progress);
            _highlightBox.GetComponent<Image>().color = currentColor;

            elapsedTime += Time.deltaTime;
            yield return null; // Wait for next frame
        }

        // Ensure final values are exact
        rect.sizeDelta = targetSize;
    }

    private void EndTutorial()
    {
        _tutorialObject.SetActive(false);
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
            "Remember, you can check your cards anytime, but you can only play them during this phase!",
            
            // Travel Phase
            "Next is the ‘Approaching Next Station’ phase. The train starts moving again, and you can now rearrange passengers while we’re traveling.",

            // Station Tracker
            "This is the station tracker. This shows the train's location! We are leaving Red Heart Station, and our next stop is Pink Flower Station.",
            "See this passenger wearing pink? That means they need to get off at the next station! Let’s make sure they’re ready.",
            "We arrived at the pink flower station! Drag the pink passenger outside to let them disembark.",

            // Passenger Types
            "Oh my… These passengers on the station look quite unique.",
            "Some passengers carry bulky items with them, make sure you make enough space for them!\r\nYou can click ‘R’ or right-click to rotate passengers.",
            "Some passengers negatively affect their surroundings. Be careful who you place around them!",
            "Some passengers take priority to sit down. Make space when you can to ensure they stay happy.",
            "Some passengers didn’t get enough sleep last night. They doze off when left alone for a while.",
            "Click on them to wake them up, They take some time to wake up before you can move them. When they’re awake click on them again to move them.",
            
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

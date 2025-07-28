using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Burst.Intrinsics;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class UiManager : MonoBehaviour
{
    public static UiManager Instance;

    [Header("Pause")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject confirmationPanel;
    public bool isPaused = false;
    private bool _isQuitting = false;
    private bool _isSettings = false;

    [Header("Settings")]
    [SerializeField] private GameObject _settingsObj;
    [SerializeField] private Slider _selectionSlider;
    [SerializeField] private Slider _colorblindSlider;
    [SerializeField] private Slider _bgmVolumeSlider;
    [SerializeField] private Slider _sfxVolumeSlider;

    [Header("Public Rating")]
    [SerializeField] private List<GameObject> stars = new List<GameObject>();

    [Header("Phase Timer")]
    [SerializeField] private TextMeshProUGUI currPhaseText;
    [SerializeField] private GameObject[] timerSegments = new GameObject[16];
    [SerializeField] private GameObject[] StationLED = new GameObject[7];

    [Header("Score")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TMP_Text scoreGameOverText;
    [SerializeField] GameObject scoreFloatieParent;
    [SerializeField] GameObject floatiePrefab;

    [Header("Station Tracker")]
    private Coroutine sliderCoroutine;
    [SerializeField] private GameObject rightTracker;
    [SerializeField] private GameObject leftTracker;
    [SerializeField] private Slider rightTrackerSlider;
    [SerializeField] private Slider leftTrackerSlider;
    public Coroutine colorTransitionCoroutine;

    [Header("Card Shop")]
    [SerializeField] private GameObject _shopCanvas;

    [Header("Game Over")]
    [SerializeField] private GameObject gameOverCanvas;
    private Animator anim;

    [Header("Card")]
    [SerializeField] private GameObject _dropZoneObj;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;

        anim = gameOverCanvas.GetComponentInChildren<Animator>();
        gameOverCanvas.SetActive(false);
    }

    private void Start()
    {
        InitializeUi();
    }

    public void InitializeUi()
    {
        _bgmVolumeSlider.value = AudioManager.Instance.musicVolume;
        _sfxVolumeSlider.value = AudioManager.Instance.sfxVolume;

        _colorblindSlider.onValueChanged.AddListener(OnColorblindSliderValueChanged);
        _selectionSlider.onValueChanged.AddListener(OnSelectionSliderValueChanged);

        _bgmVolumeSlider.onValueChanged.AddListener((value) => {
            AudioManager.Instance.ChangeBgmVolume(value);
        });

        _sfxVolumeSlider.onValueChanged.AddListener((value) => {
            AudioManager.Instance.ChangeSfxVolume(value);
        });

        _settingsObj.SetActive(false);
        _dropZoneObj.SetActive(false);
        _shopCanvas.SetActive(false);

        isPaused = false;
        _isQuitting = false;
        _isSettings = false;

        foreach (var segment in timerSegments)
        {
            segment.SetActive(true);
        }

        // Set Slider Values
        rightTrackerSlider.value = 2f;
        leftTrackerSlider.value = 2f;
        SetStationLED(StationColor.Red,false);

        // Set Slider Direction
        rightTracker.SetActive(true);
        leftTracker.SetActive(false);

        pausePanel.SetActive(false);
        confirmationPanel.SetActive(false);
    }

    private void Update()
    {
        if (LevelManager.Instance.currState == MovementState.Card && !isPaused)
        {
            _dropZoneObj.SetActive(true);
            _dropZoneObj.GetComponent<DropZone>().isActivated = true;
        }
        else
        {
            StartCoroutine(DeactivateDropZone());
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (pausePanel.activeSelf)
            {
                OnResumeButtonClicked();
            }
            else
            {
                OnPauseButtonClicked();
            }
        }
    }

    #region SETTINGS
    private void OnSelectionSliderValueChanged(float value)
    {
        GameManager.Instance.SetSelectionMode(value);
    }

    private void OnColorblindSliderValueChanged(float value)
    {
        GameManager.Instance.SetColorblindMode(value);
    }
    #endregion

    #region PUBLIC RATING
    public void SetRating(float rating)
    {
        for (int i = 0; i < stars.Count; i++)
        {
            if (i < rating)
            {
                stars[i].SetActive(true);
            }
            else
            {
                stars[i].SetActive(false);
            }
        }
    }
    #endregion

    #region PHASE TIMER
    public IEnumerator StartPhaseTimer(float time)
    {
        float segmentDuration = time / timerSegments.Length;

        foreach (var segment in timerSegments)
        {
            segment.SetActive(true);
        }

        foreach (var segment in timerSegments)
        {
            yield return new WaitForSeconds(segmentDuration);
            time -= segmentDuration;
            segment.SetActive(false);
        } 

    }

    public void SetPhaseText(MovementState phase)
    {
        switch (phase)
        {
            case MovementState.Station:
                currPhaseText.text = "Arrived at Station"; // Station Phase
                break;
            case MovementState.Card:
                currPhaseText.text = "Doors are Closing"; // Card Phase
                break;
            case MovementState.Travel:
                currPhaseText.text = "Approaching next Station"; // Travel Phase
                break;
        }
    }
    #endregion

    #region SCORE
    public void SetScoreText(int score)
    {
        scoreText.text = score.ToString();
    }

    public void CreateScoreFloatie(int score)
    {
        GameObject floatie = Instantiate(floatiePrefab,scoreFloatieParent.transform.position,scoreFloatieParent.transform.rotation,scoreFloatieParent.transform);
        ScoreFloatieScript floatScript = floatie.GetComponent<ScoreFloatieScript>();
        floatScript.score = score;
    }

    public void SetGameOverScoreText(int score)
    {
        scoreGameOverText.text = score.ToString();
    }
    #endregion

    #region STATION TRACKER
    public void SetTrackerSlider()
    {
        if (LevelManager.Instance.currDirection == TrainDirection.Right)
        {
            rightTracker.SetActive(true);
            leftTracker.SetActive(false);
        }
        else
        {
            rightTracker.SetActive(false);
            leftTracker.SetActive(true);
        }

        switch (LevelManager.Instance.currStation)
        {
            case StationColor.Red:
                SetSliderValues(100f, 2f);
                break;
            case StationColor.Pink:
                SetSliderValues(84f, 18f);
                break;
            case StationColor.Orange:
                SetSliderValues(66f, 35f);
                break;
            case StationColor.Yellow:
                SetSliderValues(50f, 50f);
                break;
            case StationColor.Green:
                SetSliderValues(35f, 66f);
                break;
            case StationColor.Blue:
                SetSliderValues(18f, 84f);
                break;
            case StationColor.Violet:
                SetSliderValues(2f, 100f);
                break;
        }
    }

    public IEnumerator TransitionColor(Color currColor, Color targetColor)
    {
        float colorTransitionDuration = LevelManager.Instance.travelPhaseTimer + (LevelManager.Instance.decelerationTimer * 2f);
        float elapsedTime = 0f;
        Slider activeSlider = LevelManager.Instance.currDirection == TrainDirection.Right
            ? rightTrackerSlider
            : leftTrackerSlider;

        Image fillImage = activeSlider.fillRect.GetComponent<Image>();
        if (fillImage == null) yield break;

        while (elapsedTime < colorTransitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / colorTransitionDuration);

            // Lerp and apply the color
            Color currentColor = Color.Lerp(currColor, targetColor, t);
            fillImage.color = currentColor;
            LevelManager.Instance.roofMaterial.color = currentColor;
            LevelManager.Instance.stationMaterial.color = currentColor;

            // Optional: Update LevelManager's reference if needed
            LevelManager.Instance.currStationColor = currentColor;

            yield return null;
        }

        // Ensure final color is exact
        fillImage.color = targetColor;
        LevelManager.Instance.roofMaterial.color = targetColor;
        LevelManager.Instance.stationMaterial.color = targetColor;
        LevelManager.Instance.currStationColor = targetColor;
    }

    public void SetStationLED(StationColor station, bool isBlinking)
    {
        foreach (var led in StationLED)
        {
            led.gameObject.SetActive(false);
        }

        switch (station)
        {
            case StationColor.Red:
            default:
                StationLED[0].SetActive(true);
                StationLED[0].GetComponent<LEDBlink>().isBlinking = isBlinking;
                break;
            case StationColor.Pink:
                StationLED[1].SetActive(true);
                StationLED[1].GetComponent<LEDBlink>().isBlinking = isBlinking;
                break;
            case StationColor.Orange:
                StationLED[2].SetActive(true);
                StationLED[2].GetComponent<LEDBlink>().isBlinking = isBlinking;
                break;
            case StationColor.Yellow:
                StationLED[3].SetActive(true);
                StationLED[3].GetComponent<LEDBlink>().isBlinking = isBlinking;
                break;
            case StationColor.Green:
                StationLED[4].SetActive(true);
                StationLED[4].GetComponent<LEDBlink>().isBlinking = isBlinking;
                break;
            case StationColor.Blue:
                StationLED[5].SetActive(true);
                StationLED[5].GetComponent<LEDBlink>().isBlinking = isBlinking;
                break;
            case StationColor.Violet:
                StationLED[6].SetActive(true);
                StationLED[6].GetComponent<LEDBlink>().isBlinking = isBlinking;
                break;
        }
    }

    private void SetSliderValues(float targetLeftValue, float targetRightValue)
    {
        if (sliderCoroutine != null)
        {
            StopCoroutine(sliderCoroutine);
        }

        sliderCoroutine = StartCoroutine(LerpSliderValues(targetRightValue, targetLeftValue, 10f));
    }

    private IEnumerator LerpSliderValues(float targetRightValue, float targetLeftValue, float duration)
    {
        float time = 0f;
        float startRightValue = rightTrackerSlider.value;
        float startLeftValue = leftTrackerSlider.value;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / duration);

            if (LevelManager.Instance.currDirection == TrainDirection.Right)
            {
                rightTrackerSlider.value = Mathf.Lerp(startRightValue, targetRightValue, t);
            }
            else if (LevelManager.Instance.currDirection == TrainDirection.Left)
            {
                leftTrackerSlider.value = Mathf.Lerp(startLeftValue, targetLeftValue, t);
            }

            yield return null;
        }

        // Ensure final values are exact
        if (LevelManager.Instance.currDirection == TrainDirection.Right)
        {
            rightTrackerSlider.value = targetRightValue;
        }
        else if (LevelManager.Instance.currDirection == TrainDirection.Left)
        {
            leftTrackerSlider.value = targetLeftValue;
        }
    }
    #endregion

    #region GAME OVER
    public void ActivateGameoverPanel()
    {
        gameOverCanvas.SetActive(true);
        SetGameOverScoreText(LevelManager.Instance.currentScore);
        LevelManager.Instance.StopGameFlow();
        AudioManager.Instance.PauseAudio();
    }
    #endregion

    #region BUTTON FUNCTIONS
    public void OnPauseButtonClicked()
    {
        if (pausePanel != null) pausePanel.SetActive(true);
        if (_dropZoneObj != null) _dropZoneObj.SetActive(false);
        _isQuitting = false;
        _isSettings = false;
        if (confirmationPanel != null) confirmationPanel.SetActive(_isQuitting);
        if (_settingsObj != null) _settingsObj.SetActive(_isSettings);

        Time.timeScale = 0f;
        isPaused = true;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PauseAudio();
    }

    public void ToggleSettingsWindow()
    {
        _isSettings = !_isSettings;

        if (pausePanel != null) pausePanel.SetActive(false);
        if (_settingsObj != null) _settingsObj.SetActive(_isSettings);
    }

    public void ToggleQuitWindow()
    {
        _isQuitting = !_isQuitting;

        if (pausePanel != null) pausePanel.SetActive(false);
        if (confirmationPanel != null) confirmationPanel.SetActive(_isQuitting);
    }

    public void OnQuitButtonClicked()
    {
        if (SceneManagement.Instance == null) return;

        Time.timeScale = 1f;
        isPaused = false;

        SceneManagement.Instance.LoadMenuScene();
    }

    public void OnResumeButtonClicked()
    {
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;

        if (AudioManager.Instance != null)
            AudioManager.Instance.ResumeAudio();
    }
    #endregion

    #region CARD
    public void SetCardShopState(bool state)
    {
        _shopCanvas.SetActive(state);
    }

    public void OnStartDayButtonClicked()
    {
        LevelManager.Instance.isEndStation = false;
        SetCardShopState(false);
    }

    public void OnPurchaseButtonClicked()
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxClips[5], false);
        AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxClips[2], false);
    }

    private IEnumerator DeactivateDropZone()
    {
        DropZone dz =_dropZoneObj.GetComponent<DropZone>();
        dz.isActivated = false;

        yield return new WaitUntil(() => dz.alpha <= 0);
        _dropZoneObj.SetActive(false);
    }
    #endregion
}

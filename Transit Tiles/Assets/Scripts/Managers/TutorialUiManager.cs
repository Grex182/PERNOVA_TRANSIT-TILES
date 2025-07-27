using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialUiManager : MonoBehaviour
{
    public static TutorialUiManager Instance;

    [Header("Pause")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject confirmationPanel;
    public bool isPaused = false;
    private bool _isQuitting = false;

    [Header("Public Rating")]
    [SerializeField] private List<GameObject> stars = new List<GameObject>();

    [Header("Phase Timer")]
    [SerializeField] private TextMeshProUGUI currPhaseText;
    [SerializeField] private GameObject[] timerSegments = new GameObject[16];
    [SerializeField] private GameObject[] StationLED = new GameObject[7];

    [Header("Score")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] GameObject scoreFloatieParent;
    [SerializeField] GameObject floatiePrefab;

    [Header("Station Tracker")]
    private Coroutine sliderCoroutine;
    [SerializeField] private GameObject rightTracker;
    [SerializeField] private Slider rightTrackerSlider;
    public Coroutine colorTransitionCoroutine;

    [Header("Card Shop")]
    [SerializeField] private GameObject _shopCanvas;

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
    }

    private void Start()
    {
        InitializeUi();
    }

    public void InitializeUi()
    {
        _dropZoneObj.SetActive(false);
        _shopCanvas.SetActive(false);

        isPaused = false;
        _isQuitting = false;

        foreach (var segment in timerSegments)
        {
            segment.SetActive(true);
        }

        // Set Slider Values
        rightTrackerSlider.value = 2f;
        SetStationLED(StationColor.Red, false);

        // Set Slider Direction
        rightTracker.SetActive(true);

        pausePanel.SetActive(false);
        confirmationPanel.SetActive(false);
    }

    private void Update()
    {
        if (TutorialManager.Instance.currState == MovementState.Card && !isPaused)
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
        GameObject floatie = Instantiate(floatiePrefab, scoreFloatieParent.transform.position, scoreFloatieParent.transform.rotation, scoreFloatieParent.transform);
        ScoreFloatieScript floatScript = floatie.GetComponent<ScoreFloatieScript>();
        floatScript.score = score;
    }
    #endregion

    #region STATION TRACKER
    public void SetTrackerSlider()
    {
        if (TutorialManager.Instance.currDirection == TrainDirection.Right)
        {
            rightTracker.SetActive(true);
        }

        switch (TutorialManager.Instance.currStation)
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
        float colorTransitionDuration = TutorialManager.Instance.travelPhaseTimer + (TutorialManager.Instance.decelerationTimer * 2f);
        float elapsedTime = 0f;

        Image fillImage = rightTrackerSlider.fillRect.GetComponent<Image>();
        if (fillImage == null) yield break;

        while (elapsedTime < colorTransitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / colorTransitionDuration);

            // Lerp and apply the color
            Color currentColor = Color.Lerp(currColor, targetColor, t);
            fillImage.color = currentColor;
            TutorialManager.Instance.roofMaterial.color = currentColor;
            TutorialManager.Instance.stationMaterial.color = currentColor;

            // Optional: Update LevelManager's reference if needed
            TutorialManager.Instance.currStationColor = currentColor;

            yield return null;
        }

        // Ensure final color is exact
        fillImage.color = targetColor;
        TutorialManager.Instance.roofMaterial.color = targetColor;
        TutorialManager.Instance.stationMaterial.color = targetColor;
        TutorialManager.Instance.currStationColor = targetColor;
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

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / duration);

            rightTrackerSlider.value = Mathf.Lerp(startRightValue, targetRightValue, t);

            yield return null;
        }

        // Ensure final values are exact
        rightTrackerSlider.value = targetRightValue;
    }
    #endregion

    #region BUTTON FUNCTIONS
    public void OnPauseButtonClicked()
    {
        if (pausePanel != null) pausePanel.SetActive(true);
        if (_dropZoneObj != null) _dropZoneObj.SetActive(false);
        _isQuitting = false;
        if (confirmationPanel != null) confirmationPanel.SetActive(_isQuitting);

        Time.timeScale = 0f;
        isPaused = true;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PauseAudio();
    }

    public void ToggleSettingsWindow()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
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
        TutorialManager.Instance.isEndStation = false;
        SetCardShopState(false);
    }

    public void OnPurchaseButtonClicked()
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxClips[5], false);
        AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxClips[2], false);
    }

    private IEnumerator DeactivateDropZone()
    {
        DropZone dz = _dropZoneObj.GetComponent<DropZone>();
        dz.isActivated = false;

        yield return new WaitUntil(() => dz.alpha <= 0);
        _dropZoneObj.SetActive(false);
    }
    #endregion
}

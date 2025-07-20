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
    public bool isPaused = false;

    [Header("Public Rating")]
    [SerializeField] private List<GameObject> stars = new List<GameObject>();

    [Header("Phase Timer")]
    [SerializeField] private TextMeshProUGUI currPhaseText;
    [SerializeField] private GameObject[] timerSegments = new GameObject[16];

    [Header("Score")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TMP_Text scoreGameOverText;

    [Header("Station Tracker")]
    private Coroutine sliderCoroutine;
    [SerializeField] private GameObject rightTracker;
    [SerializeField] private GameObject leftTracker;
    [SerializeField] private Slider rightTrackerSlider;
    [SerializeField] private Slider leftTrackerSlider;
    public Coroutine colorTransitionCoroutine;

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
        isPaused = false;

        foreach (var segment in timerSegments)
        {
            segment.SetActive(true);
        }

        // Set Slider Values
        rightTrackerSlider.value = 2f;
        leftTrackerSlider.value = 2f;

        // Set Slider Direction
        rightTracker.SetActive(true);
        leftTracker.SetActive(false);

        pausePanel.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !pausePanel.activeSelf)
        {
            OnPauseButtonClicked();
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
                currPhaseText.text = "Departing Station"; // Card Phase
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
                SetSliderValues(89f, 20f);
                break;
            case StationColor.Orange:
                SetSliderValues(70f, 37f);
                break;
            case StationColor.Yellow:
                SetSliderValues(54f, 54f);
                break;
            case StationColor.Green:
                SetSliderValues(37f, 69f);
                break;
            case StationColor.Blue:
                SetSliderValues(19f, 87f);
                break;
            case StationColor.Violet:
                SetSliderValues(2f, 100f);
                break;
        }
    }

    public IEnumerator TransitionColor(Color currColor, Color targetColor)
    {
        float colorTransitionDuration = LevelManager.Instance._travelPhaseTimer;
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

    }
    #endregion

    #region BUTTON FUNCTIONS
    public void OnPauseButtonClicked()
    {
        pausePanel.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void OnQuitButtonClicked()
    {
        // Warning window: Warning, All progress in this run will be lost. Are you sure you want to exit to the main menu?
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    public void OnResumeButtonClicked()
    {
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }
    #endregion
}

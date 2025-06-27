using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UiManager : Singleton<UiManager>
{
    [Header("Public Rating")]
    [SerializeField] private List<GameObject> stars = new List<GameObject>(); // Public Rating

    [Header("Phase Timer")]
    [SerializeField] private TextMeshProUGUI currPhaseText;
    [SerializeField] private GameObject[] timerSegments = new GameObject[16];

    [Header("Score")]
    [SerializeField] private TextMeshProUGUI scoreText;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (GameManager.Instance.gameState == GameState.GameInit)
        {
            Initialize();
        }
    }

    private void Initialize()
    {
        foreach (var segment in timerSegments)
        {
            segment.SetActive(true);
        }
    }

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
                currPhaseText.text = "Doors Opened"; // Station Phase
                break;
            case MovementState.Card:
                currPhaseText.text = "Doors Closed"; // Card Phase
                break;
            case MovementState.Travel:
                currPhaseText.text = "Approaching next Station"; // Travel Phase
                break;
            case MovementState.Accelerate:
                currPhaseText.text = "Departing Station";
                break;
            case MovementState.Decelerate:
                currPhaseText.text = "Arriving next Station";
                break;
        }
    }
    #endregion

    #region SCORE
    public void SetScoreText(int score)
    {
        scoreText.text = score.ToString();
    }
    #endregion

    #region GAME OVER
    public void ActivateGameoverPanel()
    {

    }
    #endregion
}

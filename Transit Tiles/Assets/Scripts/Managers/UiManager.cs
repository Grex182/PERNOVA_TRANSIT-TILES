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
    [SerializeField] private GameObject[] timerSegments;

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

        StartCoroutine(StartPhaseTimer(20f));
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

        for (int i = 0; i < timerSegments.Length; i++)
        {
            timerSegments[i].SetActive(true);
        }

        for (int i = 0; i < timerSegments.Length; i++)
        {
            yield return new WaitForSeconds(segmentDuration);
            time -= segmentDuration;
            timerSegments[i].SetActive(false);
        }
    }

    public void SetPhaseText(int phase)
    {
        switch (phase)
        {
            case 0:
                currPhaseText.text = "Arrived at Station"; // Station Phase
                break;
            case 1:
                currPhaseText.text = "Departing Station"; // Card Phase
                break;
            case 2:
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
    #endregion

    #region GAME OVER
    public void ActivateGameoverPanel()
    {

    }
    #endregion
}

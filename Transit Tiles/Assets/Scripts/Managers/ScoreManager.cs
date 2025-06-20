using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreManager : Singleton<ScoreManager>
{
    [Header("Player Score")]
    [SerializeField] public int currentScore = 0;
    [SerializeField] public int baseScoreValue = 100;

    [SerializeField] private TMP_Text scoreNumber;

    private void Start()
    {
        GameManager.Instance.ScoreManager = this;

        scoreNumber.text = $"Score: {currentScore}";
    }

    public void AddScore()
    {
        //Happy Standard: 10 points | Happy Priority: 50 points | Landing the station: 100 points (When player lands on a station, they get 100 points, the passengers are just plus points)
        currentScore += 100;

        scoreNumber.text = $"Score: {currentScore}";
        Debug.Log("Current Score: " + currentScore);
        //Add the update text line under the score line
    }
}

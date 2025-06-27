using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : Singleton<LevelManager>
{
    // Handle passenger spawning, Game flow, Board
    [Header("Public Rating")]
    [SerializeField] private float maxPublicRating = 5.0f;
    [SerializeField] private float basePublicRating = 2.5f;
    [SerializeField] private float currPublicRating;

    [Header("Player Score")]
    [SerializeField] public int currentScore = 0;
    [SerializeField] public int baseScoreValue = 100;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currPublicRating -= 2.5f;
            ReducePublicRating();
        }
    }

    public void InitializeLevel()
    {
        // PUBLIC RATING
        currPublicRating = basePublicRating;

        // Score
        currentScore = 0;
    }

    #region PUBLIC RATING
    public void AddPublicRating(float value) // Standard = 0.5f, Special = 1.0f
    {
        currPublicRating = Mathf.Clamp(value, 0f, maxPublicRating);
        Debug.Log("Public Rating Increased! CurrentPublicRating: " + currPublicRating);
    }

    public void ReducePublicRating()
    {
        currPublicRating = Mathf.Clamp(currPublicRating - 0.5f, 0f, maxPublicRating);
        Debug.Log("Public Rating Decreased. CurrentPublicRating: " + currPublicRating);

        //Angry Standard: -0.5 PR | Angry Priority: -1 PR
        if (currPublicRating <= 0)
        {
            currPublicRating = 0;
            UiManager.Instance.ActivateGameoverPanel();
            Time.timeScale = 0f;
        }
    }
    #endregion

    #region SCORE
    public void AddScore()
    {
        //Happy Standard: 10 points | Happy Priority: 50 points | Landing the station: 100 points (When player lands on a station, they get 100 points, the passengers are just plus points)
        currentScore += 100;

        UiManager.Instance.SetScoreText(currentScore);
        Debug.Log("Current Score: " + currentScore);
        //Add the update text line under the score line
    }
    #endregion
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PublicRatingManager : MonoBehaviour
{
    [Header("Public Rating")]
    [SerializeField] private float maxPublicRating = 5.0f;
    [SerializeField] private float startingPublicRating = 2.5f;
    [SerializeField] private float currentPublicRating;
    [SerializeField] private GameObject gameOverText;
    [SerializeField] private TMP_Text publicRatingNumber;

    private void Start()
    {
        GameManager.Instance.PublicRatingManager = this;

        currentPublicRating = startingPublicRating;
        publicRatingNumber.text = $"Public Rating: {currentPublicRating}";
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentPublicRating -= 2.5f;
            ReducePublicRating();
        }
    }

    public void AddPublicRating()
    {
        //Angry Standard: +0.5 PR | Angry Priority: +1 PR
        if (currentPublicRating >= maxPublicRating)
        {
            Debug.Log("Okay no more PR for you");
        }
        else
        {
            currentPublicRating += 0.5f;

            publicRatingNumber.text = $"Public Rating: {currentPublicRating}";
            Debug.Log("Public Rating Increased! CurrentPublicRating: " + currentPublicRating);
        }
    }

    public void ReducePublicRating()
    {
        currentPublicRating = Mathf.Clamp(currentPublicRating - 0.5f, 0f, maxPublicRating);

        publicRatingNumber.text = $"Public Rating: {currentPublicRating}";
        Debug.Log("Public Rating Decreased. CurrentPublicRating: " + currentPublicRating);

        //Angry Standard: -0.5 PR | Angry Priority: -1 PR
        if (currentPublicRating <= 0)
        {
            Debug.Log("You Dead");
            gameOverText.SetActive(true);
            currentPublicRating = 0;
            Time.timeScale = 0f;
        }
    }
}

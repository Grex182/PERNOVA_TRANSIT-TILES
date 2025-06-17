using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TemporaryPauseScript : MonoBehaviour
{
    [SerializeField] private GameObject pauseText;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (pauseText.activeSelf)
            {
                pauseText.SetActive(false);
                Time.timeScale = 1f;
            }
            else
            {
                pauseText.SetActive(true);
                Time.timeScale = 0f;
            }
        }
    }
}

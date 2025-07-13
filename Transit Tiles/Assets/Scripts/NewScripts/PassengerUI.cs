using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.ShaderKeywordFilter;
using UnityEngine;
using UnityEngine.UI;

public class PassengerUI : MonoBehaviour
{
    [SerializeField] private GameObject canvasObj;
    [SerializeField] private Canvas canvas;

    [Header("Mood UI")]
    [SerializeField] private GameObject moodletObj;
    [SerializeField] private Image moodImg;
    [SerializeField] private Sprite[] moodSprites; // [0] = Angry, [1] = Neutral, [2] = Happy
    public Coroutine moodletCoroutine;

    // Start is called before the first frame update
    void Start()
    {
        canvas.worldCamera = Camera.main;
    }

    public IEnumerator ActivateMoodlet()
    {
        moodletObj.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        moodletObj.SetActive(false);
    }

    public void ChangeMoodImg(int moodValue)
    {
        switch (moodValue)
        {
            case 1: // Angry
                moodImg.sprite = moodSprites[0];
                break;
            case 2: // Neutral
                moodImg.sprite = moodSprites[1];
                break;
            case 3: // Happy
                moodImg.sprite = moodSprites[2];
                break;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        Initialize();
    }

    private void LateUpdate()
    {
        // Ensure UI always faces camera
        ResetCanvasRotation();
    }

    private void Initialize()
    {
        canvas.worldCamera = Camera.main;

        // Mood UI setup
        moodImg.sprite = moodSprites[2];
        moodletObj.SetActive(false);
        moodletCoroutine = null;
    }

    #region MOODLET
    public void SetMoodletState(bool isActive)
    {
        moodletObj.SetActive(isActive);
    }

    public void ChangeMoodImg(int moodValue)
    {
        switch (moodValue)
        {
            // Angry
            case 1: moodImg.sprite = moodSprites[0]; break;
            // Neutral
            case 2: moodImg.sprite = moodSprites[1]; break;
            // Happy
            case 3: moodImg.sprite = moodSprites[2]; break;
        }

        if (moodletCoroutine != null)
        {
            StopCoroutine(moodletCoroutine);
        }

        moodletCoroutine = StartCoroutine(ActivateMoodletAnimation());
    }

    private IEnumerator ActivateMoodletAnimation()
    {
        moodletObj.SetActive(true);
        Color originalColor = moodImg.color;
        originalColor.a = 1f;
        moodImg.color = originalColor;

        float fadeDuration = 1.0f;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            Color newColor = moodImg.color;
            newColor.a = alpha;
            moodImg.color = newColor;

            yield return null;
        }

        moodletObj.SetActive(false);
    }
    #endregion

    private void ResetCanvasRotation()
    {
        if (canvas.worldCamera == null) return;

        canvasObj.transform.LookAt(canvasObj.transform.position + canvas.worldCamera.transform.forward,
                                   canvas.worldCamera.transform.up);
    }
}

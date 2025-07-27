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

    public bool animationActive = false;

    [Header("Color Blind UI")]
    [SerializeField] private GameObject colorBlindCanvas;
    [SerializeField] private Image stationImg;
    [SerializeField] private Sprite[] stationSprites;

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
        SetColorblindCanvasState();

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
            animationActive = false;
        }

        moodletCoroutine = StartCoroutine(ActivateMoodletAnimation());
    }

    private IEnumerator ActivateMoodletAnimation()
    {
        animationActive = true; 
        moodletObj.SetActive(true);

        Color originalColor = moodImg.color;
        originalColor.a = 1f;
        moodImg.color = originalColor;

        const float fadeDuration = 2.0f;
        float elapsedTime = 0f;

        yield return new WaitForSeconds(1f); // Initial delay before fading out

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);

            moodImg.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }

        moodletObj.SetActive(false);
        moodImg.color = originalColor;
        animationActive = false;
    }
    #endregion

    private void ResetCanvasRotation()
    {
        if (canvas.worldCamera == null) return;

        canvasObj.transform.LookAt(canvasObj.transform.position + canvas.worldCamera.transform.forward,
                                   canvas.worldCamera.transform.up);

        if (colorBlindCanvas == null) { return; }
        colorBlindCanvas.transform.LookAt(colorBlindCanvas.transform.position + canvas.worldCamera.transform.forward,
                                   canvas.worldCamera.transform.up);
    }

    public void SetColorblindCanvasState()
    {
        if (GameManager.Instance.colorblindMode == ColorblindMode.Enabled)
        {
            colorBlindCanvas.SetActive(true);
        }
        else
        {
            colorBlindCanvas.SetActive(false);
        }
    }

    public void SetColorblindCanvas(StationColor assignedColor)
    {
        switch (assignedColor)
        {
            case StationColor.Red:
                stationImg.sprite = stationSprites[0];
                break;

            case StationColor.Pink:
                stationImg.sprite = stationSprites[1];
                break;

            case StationColor.Orange:
                stationImg.sprite = stationSprites[2];
                break;

            case StationColor.Yellow:
                stationImg.sprite = stationSprites[3];
                break;

            case StationColor.Green:
                stationImg.sprite = stationSprites[4];
                break;

            case StationColor.Blue:
                stationImg.sprite = stationSprites[5];
                break;

            case StationColor.Violet:
                stationImg.sprite = stationSprites[6];
                break;
        }
    }
}

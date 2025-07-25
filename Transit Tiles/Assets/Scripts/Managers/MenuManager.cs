using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject _startWindowObj;
    private bool _isStartPressed = false;
    private bool _canLoadStart = false;
    private bool _canLoadTutorial = false;

    [SerializeField] private GameObject _optionsWindowObj;
    [SerializeField] private GameObject _audioObj;
    [SerializeField] private GameObject _controlsObj;
    private bool _isOptionsPressed = false;

    [SerializeField] private GameObject _creditsWindowObj;
    private bool _isCreditsPressed = false;

    private Coroutine announcerCoroutine;

    private void Awake()
    {
        _startWindowObj.SetActive(false);
        _isStartPressed = false;
        _canLoadStart = false;
        _canLoadTutorial = false;

        _optionsWindowObj.SetActive(false);
        _audioObj.SetActive(false);
        _controlsObj.SetActive(false);
        _isOptionsPressed = false;

        _creditsWindowObj.SetActive(false);
        _isCreditsPressed = false;
    }

    private void Start()
    {
        announcerCoroutine = StartCoroutine(PlayAnnouncerBg());
    }

    #region START BUTTON
    public void OnClickStartButton()
    {
        if (_isOptionsPressed)
        {
            _isOptionsPressed = false;
            _optionsWindowObj.SetActive(_isOptionsPressed);
        }

        if (_isCreditsPressed)
        {
            _isCreditsPressed = false;
            _creditsWindowObj.SetActive(_isCreditsPressed);
        }

        _isStartPressed = !_isStartPressed;
        _startWindowObj.SetActive(_isStartPressed);
    }

    public void LoadStartScene()
    {
        if (_canLoadStart) return;

        _canLoadStart = true;
        StopCoroutine(announcerCoroutine);

        AudioManager.Instance.StopSFX();
        AudioManager.Instance.StopVoice();

        SceneManagement.Instance.LoadGameScene();
    }

    public void LoadTutorialScene()
    {
        if (_canLoadTutorial) return;

        _canLoadTutorial = true;
        StopCoroutine(announcerCoroutine);

        AudioManager.Instance.StopSFX();
        AudioManager.Instance.StopVoice();

        SceneManagement.Instance.LoadTutorialScene();
    }
    #endregion

    #region OPTIONS BUTTON
    public void OnClickOptionsButton()
    {
        if (_isStartPressed)
        {
            _isStartPressed = false;
            _startWindowObj.SetActive(_isStartPressed);
        }

        if (_isCreditsPressed)
        {
            _isCreditsPressed = false;
            _creditsWindowObj.SetActive(_isCreditsPressed);
        }

        _isOptionsPressed = !_isOptionsPressed;
        _optionsWindowObj.SetActive(_isOptionsPressed);
        OnClickAudio();
    }

    public void OnClickAudio()
    {
        _audioObj.SetActive(true);
        _controlsObj.SetActive(false);
    }

    public void OnClickControls()
    {
        _audioObj.SetActive(false);
        _controlsObj.SetActive(true);
    }
    #endregion

    #region CREDITS BUTTON
    public void OnClickCreditsButton()
    {
        if (_isStartPressed)
        {
            _isStartPressed = false;
            _startWindowObj.SetActive(_isStartPressed);
        }

        if (_isOptionsPressed)
        {
            _isOptionsPressed = false;
            _optionsWindowObj.SetActive(_isOptionsPressed);
        }

        _isCreditsPressed = !_isCreditsPressed;
        _creditsWindowObj.SetActive(_isCreditsPressed);
    }
    #endregion

    public void OnButtonClick()
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxClips[10], false);
    }

    #region BACKGROUND MUSIC
    public IEnumerator PlayAnnouncerBg()
    {
        yield return new WaitForSeconds(3f);

        while (!_canLoadStart)
        {
            for (int i = 0; i < 9; i++)
            {
                AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxClips[1], false);
                yield return new WaitForSeconds(AudioManager.Instance.sfxClips[1].length);

                AudioManager.Instance.PlayAnnouncement(i);
                yield return new WaitForSeconds(AudioManager.Instance.announcerVoiceClips[i].length + 3f);

            }
        }
    }
    #endregion
}

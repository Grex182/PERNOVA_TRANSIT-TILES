using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject _startWindowObj;
    private bool _isStartPressed = false;
    private bool _canLoadStart = false;
    private bool _canLoadTutorial = false;
    [SerializeField] private RectTransform _ticketRect;
    [SerializeField] private TextMeshProUGUI _ticketText;
    [SerializeField] private float _ticketSpeed;
    [SerializeField] private Button _yesButton;
    [SerializeField] private Button _noButton;

    [SerializeField] private GameObject _optionsWindowObj;
    [SerializeField] private GameObject _audioObj;
    [SerializeField] private GameObject _preferencesObj;
    [SerializeField] private Slider _selectionSlider;
    [SerializeField] private Slider _colorblindSlider;
    [SerializeField] private GameObject _controlsObj;
    [SerializeField] private Animator _selectionAnimator;
    [SerializeField] private GameObject _colorblindIcons;

    private bool _isOptionsPressed = false;

    [SerializeField] private GameObject _creditsWindowObj;
    private bool _isCreditsPressed = false;

    private Coroutine announcerCoroutine;

    public Slider bgmVolumeSlider;
    public Slider sfxVolumeSlider;

    private void Awake()
    {
        _startWindowObj.SetActive(false);
        _isStartPressed = false;
        _canLoadStart = false;
        _canLoadTutorial = false;
        _selectionAnimator.SetBool("isToggleMode", true);

        _optionsWindowObj.SetActive(false);
        _preferencesObj.SetActive(false);
        _controlsObj.SetActive(false);
        _isOptionsPressed = false;

        _creditsWindowObj.SetActive(false);
        _isCreditsPressed = false;
    }

    private void Start()
    {
        AudioManager.Instance.musicVolume = 0.5f;
        AudioManager.Instance.sfxVolume = 0.5f;

        bgmVolumeSlider.value = AudioManager.Instance.musicVolume;
        sfxVolumeSlider.value = AudioManager.Instance.sfxVolume;

        _audioObj.SetActive(false);

        announcerCoroutine = StartCoroutine(PlayAnnouncerBg());
        _colorblindSlider.onValueChanged.AddListener(OnColorblindSliderValueChanged);
        _selectionSlider.onValueChanged.AddListener(OnSelectionSliderValueChanged);

        bgmVolumeSlider.onValueChanged.AddListener((value) => {
            AudioManager.Instance.ChangeBgmVolume(value);
        });

        sfxVolumeSlider.onValueChanged.AddListener((value) => {
            AudioManager.Instance.ChangeSfxVolume(value);
        });
    }

    private void OnSelectionSliderValueChanged(float value)
    {
        GameManager.Instance.SetSelectionMode(value);
        bool toggleMode = (Mathf.RoundToInt(value) == 0);
        _selectionAnimator.SetBool("isToggleMode", toggleMode);
    }

    private void OnColorblindSliderValueChanged(float value)
    {
        GameManager.Instance.SetColorblindMode(value);
        bool toggleMode = (Mathf.RoundToInt(value) != 0);
        _colorblindIcons.SetActive(toggleMode);
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
        AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxClips[11],false);
        _canLoadStart = true;
        _yesButton.interactable = false;
        _ticketText.text = "Game";

        StartCoroutine(PrintGameTicket());
    }

    public void LoadTutorialScene()
    {
        if (_canLoadTutorial) return;
        AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxClips[11], false);
        _canLoadTutorial = true;
        _noButton.interactable = false;
        _ticketText.text = "Tutorial";

        StartCoroutine(PrintTutorialTicket());
    }

    private IEnumerator PrintGameTicket()
    {
        float height = 150f;
        bool hasPrinted = false;
        while (height < 450f)
        {
            height += Time.deltaTime * _ticketSpeed;
            _ticketRect.sizeDelta = new Vector2(320f, height);
            yield return null;
        }
        hasPrinted = true;
        
        
        yield return new WaitUntil(()=> hasPrinted);

        bool hasPrinted2 = false;
        while (height < 480f)
        {
            height += Time.deltaTime * _ticketSpeed / 5f;
            _ticketRect.sizeDelta = new Vector2(320f, height);
            yield return null;
        }
        hasPrinted2 = true;


        yield return new WaitUntil(() => hasPrinted2);

        yield return new WaitForSeconds(1.5f);

        StopCoroutine(announcerCoroutine);

        AudioManager.Instance.StopSFX();
        AudioManager.Instance.StopVoice();

        SceneManagement.Instance.LoadGameScene();
    }

    private IEnumerator PrintTutorialTicket()
    {
        float height = 150f;
        bool hasPrinted = false;
        while (height < 450f)
        {
            height += Time.deltaTime * _ticketSpeed;
            _ticketRect.sizeDelta = new Vector2(320f, height);
            yield return null;
        }
        hasPrinted = true;


        yield return new WaitUntil(() => hasPrinted);

        bool hasPrinted2 = false;
        while (height < 480f)
        {
            height += Time.deltaTime * _ticketSpeed / 5f;
            _ticketRect.sizeDelta = new Vector2(320f, height);
            yield return null;
        }
        hasPrinted2 = true;


        yield return new WaitUntil(() => hasPrinted2);

        yield return new WaitForSeconds(1.5f);
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
        _preferencesObj.SetActive(false);
        _controlsObj.SetActive(false);
    }

    public void OnClickPreferences()
    {
        _audioObj.SetActive(false);
        _preferencesObj.SetActive(true);
        _controlsObj.SetActive(false);
    }

    public void OnClickControls()
    {
        _audioObj.SetActive(false);
        _preferencesObj.SetActive(false);
        _controlsObj.SetActive(true);
    }

    private void OnSelectionSliderChanged()
    {
        _selectionSlider.value = 0;
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

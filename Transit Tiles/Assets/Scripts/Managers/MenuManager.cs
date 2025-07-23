using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    private bool isPressed = false;
    private Coroutine announcerCoroutine;

    private void Awake()
    {
        isPressed = false;
    }

    private void Start()
    {
        announcerCoroutine = StartCoroutine(PlayAnnouncerBg());
    }

    public void OnClickStartButton()
    {
        if (isPressed) return;

        isPressed = true;

        StopCoroutine(announcerCoroutine);
        AudioManager.Instance.StopSFX();
        AudioManager.Instance.StopVoice();
        // NOTE: This depends on whether player skips tutorial or not.
        SceneManagement.Instance.LoadGameScene();
        //SceneManagement.Instance.LoadGameScene();
    }

    public void OnButtonClick()
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxClips[10], false);
    }

    public IEnumerator PlayAnnouncerBg()
    {
        yield return new WaitForSeconds(3f);

        while (!isPressed)
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
}

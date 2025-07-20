using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    private bool isPressed = false;

    private void Awake()
    {
        isPressed = false;
    }

    public void OnClickStartButton()
    {
        if (isPressed) return;

        isPressed = true;

        AudioManager.Instance.StopSFX();
        // NOTE: This depends on whether player skips tutorial or not.
        SceneManagement.Instance.LoadGameScene();
        //SceneManagement.Instance.LoadGameScene();
    }

    public void OnButtonClick()
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxClips[10], false);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public void OnClickPlay()
    {
        // NOTE: This depends on whether player skips tutorial or not.
        SceneManagement.Instance.LoadGameScene();
        //SceneManagement.Instance.LoadGameScene();
    }
}

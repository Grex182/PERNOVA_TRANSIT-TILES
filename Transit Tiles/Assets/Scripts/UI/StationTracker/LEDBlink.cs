using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LEDBlink : MonoBehaviour
{
    [SerializeField] private Image LedImg;
    private float _time = 0;
    public bool isBlinking = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isBlinking)
        {
            _time += Time.deltaTime;

            float blink = (Mathf.Cos( 4f * _time + Mathf.PI) + 1f)/2f;

            LedImg.color = new Color(1f, 1f, 1f, blink);
        }
        else
        {
            _time = 0;
            LedImg.color = new Color(1f, 1f, 1f, 1f);
        }
    }


}

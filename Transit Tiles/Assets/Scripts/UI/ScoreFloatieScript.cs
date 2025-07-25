using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreFloatieScript : MonoBehaviour
{
    public int score = 0;
    [SerializeField] private float _speed;
    [SerializeField] private float _timeOpaque;
    [SerializeField] private Color _colorGood;
    [SerializeField] private Color _colorBad;
    [SerializeField] private TextMeshProUGUI text;
    private string _scoreString;
    private float _alpha = 1f;


    // Start is called before the first frame update
    void Start()
    {
        _alpha = _timeOpaque + 1f;
        if (text != null )
        {
            text = gameObject.GetComponent<TextMeshProUGUI>();
        }
        

        if (score >= 0)
        {
            text.color = _colorGood;
            _scoreString = "+";
        }
        else
        {
            text.color = _colorBad;
            _scoreString = "";
        }

        _scoreString += score;
        text.text = _scoreString;
    }

    // Update is called once per frame
    void Update()
    {
        if (_alpha > 0f)
        {
            _alpha -= Time.deltaTime;
            float alpha = Mathf.Clamp(_alpha, 0f, 1f);

            text.color = DoFade(text.color, alpha);



            transform.localPosition += new Vector3(0, _speed, 0);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private Color DoFade(Color baseColor, float alpha)
    {
        Color newColor = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
        return newColor;
    }
}

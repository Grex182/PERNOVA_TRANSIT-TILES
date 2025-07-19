using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuMovingElements : MonoBehaviour
{
    [SerializeField] private Vector3 _startPos;
    [SerializeField] private float _endDistance;
    private float _coveredDistance;
    [SerializeField] private float _speed;
    [SerializeField] private Vector3 _direction;

    // Start is called before the first frame update
    void Start()
    {
        //Set Start Position
        transform.localPosition = _startPos;
        //Normalize Direction
        _direction = _direction.normalized;
        //Play Train SFX
        AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxClips[9], false);
    }

    // Update is called once per frame
    void Update()
    {
        if (_coveredDistance < _endDistance)
        {
            transform.localPosition += _direction * _speed * Time.deltaTime;
            _coveredDistance += _speed * Time.deltaTime;
        }
        else
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxClips[9], false);
            transform.localPosition = _startPos;
            _coveredDistance = 0f;
        }
    }
}

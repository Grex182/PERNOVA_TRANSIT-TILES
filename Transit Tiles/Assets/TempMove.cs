using System.Collections;
using System.Collections.Generic;
using UnityEditor.Timeline;
using UnityEngine;

public class TempMove : MonoBehaviour
{
    [SerializeField] private float _speedConst;
    [SerializeField] private float _acceleration;
    [SerializeField] private float _deceleration;
    [SerializeField] private float _elapsedTime = 0;
    [SerializeField] private float _distanceCovered;
    [SerializeField] private Vector3 startPosition;

    [SerializeField] private int _phase = 1; // 1 = acceleration, 2 = travel, 3 = deceleration

    [Header("Editable")]

    [SerializeField] private float _time;

    [SerializeField] private float _distTravel = 20f;
    [SerializeField] private float _distAcceleration = 10f;
    [SerializeField] private float _distDeceleration = 10f;


    // Start is called before the first frame update
    void Start()
    {
        GetMotionValues();
    }

    // Update is called once per frame
    void Update()
    {
        switch (_phase)
        {
            case 1: // Acceleration Phase
                AccelerationPhase();
                break;
            case 2: // Travel Phase
                TravelPhase();
                break;
            case 3: // Deceleration Phase
                DecelerationPhase();
                break;
            default:
                Debug.LogError("Invalid phase value: " + _phase);
                break;
        }
       
    }

    private void GetMotionValues()
    {
        //Get Constant Speed for Travel Phase
        _speedConst = _distTravel / _time;

        //acceleration formula
        _acceleration = Mathf.Pow(_speedConst,2f) / (2.0f * _distAcceleration);

        //deceleration formula
        _deceleration = Mathf.Pow(_speedConst, 2f) / (2.0f * _distDeceleration);

        //set starting position for movement
        startPosition = transform.position;

        
    }

    private void ResetTravel()
    {
        //set starting position for movement
        startPosition = transform.position;

        // Reset elapsed time and distance covered
        _elapsedTime = 0f;
        _distanceCovered = 0f;
    }


    private void AccelerationPhase()
    {
        if (_distanceCovered < _distAcceleration)
        {
            //Time used for calculations
            _elapsedTime += Time.deltaTime;

            //calculate distance covered over time
            _distanceCovered = 0.5f * _acceleration * (Mathf.Pow(_elapsedTime, 2));

            //Move object from initial position to target distance
            transform.position = startPosition + Vector3.right * _distanceCovered;

        }
        else
        {
            transform.position = startPosition + Vector3.right * _distAcceleration;

            ResetTravel(); // Reset elapsed time and distance covered for travel phase


            LevelManager.Instance.hasAccelerated = true;
        }
    }

    private void TravelPhase()
    {
        if (_distanceCovered < _distTravel)
        {
            //Time used for calculations
            _elapsedTime += Time.deltaTime;
            //calculate distance covered over time
            _distanceCovered = _speedConst * _elapsedTime;

            //Move object from initial position to target distance
            transform.position = startPosition + Vector3.right * _distanceCovered;
        }
        else
        {
            //Move object from initial position to exact target distance
            transform.position = startPosition + Vector3.right * _distTravel;

            ResetTravel(); // Reset elapsed time and distance covered for travel phase

            _phase = 3; // Move to deceleration phase
        }
    }

    private void DecelerationPhase()
    {
        if (_distanceCovered < _distDeceleration * 0.999f)
        {
            //Time used for calculations
            _elapsedTime += Time.deltaTime;

            //calculate distance covered over time
            _distanceCovered = (_speedConst * _elapsedTime) - (0.5f * _deceleration * (Mathf.Pow(_elapsedTime, 2)));

            //Move object from initial position to target distance
            transform.position = startPosition + Vector3.right * _distanceCovered;

        }
        else
        {
            transform.position = startPosition + Vector3.right * _distDeceleration;

            LevelManager.Instance.hasDecelerated = true;
        }
    }
}
